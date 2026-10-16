using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace TodoAMano { public partial class MainForm {
    public void RunSafetyFeatureTests() {
        soundsEnabled=false; mobileAutoSync2940=false;
        sellerQuantity2825.Text="2"; AddSellerLine2825();
        Check(sellerLines2825.Count==1 && draftUndoButton.Enabled,"añadir fila permite deshacer");
        UndoDraftChange(false); Check(sellerLines2825.Count==0 && draftRedoButton.Enabled,"deshacer restaura borrador vacío");
        UndoDraftChange(true); Check(sellerLines2825[0].Quantity==2,"rehacer recupera fila y cantidad");
        ChangeSelectedSellerQuantity2825(1); UndoDraftChange(false);
        Check(sellerLines2825[0].Quantity==2,"deshacer cantidad conserva una copia independiente");
        sellerQuantity2825.Text="7"; UpdateSelectedSellerLine2825();
        Check(!draftRedoButton.Enabled && sellerLines2825[0].Quantity==7,"editar tras deshacer descarta rehacer");
        UndoDraftChange(false); Check(sellerLines2825[0].Quantity==2,"deshacer edición restaura cantidad");
        DeleteSelectedSellerLine2825(); UndoDraftChange(false);
        Check(sellerLines2825.Count==1 && sellerLines2825[0].Quantity==2,"deshacer recupera fila eliminada");
        sellerRecordNote2826.Text="Turno mañana";
        sellerRecordDate2826.Value=new DateTime(2026,10,16,10,30,0);
        var saved=new SellerLiquidation2826 { Id="backup-test", When=DateTime.Now.ToString("o"), UpdatedAt=DateTime.UtcNow.ToString("o"), DeviceId="test", Note="Guardada" };
        saved.Lines.Add(CloneSellerLine2826(sellerLines2825[0])); sellerSaved2826.Add(saved);
        var revision=new SellerRevision2827 { CorrectedAt=DateTime.Now.ToString("o"), PreviousWhen=saved.When, PreviousNote="Anterior", ChangeSummary="Corrección" };
        revision.PreviousLines.Add(CloneSellerLine2826(sellerLines2825[0])); saved.Revisions.Add(revision);
        calculatorHistory2824.Add(new CalculatorHistoryItem2824 { When="16/10/2026 10:30:00", Kind="Básica", Input="2+3", Result="5", ReuseTarget="basica" });
        string content=BackupSerializer().Serialize(CaptureBackupData());
        string backup=WriteBackup(content);
        var data=ReadBackup(backup);
        using(var zip=ZipFile.OpenRead(backup)) Check(zip.Entries.Count==1 && zip.Entries[0].FullName=="autocalc-backup.json","copia contiene solo datos autorizados, sin credenciales");
        sellerLines2825.Clear(); sellerSaved2826.Clear(); calculatorHistory2824.Clear();
        sellerRecordNote2826.Text="Cambio posterior"; mobileAutoSync2940=true;
        RestoreBackupData(data);
        Check(sellerLines2825.Count==1 && sellerLines2825[0].Quantity==2,"restaurar recupera borrador");
        Check(sellerSaved2826.Count==1 && sellerSaved2826[0].Revisions.Count==1,"restaurar conserva liquidaciones y sus revisiones");
        Check(calculatorHistory2824.Count==1 && calculatorHistory2824[0].Result=="5","restaurar recupera historial");
        Check(sellerRecordNote2826.Text=="Turno mañana" && sellerRecordDate2826.Value.Hour==10,"restaurar recupera fecha y nota");
        Check(!mobileAutoSync2940 && draftUndo.Count==0 && draftRedo.Count==0,"restauración desactiva nube automática y vacía deshacer");
        Check(Directory.GetFiles(BackupFolder,"AutoCalc-copia-*.zip").Length==2,"restauración crea copia del estado anterior");
        string invalid=Path.Combine(dataDir,"invalid.zip");
        WriteBackupZip(invalid,"{\"format\":\"otro\"}");
        bool rejected=false; try { ReadBackup(invalid); } catch { rejected=true; }
        Check(rejected && sellerLines2825.Count==1,"rechaza copia inválida sin modificar datos");
        string traversal=Path.Combine(dataDir,"traversal.zip");
        using(var zip=ZipFile.Open(traversal,ZipArchiveMode.Create)) zip.CreateEntry("../otro.json");
        rejected=false; try { ReadBackup(traversal); } catch { rejected=true; }
        Check(rejected,"rechaza entradas inesperadas y rutas externas");
        data["draft"]=new object[]{new Dictionary<string,object>{{"tipo","Moneda"},{"denominacion","1"},{"cantidad",-5},{"unidades_envase",1},{"valor_unitario",1}}};
        rejected=false; try { RestoreBackupData(data); } catch { rejected=true; }
        Check(rejected && sellerLines2825[0].Quantity==2,"valida contenido antes de restaurar");
        for(int i=0;i<22;i++) WriteBackup(content);
        Check(Directory.GetFiles(BackupFolder,"AutoCalc-copia-*.zip").Length==20,"rotación conserva las 20 últimas copias");
        SaveDraftContext();
        using(var reopened=new MainForm(dataDir)) {
            Check(reopened.sellerRecordNote2826.Text=="Turno mañana" && reopened.sellerRecordDate2826.Value.Hour==10,"fecha y nota conservadas al reabrir"); }
        for(int i=0;i<55;i++) ChangeSelectedSellerQuantity2825(1);
        Check(draftUndo.Count==50,"deshacer limita memoria a 50 estados");
        calculatorTabs2824.SelectedTab=calculatorSellerPage2825;
        int before=sellerLines2825[0].Quantity; var message=new Message();
        ProcessCmdKey(ref message,Keys.Control|Keys.Alt|Keys.Z);
        Check(sellerLines2825[0].Quantity==before-1,"atajo de deshacer funciona en liquidaciones");
        ProcessCmdKey(ref message,Keys.Control|Keys.Alt|Keys.Y);
        Check(sellerLines2825[0].Quantity==before,"atajo de rehacer funciona en liquidaciones");
        var handle=Handle;
        QueueBackup(false);
        var deadline=DateTime.UtcNow.AddSeconds(10);
        while(backupBusy && DateTime.UtcNow<deadline) { Application.DoEvents(); System.Threading.Thread.Sleep(20); }
        Check(!backupBusy && lastBackupContent!=null,"copia automática se completa en segundo plano");
        var names=Directory.GetFiles(BackupFolder,"AutoCalc-copia-*.zip").OrderBy(n=>n).ToArray();
        QueueBackup(false);
        Check(!backupBusy && names.SequenceEqual(Directory.GetFiles(BackupFolder,"AutoCalc-copia-*.zip").OrderBy(n=>n)),"sin cambios no duplica copias automáticas");
        PrepareNewSellerLiquidationAfterSave2826();
        Check(draftUndo.Count==0 && draftRedo.Count==0,"guardar cierra el historial de deshacer del borrador");
    }
    public void RunProductivityTests() {
        soundsEnabled=false; mobileAutoSync2940=false; calcTemplate.SelectedIndex=0;
        lastAnswer=null; calculadoraInput.Text="Ans+1"; Calculate();
        Check(calculadoraResultado.Text.StartsWith("No se pudo") && !lastAnswer.HasValue,"Ans avisa si todavía no hay resultado");
        calculadoraInput.Text="-2"; Calculate(); calculadoraInput.Text="Ans*3"; Calculate();
        Check(calculadoraResultado.Text=="-6","Ans encadena correctamente resultados negativos");
        calculadoraInput.Text="Ans+10"; Calculate();
        Check(calculadoraResultado.Text=="4","Ans usa el resultado más reciente");
        calculadoraInput.Text="1/0"; Calculate(); Check(lastAnswer==4,"un cálculo inválido conserva el último resultado válido");
        calculadoraInput.Text="1e20"; Calculate(); calculadoraInput.Text="Ans/1e20"; Calculate();
        Check(calculadoraResultado.Text=="1","encadenado admite notación científica");
        calculadoraInput.Text="Ans*2"; Check(!TryAnnounceRepeatedCalculation(),"Ans no reutiliza avisos de un contexto antiguo");
        ContinueCalculation(); Check(calculadoraInput.Text=="Ans " && calcTemplate.SelectedIndex==0,"botón de continuar prepara una expresión libre");
        var basic=new Favorite { Name="Porcentaje favorito",Kind="basic",Category="",Operation=calcTemplate.Items[1].ToString(),Input="25;200" };
        var advanced=new Favorite { Name="Precio favorito",Kind="advanced",Category="Porcentajes y compras",Operation="Precio por unidad",Input="12;8" };
        ChangeProductivity(delegate { favorites.Add(basic); favorites.Add(advanced); namedMemories["Caja"]=125.5; });
        OpenFavorite(basic);
        Check(calcTemplate.SelectedIndex==1 && calculadoraInput.Text=="25;200","favorito recupera modo y datos básicos");
        Calculate(); Check(calculadoraResultado.Text=="50","cálculo cargado de favorito conserva resultado correcto");
        OpenFavorite(advanced); CalculateAdvanced2824();
        Check(advancedResult2824.Text=="Precio por unidad: 1,5.","favorito avanzado recupera categoría, operación y datos");
        using(var reopened=new MainForm(dataDir)) {
            Check(reopened.favorites.Count==2 && reopened.namedMemories["caja"]==125.5,"favoritos y memorias persisten y no distinguen mayúsculas en el nombre");
            Check(reopened.favoriteShortcuts.DropDownItems.Count==2 && ((ToolStripMenuItem)reopened.favoriteShortcuts.DropDownItems[0]).ShortcutKeys==(Keys.Control|Keys.Shift|Keys.F1),"favorito tiene atajo de teclado");
        }
        var data=ReadBackup(WriteBackup(BackupSerializer().Serialize(CaptureBackupData())));
        ChangeProductivity(delegate { favorites.Clear(); namedMemories.Clear(); });
        RestoreBackupData(data);
        Check(favorites.Count==2 && namedMemories["Caja"]==125.5,"copia restaura favoritos y memorias");
        data.Remove("productivity"); RestoreBackupData(data);
        Check(favorites.Count==2 && namedMemories.Count==1,"restaurar copia antigua conserva favoritos y memorias actuales");
        string originalDir=dataDir;
        string blocked=Path.Combine(dataDir,"blocked-data"); File.WriteAllText(blocked,"test"); dataDir=blocked;
        bool rejected=false;
        try { ChangeProductivity(delegate { namedMemories["Caja"]=999; }); } catch { rejected=true; } finally { dataDir=originalDir; }
        Check(rejected && namedMemories["Caja"]==125.5,"fallo al guardar revierte cambios de memoria");
        Check(CalculateAdvancedText2824("Quitar IVA del total","121;21")=="Base sin IVA: 100. IVA incluido: 21.","desglosa base e IVA desde el total");
        Check(CalculateAdvancedText2824("Qué porcentaje es una parte del total","30;200")=="Porcentaje: 15 %.","calcula porcentaje de una parte");
        Check(CalculateAdvancedText2824("Precio por unidad","12;8")=="Precio por unidad: 1,5.","calcula precio unitario");
        Check(CalculateAdvancedText2824("Media ponderada","10;2;20;3")=="Media ponderada: 16.","media ponderada correcta");
        Check(CalculateAdvancedText2824("Media ponderada","10;1e308;20;1e308")=="Media ponderada: 15.","media ponderada evita desbordar pesos grandes");
        foreach(var pair in new[]{new[]{"Precio por unidad","10;0"},new[]{"Quitar IVA del total","121;-100"},new[]{"Qué porcentaje es una parte del total","3;0"},new[]{"Media ponderada","1;0;2;0"},new[]{"Media ponderada","1;-2"},new[]{"Media ponderada","1;2;3"},new[]{"Precio por unidad","1e309;2"}}) {
            rejected=false; try { CalculateAdvancedText2824(pair[0],pair[1]); } catch { rejected=true; }
            Check(rejected,"rechaza datos inválidos en "+pair[0]);
        }
        advancedCategory2824.SelectedIndex=0; advancedOperation2824.SelectedItem="Quitar IVA del total";
        Check(advancedInput2824.Text=="121;21" && advancedOperationHelp.Text.Contains("total con IVA;porcentaje"),"operación nueva ofrece ejemplo y miniayuda con orden de datos");
    }
} }
