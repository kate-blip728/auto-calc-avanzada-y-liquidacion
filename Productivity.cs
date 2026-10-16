using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TodoAMano { public partial class MainForm {
    private sealed class Favorite {
        internal string Name, Kind, Category, Operation, Input;
        public override string ToString() { return Name; }
    }
    private readonly List<Favorite> favorites = new List<Favorite>();
    private readonly SortedDictionary<string,double> namedMemories = new SortedDictionary<string,double>(StringComparer.OrdinalIgnoreCase);
    private ToolStripMenuItem favoriteShortcuts;
    private double? lastAnswer;
    private static bool HasAnswerToken(string input) { return Regex.IsMatch(input??"",@"\bAns\b",RegexOptions.IgnoreCase); }
    private string ResolveAnswerToken(string input) {
        if(!HasAnswerToken(input)) return input;
        if(!lastAnswer.HasValue) throw new InvalidOperationException("Todavía no hay un resultado para Ans. Realiza primero un cálculo válido.");
        string number="("+lastAnswer.Value.ToString("R",CultureInfo.InvariantCulture)+")";
        return Regex.Replace(input,@"\bAns\b",number,RegexOptions.IgnoreCase);
    }
    private void ContinueCalculation() {
        if(!lastAnswer.HasValue) { AnnounceToScreenReader("Calcula primero un resultado para continuar."); return; }
        calcTemplate.SelectedIndex=0;
        calculadoraInput.Text="Ans ";
        calculatorTabs2824.SelectedTab=calculatorBasicPage2824;
        calculadoraInput.Focus(); calculadoraInput.SelectionStart=calculadoraInput.TextLength;
        AnnounceToScreenReader("Continúa desde "+lastAnswer.Value.ToString("0.############",SellerCulture2825)+". Escribe, por ejemplo, asterisco dos.");
    }
    private Dictionary<string,object> CaptureProductivityData() {
        var list=new ArrayList();
        foreach(var f in favorites) list.Add(new Dictionary<string,object>{{"name",f.Name},{"kind",f.Kind},{"category",f.Category},{"operation",f.Operation},{"input",f.Input}});
        var memories=new Dictionary<string,object>();
        foreach(var pair in namedMemories) memories[pair.Key]=pair.Value.ToString("R",CultureInfo.InvariantCulture);
        return new Dictionary<string,object>{{"favorites",list},{"memories",memories}};
    }
    private static void ValidateProductivityData(Dictionary<string,object> data) {
        if(data==null) throw new InvalidDataException("Datos de favoritos y memorias no válidos.");
        var list=BackupList(data,"favorites");
        var memories=data.ContainsKey("memories") ? data["memories"] as Dictionary<string,object> : null;
        if(list.Count>50 || memories==null || memories.Count>50) throw new InvalidDataException("Se admiten hasta 50 favoritos y 50 memorias.");
        var names=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach(var value in list) {
            var row=BackupRow(value); string name=BackupText(row,"name");
            if(string.IsNullOrWhiteSpace(name) || name.Length>80 || !names.Add(name)) throw new InvalidDataException("Nombre de favorito inválido o repetido.");
            string kind=BackupText(row,"kind");
            if(kind!="basic" && kind!="advanced") throw new InvalidDataException("Tipo de favorito desconocido.");
            BackupText(row,"category"); BackupText(row,"operation");
            if(BackupText(row,"input").Length>10000) throw new InvalidDataException("El cálculo guardado es demasiado largo.");
        }
        var memoryNames=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach(var pair in memories) {
            double number;
            if(string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Length>80 || !memoryNames.Add(pair.Key) || !(pair.Value is string) ||
                !double.TryParse((string)pair.Value,NumberStyles.Float,CultureInfo.InvariantCulture,out number) || double.IsNaN(number) || double.IsInfinity(number))
                throw new InvalidDataException("Memoria con nombre o valor inválido.");
        }
    }
    private void ApplyProductivityData(Dictionary<string,object> data) {
        ValidateProductivityData(data);
        favorites.Clear(); namedMemories.Clear();
        foreach(var value in BackupList(data,"favorites")) {
            var row=BackupRow(value);
            favorites.Add(new Favorite { Name=(string)row["name"],Kind=(string)row["kind"],Category=(string)row["category"],Operation=(string)row["operation"],Input=(string)row["input"] });
        }
        foreach(var pair in (Dictionary<string,object>)data["memories"]) namedMemories.Add(pair.Key,double.Parse((string)pair.Value,CultureInfo.InvariantCulture));
        RefreshFavoriteMenu();
    }
    private void LoadProductivityData() {
        string path=Path.Combine(dataDir,"favoritos-memorias.json");
        if(!File.Exists(path)) return;
        try { ApplyProductivityData(BackupSerializer().DeserializeObject(File.ReadAllText(path)) as Dictionary<string,object>); }
        catch { AnnounceToScreenReader("No se pudieron cargar los favoritos y memorias. El archivo original se conserva."); }
    }
    private void ChangeProductivity(Action change) {
        var previous=CaptureProductivityData();
        try {
            change();
            var data=CaptureProductivityData(); ValidateProductivityData(data);
            AtomicWriteCalculatorJson285(Path.Combine(dataDir,"favoritos-memorias.json"),BackupSerializer().Serialize(data));
            RefreshFavoriteMenu();
        } catch { ApplyProductivityData(previous); throw; }
    }
    private void InstallProductivityMenu(MenuStrip menu) {
        var item=new ToolStripMenuItem("Favoritos y memorias");
        item.DropDownItems.Add("Guardar cálculo como favorito…",null,delegate { SaveCurrentFavorite(); });
        favoriteShortcuts=new ToolStripMenuItem("Abrir favorito"); item.DropDownItems.Add(favoriteShortcuts);
        item.DropDownItems.Add("Gestionar favoritos…",null,delegate { ShowFavorites(); });
        item.DropDownItems.Add("Memorias con nombre…",null,delegate { ShowNamedMemories(); });
        item.DropDownItems.Add("Continuar desde el último resultado (Ans)",null,delegate { ContinueCalculation(); });
        menu.Items.Add(item); LoadProductivityData(); RefreshFavoriteMenu();
    }
    private void RefreshFavoriteMenu() {
        if(favoriteShortcuts==null) return;
        foreach(ToolStripItem item in favoriteShortcuts.DropDownItems.Cast<ToolStripItem>().ToArray()) item.Dispose();
        favoriteShortcuts.DropDownItems.Clear();
        for(int i=0;i<favorites.Count;i++) {
            var favorite=favorites[i]; var item=new ToolStripMenuItem(favorite.Name.Replace("&","&&"));
            item.Click+=delegate { OpenFavorite(favorite); };
            if(i<9) item.ShortcutKeys=Keys.Control|Keys.Shift|(Keys)((int)Keys.F1+i);
            favoriteShortcuts.DropDownItems.Add(item);
        }
        favoriteShortcuts.Enabled=favorites.Count>0;
    }
    private string AskProductivityName(string title,string current) {
        using(var dialog=new EscapeForm { Text=title,Width=460,Height=160,StartPosition=FormStartPosition.CenterParent }) {
            var label=new Label { Text="Nombre:",Dock=DockStyle.Top,AutoSize=true };
            var input=new TextBox { Text=current,MaxLength=80,Dock=DockStyle.Top,AccessibleName="Nombre" };
            var row=new FlowLayoutPanel { Dock=DockStyle.Bottom,AutoSize=true };
            var ok=new Button { Text="Guardar",DialogResult=DialogResult.OK,AutoSize=true };
            var cancel=new Button { Text="Cancelar",DialogResult=DialogResult.Cancel,AutoSize=true };
            row.Controls.Add(ok); row.Controls.Add(cancel); dialog.Controls.Add(input); dialog.Controls.Add(label); dialog.Controls.Add(row);
            dialog.AcceptButton=ok; dialog.CancelButton=cancel;
            return dialog.ShowDialog(this)==DialogResult.OK ? input.Text.Trim() : null;
        }
    }
    private void SaveCurrentFavorite() {
        bool basic=calculatorTabs2824.SelectedTab==calculatorBasicPage2824;
        if(!basic && calculatorTabs2824.SelectedTab!=calculatorAdvancedPage2824) { AnnounceToScreenReader("Selecciona un cálculo de Calcular o Práctica y científica."); return; }
        var favorite=new Favorite { Kind=basic?"basic":"advanced",Category=basic?"":advancedCategory2824.Text,
            Operation=basic?calcTemplate.Text:advancedOperation2824.Text,Input=basic?calculadoraInput.Text:advancedInput2824.Text };
        string name=AskProductivityName("Guardar favorito",favorite.Operation);
        if(name==null) return;
        try {
            favorite.Name=name;
            if(favorites.Any(f=>string.Equals(f.Name,name,StringComparison.OrdinalIgnoreCase))) { MessageBox.Show(this,"Ya existe ese nombre. Usa otro o renómbralo en Gestionar favoritos."); return; }
            ChangeProductivity(delegate { favorites.Add(favorite); });
            AnnounceToScreenReader("Favorito guardado. Los primeros nueve tienen atajos Control Mayúsculas F1 a F9.");
        } catch(Exception ex) { MessageBox.Show(this,ex.Message,"No se pudo guardar el favorito"); }
    }
    private void OpenFavorite(Favorite favorite) {
        if(favorite.Kind=="basic") {
            int index=calcTemplate.Items.IndexOf(favorite.Operation);
            if(index<0) { AnnounceToScreenReader("La operación del favorito ya no está disponible."); return; }
            calcTemplate.SelectedIndex=index; calculadoraInput.Text=favorite.Input;
            calculatorTabs2824.SelectedTab=calculatorBasicPage2824; calculadoraInput.Focus(); calculadoraInput.SelectAll();
        } else {
            int category=advancedCategory2824.Items.IndexOf(favorite.Category);
            if(category<0) { AnnounceToScreenReader("La categoría del favorito ya no está disponible."); return; }
            advancedCategory2824.SelectedIndex=category;
            int operation=advancedOperation2824.Items.IndexOf(favorite.Operation);
            if(operation<0) { AnnounceToScreenReader("La operación del favorito ya no está disponible."); return; }
            advancedOperation2824.SelectedIndex=operation; advancedInput2824.Text=favorite.Input;
            calculatorTabs2824.SelectedTab=calculatorAdvancedPage2824; advancedInput2824.Focus(); advancedInput2824.SelectAll();
        }
        AnnounceToScreenReader("Favorito cargado: "+favorite.Name+". Pulsa Calcular para realizarlo.");
    }
    private void ShowFavorites() {
        using(var dialog=new EscapeForm { Text="Gestionar favoritos",Width=540,Height=400,StartPosition=FormStartPosition.CenterParent }) {
            var list=new ListBox { Dock=DockStyle.Fill,AccessibleName="Favoritos guardados" };
            Action refresh=delegate { list.Items.Clear(); list.Items.AddRange(favorites.ToArray()); if(list.Items.Count>0)list.SelectedIndex=0; };
            var row=new FlowLayoutPanel { Dock=DockStyle.Bottom,AutoSize=true };
            row.Controls.Add(NewButton("Cargar",delegate { var selected=list.SelectedItem as Favorite; if(selected!=null) { dialog.Close(); OpenFavorite(selected); } }));
            row.Controls.Add(NewButton("Renombrar",delegate {
                var selected=list.SelectedItem as Favorite; if(selected==null)return;
                string name=AskProductivityName("Renombrar favorito",selected.Name); if(name==null)return;
                try { ChangeProductivity(delegate { selected.Name=name; }); refresh(); } catch(Exception ex) { MessageBox.Show(dialog,ex.Message); refresh(); }
            }));
            row.Controls.Add(NewButton("Eliminar",delegate { var selected=list.SelectedItem as Favorite; if(selected==null)return;
                if(MessageBox.Show(dialog,"¿Eliminar el favorito seleccionado?","Favoritos",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button2)!=DialogResult.Yes)return;
                try { ChangeProductivity(delegate { favorites.Remove(selected); }); refresh(); } catch(Exception ex) { MessageBox.Show(dialog,ex.Message); }
            }));
            row.Controls.Add(NewButton("Cerrar",delegate { dialog.Close(); }));
            dialog.Controls.Add(list); dialog.Controls.Add(row); refresh(); dialog.ShowDialog(this);
        }
    }
    private void ShowNamedMemories() {
        using(var dialog=new EscapeForm { Text="Memorias con nombre",Width=580,Height=440,StartPosition=FormStartPosition.CenterParent }) {
            var layout=new TableLayoutPanel { Dock=DockStyle.Fill,ColumnCount=1,RowCount=6,Padding=new Padding(12) };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            var list=new ListBox { Dock=DockStyle.Fill,AccessibleName="Memorias guardadas" }; layout.Controls.Add(list);
            var name=new TextBox { Dock=DockStyle.Top,MaxLength=80,AccessibleName="Nombre de memoria" };
            var value=new TextBox { Dock=DockStyle.Top,AccessibleName="Valor de memoria" };
            layout.Controls.Add(new Label { Text="Nombre:",AutoSize=true }); layout.Controls.Add(name);
            layout.Controls.Add(new Label { Text="Valor (coma o punto decimal):",AutoSize=true }); layout.Controls.Add(value);
            double current;
            if(double.TryParse(calculadoraResultado.Text.Replace(',','.'),NumberStyles.Float,CultureInfo.InvariantCulture,out current) && !double.IsNaN(current) && !double.IsInfinity(current)) value.Text=current.ToString("R",SellerCulture2825);
            Action refresh=delegate { list.Items.Clear(); foreach(var pair in namedMemories) list.Items.Add(pair.Key); };
            list.SelectedIndexChanged+=delegate { if(list.SelectedItem==null)return; name.Text=list.SelectedItem.ToString(); value.Text=namedMemories[name.Text].ToString("R",SellerCulture2825); };
            var row=new FlowLayoutPanel { AutoSize=true,Dock=DockStyle.Bottom };
            row.Controls.Add(NewButton("Guardar",delegate {
                double number; string key=name.Text.Trim();
                if(!double.TryParse(value.Text.Trim().Replace(',','.'),NumberStyles.Float,CultureInfo.InvariantCulture,out number) || double.IsNaN(number) || double.IsInfinity(number)) { MessageBox.Show(dialog,"Escribe un número válido."); return; }
                if(namedMemories.ContainsKey(key) && MessageBox.Show(dialog,"¿Sustituir el valor de "+key+"?","Memoria",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button2)!=DialogResult.Yes)return;
                try { ChangeProductivity(delegate { namedMemories[key]=number; }); refresh(); AnnounceToScreenReader("Memoria guardada: "+key+"."); } catch(Exception ex) { MessageBox.Show(dialog,ex.Message); }
            }));
            row.Controls.Add(NewButton("Usar en calculadora",delegate { if(list.SelectedItem==null)return; double number=namedMemories[list.SelectedItem.ToString()];
                dialog.Close(); calcTemplate.SelectedIndex=0; calculadoraInput.Text=number.ToString("R",SellerCulture2825);
                calculatorTabs2824.SelectedTab=calculatorBasicPage2824; calculadoraInput.Focus(); calculadoraInput.SelectionStart=calculadoraInput.TextLength;
                AnnounceToScreenReader("Memoria recuperada como entrada.");
            }));
            row.Controls.Add(NewButton("Eliminar",delegate { if(list.SelectedItem==null)return; string key=list.SelectedItem.ToString();
                if(MessageBox.Show(dialog,"¿Eliminar la memoria "+key+"?","Memorias",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button2)!=DialogResult.Yes)return;
                try { ChangeProductivity(delegate { namedMemories.Remove(key); }); refresh(); } catch(Exception ex) { MessageBox.Show(dialog,ex.Message); }
            }));
            row.Controls.Add(NewButton("Cerrar",delegate { dialog.Close(); }));
            dialog.Controls.Add(layout); dialog.Controls.Add(row); refresh(); dialog.ShowDialog(this);
        }
    }
} }
