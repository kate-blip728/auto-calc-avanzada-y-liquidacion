using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace TodoAMano { public partial class MainForm {
    private readonly object cloudTokenLock = new object();
    private string CloudConfigPath(string provider){return Path.Combine(dataDir,provider=="Dropbox"?"dropbox_oauth.json":"drive_oauth.json");}
    private Dictionary<string,string> ReadCloudConfig(string provider){
        string path=CloudConfigPath(provider);if(!File.Exists(path))return new Dictionary<string,string>();
        return new JavaScriptSerializer().Deserialize<Dictionary<string,string>>(File.ReadAllText(path,Encoding.UTF8));
    }
    private string ConfigValue(Dictionary<string,string> config,string key){string value;return config.TryGetValue(key,out value)?value:"";}
    private void WriteCloudConfig(string provider,Dictionary<string,string> config){
        string path=CloudConfigPath(provider);File.WriteAllText(path+".tmp",new JavaScriptSerializer().Serialize(config),new UTF8Encoding(false));
        if(File.Exists(path))File.Replace(path+".tmp",path,path+".bak");else File.Move(path+".tmp",path);
    }
    private string ProtectCloud(string value){return Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(value),null,DataProtectionScope.CurrentUser));}
    private string UnprotectCloud(string value){if(string.IsNullOrWhiteSpace(value))return "";return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(value),null,DataProtectionScope.CurrentUser));}
    private bool CloudConnected2878(string provider){try{return UnprotectCloud(ConfigValue(ReadCloudConfig(provider),"refresh_token")).Length>0;}catch{return false;}}
    private bool DriveConnected2851(){return CloudConnected2878("Google Drive");}
    private string GetDropboxAccessToken2878(){return CloudAccessToken("Dropbox");}
    private string GetDriveAccessToken2851(){return CloudAccessToken("Google Drive");}
    private string CloudAccessToken(string provider){
        lock(cloudTokenLock){
            Dictionary<string,string> cfg=ReadCloudConfig(provider);
            Dictionary<string,string> args=new Dictionary<string,string>();
            args["grant_type"]="refresh_token";args["refresh_token"]=UnprotectCloud(ConfigValue(cfg,"refresh_token"));args["client_id"]=ConfigValue(cfg,"client_id");
            if(provider=="Google Drive")args["client_secret"]=UnprotectCloud(ConfigValue(cfg,"client_secret"));
            Dictionary<string,object> response=CloudTokenRequest(provider,args);
            string token=GetString(response,"access_token");if(token.Length==0)throw new Exception("La nube no devolvió un token. Vuelve a conectar la cuenta.");return token;
        }
    }
    private Dictionary<string,object> CloudTokenRequest(string provider,Dictionary<string,string> args){
        List<string> parts=new List<string>();foreach(var pair in args)parts.Add(Uri.EscapeDataString(pair.Key)+"="+Uri.EscapeDataString(pair.Value));
        int status;string result=CloudHttp2878("POST",provider=="Dropbox"?"https://api.dropboxapi.com/oauth2/token":"https://oauth2.googleapis.com/token",string.Join("&",parts.ToArray()),"application/x-www-form-urlencoded",null,null,null,out status);
        if(status<200||status>=300)throw new Exception("La nube rechazó la autorización (HTTP "+status+"). Comprueba la configuración y vuelve a conectar.");
        return new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(result);
    }
    private string CloudHttp2878(string method,string url,string body,string contentType,string token,Dictionary<string,string> headers,object unused,out int status){
        return Encoding.UTF8.GetString(CloudHttpBytes2878(method,url,body==null?null:Encoding.UTF8.GetBytes(body),contentType,token,headers,out status));
    }
    private byte[] CloudHttpBytes2878(string method,string url,byte[] body,string contentType,string token,Dictionary<string,string> headers,out int status){
        HttpWebRequest request=(HttpWebRequest)WebRequest.Create(url);request.Method=method;request.Timeout=30000;request.ReadWriteTimeout=30000;request.AllowAutoRedirect=false;
        if(!string.IsNullOrEmpty(token))request.Headers["Authorization"]="Bearer "+token;
        if(headers!=null)foreach(var header in headers)request.Headers[header.Key]=header.Value;
        if(body!=null){request.ContentType=contentType;request.ContentLength=body.Length;using(Stream stream=request.GetRequestStream())stream.Write(body,0,body.Length);}
        else if(method=="POST"||method=="PATCH")request.ContentLength=0;
        HttpWebResponse response;
        try{response=(HttpWebResponse)request.GetResponse();}catch(WebException ex){response=ex.Response as HttpWebResponse;if(response==null)throw;}
        using(response){status=(int)response.StatusCode;using(Stream stream=response.GetResponseStream())using(MemoryStream memory=new MemoryStream()){stream.CopyTo(memory);return memory.ToArray();}}
    }
    private void ShowCloudConfiguration(string provider){
        using(Form f=new EscapeForm()){f.Text="Conectar "+provider;f.Width=650;f.Height=420;f.StartPosition=FormStartPosition.CenterParent;
            TableLayoutPanel layout=BaseLayout();
            TextBox info=ResultBox();info.Text="Usa la misma aplicación OAuth que Navaja para compartir su archivo de nube. Google Drive guarda estos datos en una carpeta privada por aplicación. Configura un cliente de escritorio en Google o una aplicación con PKCE en Dropbox. Las credenciales se guardan protegidas en este usuario de Windows.";info.Height=110;AddResultRow(layout,"Conexión:",info);
            var cfg=ReadCloudConfig(provider);
            TextBox client=AddLabeledText(layout,"Identificador de cliente / app key:",false);client.Text=ConfigValue(cfg,"client_id");
            TextBox secret=null;if(provider=="Google Drive"){secret=AddLabeledText(layout,"Secreto del cliente de escritorio:",false);secret.UseSystemPasswordChar=true;secret.Text=UnprotectCloud(ConfigValue(cfg,"client_secret"));}
            FlowLayoutPanel buttons=ButtonRow();Button connect=NewButton("Autorizar en el navegador",delegate{
                if(string.IsNullOrWhiteSpace(client.Text)){MessageBox.Show(f,"Introduce el identificador de la aplicación.");return;}
                cfg["client_id"]=client.Text.Trim();if(secret!=null)cfg["client_secret"]=ProtectCloud(secret.Text.Trim());WriteCloudConfig(provider,cfg);
                AuthorizeCloud(provider);f.Close();
            });buttons.Controls.Add(connect);
            buttons.Controls.Add(NewButton("Importar conexión de Navaja",delegate { try { ImportNavajaConnection(provider); f.Close(); } catch(Exception ex) { MessageBox.Show(f,ex.Message,"Importar conexión"); } }));
            buttons.Controls.Add(NewButton("Desconectar",delegate{cfg.Remove("refresh_token");WriteCloudConfig(provider,cfg);RefreshSellerCloudStatus2962();f.Close();}));
            buttons.Controls.Add(NewButton("Cancelar",delegate{f.Close();}));AddFullRow(layout,buttons,false);f.Controls.Add(layout);f.ShowDialog(this);
        }
    }
    private void ImportNavajaConnection(string provider){
        using(FolderBrowserDialog picker=new FolderBrowserDialog()){
            picker.Description="Selecciona la carpeta de tu instalación de Navaja Suiza";
            if(picker.ShowDialog(this)!=DialogResult.OK)return;
            string root=picker.SelectedPath,source=Path.Combine(root,"datos");
            string tokenPath=Path.Combine(source,provider=="Dropbox"?"nube_dropbox_token.bin":"google_drive_token.bin");
            string entropy=provider=="Dropbox"?"NavajaSuiza-NubesAPI-2878-dropbox":"NavajaSuiza-GoogleDrive-2851";
            byte[] plain=ProtectedData.Unprotect(File.ReadAllBytes(tokenPath),Encoding.UTF8.GetBytes(entropy),DataProtectionScope.CurrentUser);
            var tokens=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(Encoding.UTF8.GetString(plain));
            string refresh=GetString(tokens,"refresh_token");if(refresh.Length==0)throw new Exception("La conexión de Navaja no contiene autorización permanente. Reconecta la cuenta en Navaja o en Auto Calc.");
            var config=new Dictionary<string,string>();
            if(provider=="Dropbox"){
                var settings=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(File.ReadAllText(Path.Combine(source,"nubes_api.json"),Encoding.UTF8));
                config["client_id"]=GetString(settings,"dropbox_app_key");
            }else{
                var settings=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(File.ReadAllText(Path.Combine(root,"NAVAJA_CORREO_OAUTH_PUBLICO.json"),Encoding.UTF8));
                config["client_id"]=GetString(settings,"googleClientId");config["client_secret"]=ProtectCloud(GetString(settings,"googleClientSecret"));
            }
            if(ConfigValue(config,"client_id").Length==0)throw new Exception("No se encontró el identificador OAuth de Navaja. Puedes introducirlo manualmente.");
            config["refresh_token"]=ProtectCloud(refresh);lock(cloudTokenLock){WriteCloudConfig(provider,config);}
            mobileSyncProvider2940=provider;SaveLiquidacionMovilSettings2940();RefreshLiquidacionMovilUi2940();
            SetLiquidacionMovilStatus2940("Conexión importada de Navaja para este usuario de Windows. Pulsa Sincronizar ahora para comprobarla.",true);
        }
    }
    private string RandomCloudString(){byte[] bytes=new byte[32];using(var rng=RandomNumberGenerator.Create())rng.GetBytes(bytes);return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+','-').Replace('/','_');}
    private void AuthorizeCloud(string provider){
        string verifier=RandomCloudString(),state=RandomCloudString(),challenge;
        using(SHA256 sha=SHA256.Create())challenge=Convert.ToBase64String(sha.ComputeHash(Encoding.ASCII.GetBytes(verifier))).TrimEnd('=').Replace('+','-').Replace('/','_');
        var cfg=ReadCloudConfig(provider);
        if(provider=="Dropbox"){
            string url="https://www.dropbox.com/oauth2/authorize?response_type=code&token_access_type=offline&client_id="+Uri.EscapeDataString(cfg["client_id"])+"&code_challenge_method=S256&code_challenge="+challenge;
            Process.Start(new ProcessStartInfo(url){UseShellExecute=true});
            using(Form f=new EscapeForm()){f.Text="Código de autorización de Dropbox";f.Width=550;f.Height=210;
                TableLayoutPanel layout=BaseLayout();TextBox code=AddLabeledText(layout,"Pega el código mostrado por Dropbox:",false);
                var buttons=ButtonRow();buttons.Controls.Add(NewButton("Conectar",delegate{string entered=code.Text.Trim();if(entered.Length==0)return;CompleteCloudAuthorization(provider,cfg,verifier,entered,null);f.Close();}));buttons.Controls.Add(NewButton("Cancelar",delegate{f.Close();}));AddFullRow(layout,buttons,false);f.Controls.Add(layout);f.ShowDialog(this);
            }return;
        }
        // Loopback callback for Google's desktop OAuth client, with state and PKCE.
        TcpListener listener=new TcpListener(IPAddress.Loopback,0);listener.Start();int port=((IPEndPoint)listener.LocalEndpoint).Port;
        string redirect="http://127.0.0.1:"+port+"/";
        string auth="https://accounts.google.com/o/oauth2/v2/auth?response_type=code&client_id="+Uri.EscapeDataString(cfg["client_id"])+"&redirect_uri="+Uri.EscapeDataString(redirect)+"&scope="+Uri.EscapeDataString("https://www.googleapis.com/auth/drive.appdata")+"&access_type=offline&prompt=consent&state="+state+"&code_challenge_method=S256&code_challenge="+challenge;
        try{Process.Start(new ProcessStartInfo(auth){UseShellExecute=true});}catch{listener.Stop();throw;}
        SetStatus("Esperando autorización de Google Drive en el navegador.");
        ThreadPool.QueueUserWorkItem(delegate{
            try{
                var accept=listener.BeginAcceptTcpClient(null,null);if(!accept.AsyncWaitHandle.WaitOne(TimeSpan.FromMinutes(3)))throw new Exception("Se agotó el tiempo para autorizar Google Drive.");
                using(TcpClient socket=listener.EndAcceptTcpClient(accept)){socket.ReceiveTimeout=10000;socket.SendTimeout=10000;
                    using(NetworkStream stream=socket.GetStream()){
                        StreamReader reader=new StreamReader(stream,Encoding.ASCII);string request=reader.ReadLine();
                        if(request==null||request.Length>8192)throw new Exception("Respuesta OAuth inválida.");
                        string path=request.Split(' ')[1];var query=System.Web.HttpUtility.ParseQueryString(new Uri(redirect.TrimEnd('/')+path).Query);
                        if(query["state"]!=state||string.IsNullOrWhiteSpace(query["code"]))throw new Exception("Autorización cancelada o respuesta inválida.");
                        byte[] reply=Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: text/plain; charset=utf-8\r\nConnection: close\r\n\r\nAutorización recibida. Puedes volver a Auto Calc.");stream.Write(reply,0,reply.Length);
                        CompleteCloudAuthorization(provider,cfg,verifier,query["code"],redirect);
                    }
                }
            }catch(Exception ex){try{BeginInvoke((MethodInvoker)delegate{MessageBox.Show(this,ex.Message,"Conectar nube");});}catch{}}
            finally{listener.Stop();}
        });
    }
    private void CompleteCloudAuthorization(string provider,Dictionary<string,string> cfg,string verifier,string code,string redirect){
        ThreadPool.QueueUserWorkItem(delegate{
            try{
                var args=new Dictionary<string,string>();args["grant_type"]="authorization_code";args["client_id"]=cfg["client_id"];args["code"]=code;args["code_verifier"]=verifier;
                if(redirect!=null){args["redirect_uri"]=redirect;args["client_secret"]=UnprotectCloud(ConfigValue(cfg,"client_secret"));}
                var response=CloudTokenRequest(provider,args);string refresh=GetString(response,"refresh_token");if(refresh.Length==0)throw new Exception("No se recibió autorización permanente; vuelve a conectar la cuenta.");
                cfg["refresh_token"]=ProtectCloud(refresh);lock(cloudTokenLock){WriteCloudConfig(provider,cfg);}
                BeginInvoke((MethodInvoker)delegate{mobileSyncProvider2940=provider;SaveLiquidacionMovilSettings2940();RefreshLiquidacionMovilUi2940();SetLiquidacionMovilStatus2940("Cuenta conectada. Pulsa Sincronizar ahora para confirmar el envío.",true);});
            }catch(Exception ex){try{BeginInvoke((MethodInvoker)delegate{MessageBox.Show(this,ex.Message,"Conectar nube");});}catch{}}
        });
    }
}}
