using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace TodoAMano { public partial class MainForm {
    private readonly object backupStorageLock = new object();
    private System.Windows.Forms.Timer automaticBackupTimer;
    private bool backupBusy;
    private string lastBackupContent;
    private string BackupFolder { get { return Path.Combine(dataDir,"copias-seguridad"); } }
    private JavaScriptSerializer BackupSerializer() { return new JavaScriptSerializer { MaxJsonLength = 10*1024*1024 }; }
    private Dictionary<string,object> CaptureBackupData() {
        return new Dictionary<string,object> {
            {"format","AutoCalcBackup-1"}, {"productivity",CaptureProductivityData()}, {"draft",BuildSellerDraftSnapshot285()},
            {"saved",BuildSellerLiquidationsSnapshot285()}, {"history",BuildCalculatorHistorySnapshot285()},
            {"note",sellerRecordNote2826.Text}, {"when",sellerRecordDate2826.Value.ToString("o",CultureInfo.InvariantCulture)}
        };
    }
    private void InstallBackupMenu(MenuStrip menu) {
        var item = new ToolStripMenuItem("Copias de seguridad");
        item.DropDownItems.Add("Crear copia ahora",null,delegate { QueueBackup(true); });
        item.DropDownItems.Add("Exportar copia a un archivo…",null,delegate { ExportBackup(); });
        item.DropDownItems.Add("Restaurar copia…",null,delegate { ChooseBackupToRestore(); });
        menu.Items.Add(item);
        Shown += delegate {
            BeginInvoke((MethodInvoker)delegate { QueueBackup(false); });
            automaticBackupTimer = new System.Windows.Forms.Timer { Interval=15*60*1000 };
            automaticBackupTimer.Tick += delegate { QueueBackup(false); };
            automaticBackupTimer.Start();
        };
        FormClosed += delegate { if(automaticBackupTimer!=null) automaticBackupTimer.Dispose(); };
    }
    private string WriteBackup(string content) {
        if(Encoding.UTF8.GetByteCount(content)>10*1024*1024) throw new IOException("Los datos superan el límite de 10 MB de esta versión.");
        lock(backupStorageLock) {
            Directory.CreateDirectory(BackupFolder);
            string path = Path.Combine(BackupFolder,"AutoCalc-copia-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+"-"+Guid.NewGuid().ToString("N")+".zip");
            WriteBackupZip(path,content);
            // Only rotate files produced here; exported copies are never removed.
            var files = new DirectoryInfo(BackupFolder).GetFiles("AutoCalc-copia-*.zip").OrderByDescending(f=>f.Name).ToArray();
            foreach(var file in files.Skip(20)) { try { file.Delete(); } catch { } }
            return path;
        }
    }
    private static void WriteBackupZip(string path,string content) {
        if(Encoding.UTF8.GetByteCount(content)>10*1024*1024) throw new IOException("Los datos superan el límite de 10 MB de esta versión.");
        string temp=path+".tmp-"+Guid.NewGuid().ToString("N");
        try {
            using(var zip=ZipFile.Open(temp,ZipArchiveMode.Create))
            using(var writer=new StreamWriter(zip.CreateEntry("autocalc-backup.json").Open(),new UTF8Encoding(false))) writer.Write(content);
            if(File.Exists(path)) File.Replace(temp,path,null); else File.Move(temp,path);
        } finally { if(File.Exists(temp)) File.Delete(temp); }
    }
    private void QueueBackup(bool announce) {
        if(backupBusy) { if(announce) AnnounceToScreenReader("Se está creando una copia. Espera un instante."); return; }
        string content;
        try { content=BackupSerializer().Serialize(CaptureBackupData()); }
        catch(Exception ex) { AnnounceToScreenReader("No se pudo preparar la copia. "+ex.Message); return; }
        if(!announce && content==lastBackupContent) return;
        backupBusy=true;
        ThreadPool.QueueUserWorkItem(delegate {
            string error=null;
            try { WriteBackup(content); } catch(Exception ex) { error=ex.Message; }
            try { BeginInvoke((MethodInvoker)delegate {
                backupBusy=false;
                if(error==null) { lastBackupContent=content; if(announce) AnnounceToScreenReader("Copia creada. Se conservan las últimas 20 en la carpeta copias-seguridad de Auto Calc."); }
                else AnnounceToScreenReader("No se pudo crear la copia de seguridad. "+error);
            }); } catch { }
        });
    }
    private void ExportBackup() {
        using(var dialog=new SaveFileDialog { Filter="Copia de Auto Calc (*.zip)|*.zip", FileName="AutoCalc-copia-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".zip" }) {
            if(dialog.ShowDialog(this)!=DialogResult.OK) return;
            try { WriteBackupZip(dialog.FileName,BackupSerializer().Serialize(CaptureBackupData())); AnnounceToScreenReader("Copia exportada."); }
            catch(Exception ex) { MessageBox.Show(this,ex.Message,"No se pudo exportar la copia"); }
        }
    }
    private Dictionary<string,object> ReadBackup(string path) {
        using(var zip=ZipFile.OpenRead(path)) {
            if(zip.Entries.Count!=1 || zip.Entries[0].FullName!="autocalc-backup.json" || zip.Entries[0].Length>10*1024*1024)
                throw new InvalidDataException("El archivo no es una copia compatible de Auto Calc.");
            string content;
            using(var reader=new StreamReader(zip.Entries[0].Open(),Encoding.UTF8)) {
                char[] buffer=new char[8192]; int count; var text=new StringBuilder();
                while((count=reader.Read(buffer,0,buffer.Length))>0) {
                    if(text.Length+count>10*1024*1024) throw new InvalidDataException("Copia demasiado grande.");
                    text.Append(buffer,0,count);
                }
                content=text.ToString();
            }
            var data=BackupSerializer().DeserializeObject(content) as Dictionary<string,object>;
            ValidateBackup(data); return data;
        }
    }
    private static string BackupText(Dictionary<string,object> row,string key) {
        object value;
        if(!row.TryGetValue(key,out value) || !(value is string)) throw new InvalidDataException("Campo inválido en la copia: "+key);
        return (string)value;
    }
    private static IList BackupList(Dictionary<string,object> row,string key) {
        object value;
        if(!row.TryGetValue(key,out value) || !(value is IList)) throw new InvalidDataException("Lista inválida en la copia: "+key);
        return (IList)value;
    }
    private static Dictionary<string,object> BackupRow(object value) {
        var row=value as Dictionary<string,object>;
        if(row==null) throw new InvalidDataException("Registro inválido en la copia."); return row;
    }
    private static void ValidateBackupLines(IList lines) {
        foreach(object value in lines) {
            var row=BackupRow(value); BackupText(row,"tipo"); BackupText(row,"denominacion");
            int quantity=Convert.ToInt32(row["cantidad"],CultureInfo.InvariantCulture);
            int units=Convert.ToInt32(row["unidades_envase"],CultureInfo.InvariantCulture);
            double amount=Convert.ToDouble(row["valor_unitario"],CultureInfo.InvariantCulture);
            if(quantity<1 || units<1 || amount<0 || double.IsNaN(amount) || double.IsInfinity(amount)) throw new InvalidDataException("Fila de efectivo inválida.");
        }
    }
    private static void ValidateBackup(Dictionary<string,object> data) {
        if(data==null || BackupText(data,"format")!="AutoCalcBackup-1") throw new InvalidDataException("Formato de copia incompatible.");
        if(data.ContainsKey("productivity")) ValidateProductivityData(data["productivity"] as Dictionary<string,object>);
        BackupText(data,"note");
        DateTime when=DateTime.Parse(BackupText(data,"when"),CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind);
        if(when.Year<1753 || when.Year>9998) throw new InvalidDataException("Fecha de borrador inválida.");
        ValidateBackupLines(BackupList(data,"draft"));
        foreach(object value in BackupList(data,"history")) {
            var row=BackupRow(value);
            foreach(string key in new[]{"fecha","tipo","entrada","resultado","destino"}) BackupText(row,key);
        }
        var ids=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach(object value in BackupList(data,"saved")) {
            var row=BackupRow(value); string id=BackupText(row,"id");
            if(string.IsNullOrWhiteSpace(id) || !ids.Add(id)) throw new InvalidDataException("Identificador de liquidación inválido o repetido.");
            DateTime.Parse(BackupText(row,"fecha"),CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind);
            BackupText(row,"nota"); BackupText(row,"updated_at"); BackupText(row,"device_id");
            ValidateBackupLines(BackupList(row,"filas"));
            if(row.ContainsKey("correcciones")) foreach(object revision in BackupList(row,"correcciones")) {
                var r=BackupRow(revision);
                foreach(string key in new[]{"fecha_correccion","fecha_anterior","nota_anterior","resumen"}) BackupText(r,key);
                ValidateBackupLines(BackupList(r,"filas_anteriores"));
            }
        }
    }
    private void ChooseBackupToRestore() {
        if(mobileSyncBusy2940) { AnnounceToScreenReader("Espera a que termine la sincronización antes de restaurar."); return; }
        using(var dialog=new OpenFileDialog { Filter="Copia de Auto Calc (*.zip)|*.zip", InitialDirectory=BackupFolder }) {
            if(dialog.ShowDialog(this)!=DialogResult.OK) return;
            try {
                var data=ReadBackup(dialog.FileName);
                if(MessageBox.Show(this,"Se sustituirán el borrador, las liquidaciones guardadas y el historial de cálculos por esta copia. Antes se conservará una copia del estado actual. La sincronización automática quedará desactivada; podrás revisarla en Inventario y nube. No se restauran conexiones ni inventario. Las copias nuevas incluyen favoritos y memorias. ¿Restaurar?","Restaurar copia",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button2)!=DialogResult.Yes) return;
                RestoreBackupData(data);
                AnnounceToScreenReader("Copia restaurada. La sincronización automática está desactivada. Revisa los datos antes de sincronizar.");
            } catch(Exception ex) { MessageBox.Show(this,ex.Message,"No se pudo restaurar la copia"); }
        }
    }
    private void RestoreBackupData(Dictionary<string,object> data) {
        ValidateBackup(data);
        if(mobileSyncBusy2940) throw new InvalidOperationException("Hay una sincronización en curso.");
        var serializer=BackupSerializer();
        foreach(string key in new[]{"draft","saved","history"}) {
            if(serializer.Serialize(data[key]).Length>2097152) throw new InvalidDataException("Una lista supera el tamaño compatible con el almacenamiento de esta versión.");
        }
        var old=CaptureBackupData();
        WriteBackup(serializer.Serialize(old)); // Must succeed before changing any data.
        EnsureLiquidacionMovilData2940();
        mobileAutoSync2940=false;
        if(mobileSyncDebounceTimer2940!=null) mobileSyncDebounceTimer2940.Change(Timeout.Infinite,Timeout.Infinite);
        SaveLiquidacionMovilSettings2940();
        FlushSellerDraftSave2930(); FlushCalculatorPersistence285();
        lock(calculatorPersistenceLock285) {
            try { ApplyBackupFiles(data,serializer); }
            catch { ApplyBackupFiles(old,serializer); throw; }
        }
        ClearDraftUndo(); sellerCorrectionId2827=""; lastAnswer=null;
        RefreshSellerRows2825(-1); RefreshSellerSavedList2826(); RefreshHistoryList2824(-1);
        UpdateSellerCorrectionStatus2827(); RefreshLiquidacionMovilUi2940();
        SaveDraftContext();
        lastBackupContent=null;
    }
    private void ApplyBackupFiles(Dictionary<string,object> data,JavaScriptSerializer serializer) {
        AtomicWriteCalculatorJson285(sellerDraftPath2825,serializer.Serialize(data["draft"]));
        AtomicWriteCalculatorJson285(sellerSavedPath2826,serializer.Serialize(data["saved"]));
        AtomicWriteCalculatorJson285(calculatorHistoryPath2824,serializer.Serialize(data["history"]));
        if(data.ContainsKey("productivity")) {
            AtomicWriteCalculatorJson285(Path.Combine(dataDir,"favoritos-memorias.json"),serializer.Serialize(data["productivity"]));
            ApplyProductivityData((Dictionary<string,object>)data["productivity"]);
        }
        LoadSellerDraft2825(); LoadSellerLiquidations2826(); LoadCalculatorHistory2824();
        sellerRecordNote2826.Text=(string)data["note"];
        sellerRecordDate2826.Value=DateTime.Parse((string)data["when"],CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind);
    }
} }
