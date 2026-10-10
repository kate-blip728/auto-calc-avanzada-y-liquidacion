using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Diagnostics;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace TodoAMano { public partial class MainForm {
    private const string AutoCalcVersion = "0.1.6";
    private const string UpdateRoot = "https://raw.githubusercontent.com/kate-blip728/auto-calc-avanzada-y-liquidacion/main/";
    private bool updateBusy;
    private void CheckGitHubUpdate() {
        if(updateBusy){AnnounceToScreenReader("Ya se están buscando actualizaciones.");return;}
        updateBusy=true;AnnounceToScreenReader("Buscando actualizaciones en GitHub.");
        BackgroundWorker worker=new BackgroundWorker();
        worker.DoWork+=delegate(object sender,DoWorkEventArgs e){
            byte[] bytes=ReadUpdateUrl(UpdateRoot+"update.json",65536);
            var info=new JavaScriptSerializer().Deserialize<Dictionary<string,string>>(Encoding.UTF8.GetString(bytes));
            Version latest; if(!Version.TryParse(info["version"],out latest))throw new Exception("Versión publicada no válida.");
            if(latest<=new Version(AutoCalcVersion)){e.Result=null;return;}
            string file="AutoCalc-"+latest.ToString()+".nvda-addon";
            if(info["file"]!=file || info["sha256"].Length!=64)throw new Exception("Información de descarga no válida.");
            e.Result=info;
        };
        worker.RunWorkerCompleted+=delegate(object sender,RunWorkerCompletedEventArgs e){
            updateBusy=false;worker.Dispose();
            if(IsDisposed)return;
            if(e.Error!=null){AnnounceToScreenReader("No se pudo buscar la actualización. "+e.Error.Message);return;}
            var info=e.Result as Dictionary<string,string>;
            if(info==null){AnnounceToScreenReader("Auto Calc "+AutoCalcVersion+" está actualizado.");return;}
            if(MessageBox.Show(this,"Hay una nueva versión: "+info["version"]+".\r\n\r\n"+info["notes"]+"\r\n\r\n¿Descargarla y abrir el instalador de NVDA?","Actualizar Auto Calc",MessageBoxButtons.YesNoCancel,MessageBoxIcon.Information)!=DialogResult.Yes)return;
            DownloadGitHubUpdate(info);
        };
        worker.RunWorkerAsync();
    }
    private void DownloadGitHubUpdate(Dictionary<string,string> info){
        updateBusy=true;AnnounceToScreenReader("Descargando actualización.");
        BackgroundWorker worker=new BackgroundWorker();
        worker.DoWork+=delegate(object sender,DoWorkEventArgs e){
            byte[] package=ReadUpdateUrl(UpdateRoot+info["file"],20*1024*1024);
            string hash;using(SHA256 sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(package)).Replace("-","").ToLowerInvariant();
            if(!string.Equals(hash,info["sha256"],StringComparison.OrdinalIgnoreCase))throw new Exception("La descarga no coincide con la versión publicada. Vuelve a buscar actualizaciones.");
            using(var stream=new MemoryStream(package))using(var zip=new ZipArchive(stream,ZipArchiveMode.Read)){
                var manifest=zip.GetEntry("manifest.ini");if(manifest==null)throw new Exception("El paquete no contiene un complemento de NVDA.");
                using(var reader=new StreamReader(manifest.Open())){string text=reader.ReadToEnd();if(text.IndexOf("version = "+info["version"],StringComparison.Ordinal)<0)throw new Exception("La versión del paquete no coincide.");}
            }
            string folder=Path.Combine(dataDir,"actualizaciones");Directory.CreateDirectory(folder);
            string path=Path.Combine(folder,info["file"]);File.WriteAllBytes(path+".tmp",package);
            if(File.Exists(path))File.Replace(path+".tmp",path,null);else File.Move(path+".tmp",path);e.Result=path;
        };
        worker.RunWorkerCompleted+=delegate(object sender,RunWorkerCompletedEventArgs e){
            updateBusy=false;worker.Dispose();if(IsDisposed)return;
            if(e.Error!=null){AnnounceToScreenReader("No se pudo descargar la actualización. "+e.Error.Message);return;}
            string path=(string)e.Result;
            AnnounceToScreenReader("Actualización descargada y verificada. Cierra la calculadora y acepta la instalación en NVDA. Después reinicia NVDA.");
            try{Process.Start(new ProcessStartInfo(path){UseShellExecute=true});}catch(Exception ex){MessageBox.Show(this,"Abre este archivo con NVDA: "+path+"\r\n"+ex.Message,"Actualización descargada");}
        };worker.RunWorkerAsync();
    }
    private static byte[] ReadUpdateUrl(string url,int maximum){
        HttpWebRequest request=(HttpWebRequest)WebRequest.Create(url);request.Timeout=20000;request.ReadWriteTimeout=20000;request.UserAgent="AutoCalc/"+AutoCalcVersion;request.AllowAutoRedirect=false;
        using(var response=(HttpWebResponse)request.GetResponse())using(var stream=response.GetResponseStream())using(var memory=new MemoryStream()){
            byte[] buffer=new byte[8192];int count;while((count=stream.Read(buffer,0,buffer.Length))>0){if(memory.Length+count>maximum)throw new Exception("La descarga supera el tamaño permitido.");memory.Write(buffer,0,count);}return memory.ToArray();
        }
    }
} }
