using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace TodoAMano
{
    public partial class MainForm
    {
        private const string mobileSyncFile2940 = "navaja-liquidacion-sync.json";
        private bool mobileSyncBusy2940;
        private System.Threading.Timer mobileSyncDebounceTimer2940;
        private System.Threading.Timer mobileSyncPeriodicTimer2940;
        private bool mobileSyncBackgroundStarted2940;

        private readonly bool mobileSyncIdleHookToken2940 = InstallLiquidacionMovilIdleHook2940();
        private static bool mobileSyncIdleHookInstalled2940;

        private static bool InstallLiquidacionMovilIdleHook2940()
        {
            if (mobileSyncIdleHookInstalled2940) return true;
            mobileSyncIdleHookInstalled2940 = true;
            Application.Idle += StartLiquidacionMovilOnIdle2940;
            return true;
        }

        private static void StartLiquidacionMovilOnIdle2940(object sender, EventArgs e)
        {
            try
            {
                foreach (Form form in Application.OpenForms)
                {
                    MainForm main = form as MainForm;
                    if (main == null || main.IsDisposed) continue;
                    main.StartLiquidacionMovilBackground2940();
                    Application.Idle -= StartLiquidacionMovilOnIdle2940;
                    break;
                }
            }
            catch { }
        }

        private void StartLiquidacionMovilBackground2940()
        {
            if (mobileSyncBackgroundStarted2940) return;
            mobileSyncBackgroundStarted2940 = true;
            EnsureLiquidacionMovilData2940();
            mobileSyncPeriodicTimer2940 = new System.Threading.Timer(delegate
            {
                if (!mobileAutoSync2940 || string.IsNullOrWhiteSpace(mobileSyncProvider2940)) return;
                QueueLiquidacionMovilSync2940(false);
            }, null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(5));
        }

        private void ScheduleLiquidacionMovilSync2940()
        {
            EnsureLiquidacionMovilData2940();
            if (!mobileAutoSync2940 || string.IsNullOrWhiteSpace(mobileSyncProvider2940)) return;
            if (mobileSyncDebounceTimer2940 == null)
            {
                mobileSyncDebounceTimer2940 = new System.Threading.Timer(delegate
                {
                    QueueLiquidacionMovilSync2940(false);
                }, null, Timeout.Infinite, Timeout.Infinite);
            }
            mobileSyncDebounceTimer2940.Change(1500, Timeout.Infinite);
        }

        private bool LiquidacionMovilCloudReady2940(string provider)
        {
            try
            {
                if (string.Equals(provider, "Dropbox", StringComparison.OrdinalIgnoreCase))
                    return CloudConnected2878("Dropbox");
                if (string.Equals(provider, "Google Drive", StringComparison.OrdinalIgnoreCase))
                    return DriveConnected2851();
            }
            catch { }
            return false;
        }

        private byte[] DownloadLiquidacionMovilDropbox2940()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers["Dropbox-API-Arg"] = new JavaScriptSerializer().Serialize(
                new Dictionary<string, object> { { "path", "/" + mobileSyncFile2940 } });
            int status;
            byte[] bytes = CloudHttpBytes2878(
                "POST",
                "https://content.dropboxapi.com/2/files/download",
                null,
                "application/octet-stream",
                GetDropboxAccessToken2878(),
                headers,
                out status);
            if (status == 409)
            {
                Dictionary<string, object> failure = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(Encoding.UTF8.GetString(bytes));
                object errorRaw, pathRaw; Dictionary<string, object> error = null, path = null;
                if (failure != null && failure.TryGetValue("error", out errorRaw)) error = errorRaw as Dictionary<string, object>;
                if (error != null && error.TryGetValue("path", out pathRaw)) path = pathRaw as Dictionary<string, object>;
                if (path != null && MobileText2940(path, ".tag") == "not_found") return null;
            }
            if (status >= 400) throw new Exception("Dropbox no pudo descargar la sincronización. HTTP " + status.ToString(CultureInfo.InvariantCulture) + ".");
            return bytes;
        }

        private void UploadLiquidacionMovilDropboxFile2940(string fileName, byte[] bytes)
        {
            Dictionary<string, object> arg = new Dictionary<string, object>();
            arg["path"] = "/" + fileName;
            arg["mode"] = "overwrite";
            arg["autorename"] = false;
            arg["mute"] = true;
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers["Dropbox-API-Arg"] = new JavaScriptSerializer().Serialize(arg);
            int status;
            byte[] response = CloudHttpBytes2878(
                "POST",
                "https://content.dropboxapi.com/2/files/upload",
                bytes,
                "application/octet-stream",
                GetDropboxAccessToken2878(),
                headers,
                out status);
            if (status >= 400)
                throw new Exception("Dropbox no pudo guardar " + fileName + ". HTTP " + status.ToString(CultureInfo.InvariantCulture) + ". " + Encoding.UTF8.GetString(response));
        }

        private void UploadLiquidacionMovilDropbox2940(byte[] bytes)
        {
            UploadLiquidacionMovilDropboxFile2940(mobileSyncFile2940, bytes);
        }

        private string FindLiquidacionMovilGoogleFile2940(string fileName)
        {
            string q = "name='" + fileName.Replace("'", "\\'") + "' and trashed=false";
            string url = "https://www.googleapis.com/drive/v3/files?spaces=appDataFolder&pageSize=1&fields=" +
                Uri.EscapeDataString("files(id,name)") + "&q=" + Uri.EscapeDataString(q);
            int status;
            string text = CloudHttp2878(
                "GET", url, null, "application/json", GetDriveAccessToken2851(), null, null, out status);
            if (status >= 400)
                throw new Exception("Google Drive no pudo localizar el archivo de sincronización. HTTP " + status.ToString(CultureInfo.InvariantCulture) + ".");
            Dictionary<string, object> root = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(text);
            object raw;
            if (root == null || !root.TryGetValue("files", out raw)) return "";
            IList files = raw as IList;
            if (files == null || files.Count == 0) return "";
            Dictionary<string, object> first = files[0] as Dictionary<string, object>;
            return MobileText2940(first, "id");
        }

        private byte[] DownloadLiquidacionMovilGoogle2940()
        {
            string id = FindLiquidacionMovilGoogleFile2940(mobileSyncFile2940);
            if (string.IsNullOrWhiteSpace(id)) return null;
            int status;
            byte[] bytes = CloudHttpBytes2878(
                "GET",
                "https://www.googleapis.com/drive/v3/files/" + Uri.EscapeDataString(id) + "?alt=media",
                null,
                "application/octet-stream",
                GetDriveAccessToken2851(),
                null,
                out status);
            if (status >= 400)
                throw new Exception("Google Drive no pudo descargar la sincronización. HTTP " + status.ToString(CultureInfo.InvariantCulture) + ".");
            return bytes;
        }

        private string EnsureLiquidacionMovilGoogleFile2940(string fileName)
        {
            string id = FindLiquidacionMovilGoogleFile2940(fileName);
            if (!string.IsNullOrWhiteSpace(id)) return id;

            Dictionary<string, object> metadata = new Dictionary<string, object>();
            metadata["name"] = fileName;
            metadata["parents"] = new string[] { "appDataFolder" };
            string body = new JavaScriptSerializer().Serialize(metadata);
            int status;
            string response = CloudHttp2878(
                "POST",
                "https://www.googleapis.com/drive/v3/files?fields=id",
                body,
                "application/json; charset=utf-8",
                GetDriveAccessToken2851(),
                null,
                null,
                out status);
            if (status >= 400)
                throw new Exception("Google Drive no pudo crear el archivo de sincronización. HTTP " + status.ToString(CultureInfo.InvariantCulture) + ".");
            Dictionary<string, object> created = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(response);
            id = MobileText2940(created, "id");
            if (string.IsNullOrWhiteSpace(id)) throw new Exception("Google Drive no devolvió el identificador del archivo de sincronización.");
            return id;
        }

        private void UploadLiquidacionMovilGoogleFile2940(string fileName, byte[] bytes, string contentType)
        {
            string id = EnsureLiquidacionMovilGoogleFile2940(fileName);
            int status;
            byte[] response = CloudHttpBytes2878(
                "PATCH",
                "https://www.googleapis.com/upload/drive/v3/files/" + Uri.EscapeDataString(id) + "?uploadType=media",
                bytes,
                string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
                GetDriveAccessToken2851(),
                null,
                out status);
            if (status >= 400)
                throw new Exception("Google Drive no pudo guardar " + fileName + ". HTTP " + status.ToString(CultureInfo.InvariantCulture) + ". " + Encoding.UTF8.GetString(response));
        }

        private void UploadLiquidacionMovilGoogle2940(byte[] bytes)
        {
            UploadLiquidacionMovilGoogleFile2940(mobileSyncFile2940, bytes, "application/json; charset=utf-8");
        }

        private void UploadLiquidacionMovilCloudFile2940(string fileName, byte[] bytes, string contentType)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Falta el nombre del archivo.");
            if (fileName.IndexOf('/') >= 0 || fileName.IndexOf('\\') >= 0)
                throw new ArgumentException("El nombre del archivo de nube no es válido.");

            if (string.Equals(mobileSyncProvider2940, "Dropbox", StringComparison.OrdinalIgnoreCase))
            {
                UploadLiquidacionMovilDropboxFile2940(fileName, bytes);
                return;
            }
            if (string.Equals(mobileSyncProvider2940, "Google Drive", StringComparison.OrdinalIgnoreCase))
            {
                UploadLiquidacionMovilGoogleFile2940(fileName, bytes, contentType);
                return;
            }
            throw new Exception("Selecciona Dropbox o Google Drive para la sincronización móvil.");
        }

        private byte[] DownloadLiquidacionMovilCloud2940(string provider)
        {
            if (string.Equals(provider, "Dropbox", StringComparison.OrdinalIgnoreCase))
                return DownloadLiquidacionMovilDropbox2940();
            if (string.Equals(provider, "Google Drive", StringComparison.OrdinalIgnoreCase))
                return DownloadLiquidacionMovilGoogle2940();
            throw new Exception("Selecciona Dropbox o Google Drive para la sincronización móvil.");
        }

        private void UploadLiquidacionMovilCloud2940(byte[] bytes, string provider)
        {
            if (string.Equals(provider, "Dropbox", StringComparison.OrdinalIgnoreCase))
            {
                UploadLiquidacionMovilDropbox2940(bytes);
                return;
            }
            if (string.Equals(provider, "Google Drive", StringComparison.OrdinalIgnoreCase))
            {
                UploadLiquidacionMovilGoogle2940(bytes);
                return;
            }
            throw new Exception("Selecciona Dropbox o Google Drive para la sincronización móvil.");
        }

        private void QueueLiquidacionMovilSync2940(bool announce)
        {
            if (InvokeRequired) { try { BeginInvoke((MethodInvoker)delegate { QueueLiquidacionMovilSync2940(announce); }); } catch { } return; }
            EnsureLiquidacionMovilData2940();
            if (string.IsNullOrWhiteSpace(sellerSavedPath2826)) { sellerSavedPath2826 = System.IO.Path.Combine(dataDir, "calculadora_vendedor_liquidaciones.json"); LoadSellerLiquidations2826(); }
            if (mobileSyncBusy2940)
            {
                if (announce) AnnounceToScreenReader("Ya hay una sincronización móvil en curso.");
                return;
            }
            if (string.IsNullOrWhiteSpace(mobileSyncProvider2940))
            {
                SetLiquidacionMovilStatus2940("Sin sincronizar: selecciona Dropbox o Google Drive en Configurar nube.", announce);
                return;
            }
            if (!LiquidacionMovilCloudReady2940(mobileSyncProvider2940))
            {
                string disconnected = "Sin sincronizar: " + mobileSyncProvider2940 + " no está conectado en Nubes por API.";
                SetLiquidacionMovilStatus2940(disconnected, announce || disconnected != mobileLastResult2962);
                return;
            }

            string syncProvider = mobileSyncProvider2940;
            mobileSyncBusy2940 = true;
            SetLiquidacionMovilStatus2940("Sincronizando con " + mobileSyncProvider2940 + "...", announce);
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    int changed = 0;
                    bool localChanges = false;
                    Dictionary<string, object> remoteRoot = null;
                    byte[] remote = DownloadLiquidacionMovilCloud2940(syncProvider);
                    if (remote != null && remote.Length > 0)
                    {
                        string json = Encoding.UTF8.GetString(remote);
                        remoteRoot = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(json);
                        if (remoteRoot == null) throw new FormatException("El archivo de nube no contiene un objeto de sincronización válido.");
                    }

                    byte[] payload = null;
                    Dictionary<string, string> acknowledged = null;
                    Invoke((MethodInvoker)delegate
                    {
                        foreach (SellerLiquidation2826 item in sellerSaved2826)
                            if (!SellerRecordCloudStatus2962(item).StartsWith("sincronizada")) localChanges = true;
                        foreach (Dictionary<string, object> row in mobileInventory2940)
                            if (MobileDate2940(row, "updated_at") > mobileLastSync2940.ToUniversalTime()) localChanges = true;
                        foreach (Dictionary<string, object> row in mobileScratchBooks2940)
                            if (MobileDate2940(row, "updated_at") > mobileLastSync2940.ToUniversalTime()) localChanges = true;
                        if (remoteRoot != null)
                            changed = MergeLiquidacionMovilSnapshot2940(remoteRoot);

                        if (changed > 0)
                        {
                            QueueSellerLiquidationsSave285();
                            RefreshSellerSavedList2826();
                            RefreshLiquidacionMovilUi2940();
                        }

                        Dictionary<string, object> snapshot = BuildLiquidacionMovilSnapshot2940();
                        payload = Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(snapshot));
                        acknowledged = CaptureSellerCloudVersions2962();
                    });

                    UploadLiquidacionMovilCloud2940(payload, syncProvider);

                    // Confirmar exactamente las versiones enviadas, en el hilo de interfaz.
                    try
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            mobileSyncBusy2940 = false;
                            mobileLastSync2940 = DateTime.Now;
                            SaveSellerCloudVersions2962(acknowledged, syncProvider);
                            RefreshSellerSavedList2826();
                            SaveLiquidacionMovilSettings2940();
                            RefreshLiquidacionMovilUi2940();
                            string text = changed > 0
                                ? "Sincronización terminada. Se incorporaron " + changed.ToString(CultureInfo.CurrentCulture) + (changed == 1 ? " cambio." : " cambios.")
                                : (localChanges ? "Sincronización terminada. Se enviaron cambios locales a " + syncProvider + "." : "Sincronización terminada. Todo está al día.");
                            text += " Última sincronización: " + mobileLastSync2940.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture) + ".";
                            SetLiquidacionMovilStatus2940(text, announce || changed > 0 || localChanges);
                            if (changed > 0 || localChanges) PlayNamedSound("aviso");
                        });
                    }
                    catch { mobileSyncBusy2940 = false; }
                }
                catch (Exception ex)
                {
                    try
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            mobileSyncBusy2940 = false;
                            string error = "No se pudo sincronizar: " + ex.Message;
                            SetLiquidacionMovilStatus2940(error, announce || error != mobileLastResult2962);
                            if (announce) PlayNamedSound("error");
                        });
                    }
                    catch { mobileSyncBusy2940 = false; }
                }
            });
        }
    }
}
