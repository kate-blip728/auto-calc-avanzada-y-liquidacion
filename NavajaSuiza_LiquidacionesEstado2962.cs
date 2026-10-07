using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace TodoAMano
{
    public partial class MainForm
    {
        private TextBox sellerCloudStatus2962;
        private string mobileLastResult2962 = "";
        private string SellerCloudVersion2962(SellerLiquidation2826 item)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(SellerLiquidationToSync2940(item)));
            using (SHA256 hash = SHA256.Create()) return Convert.ToBase64String(hash.ComputeHash(bytes));
        }
        private Dictionary<string, string> CaptureSellerCloudVersions2962()
        {
            Dictionary<string, string> versions = new Dictionary<string, string>();
            foreach (SellerLiquidation2826 item in sellerSaved2826) versions[item.Id] = SellerCloudVersion2962(item);
            return versions;
        }
        private string SellerCloudReceiptPath2962(string provider)
        {
            return Path.Combine(dataDir, "liquidaciones_confirmadas_" + (provider == "Google Drive" ? "drive" : "dropbox") + ".json");
        }
        private void SaveSellerCloudVersions2962(Dictionary<string, string> versions, string provider)
        {
            string path = SellerCloudReceiptPath2962(provider);
            File.WriteAllText(path + ".tmp", new JavaScriptSerializer().Serialize(versions), new UTF8Encoding(false));
            if (File.Exists(path)) File.Replace(path + ".tmp", path, path + ".bak");
            else File.Move(path + ".tmp", path);
        }
        private string SellerRecordCloudStatus2962(SellerLiquidation2826 item)
        {
            EnsureLiquidacionMovilData2940();
            if (string.IsNullOrWhiteSpace(mobileSyncProvider2940)) return "guardada localmente; nube sin configurar";
            try
            {
                string path = SellerCloudReceiptPath2962(mobileSyncProvider2940);
                if (File.Exists(path))
                {
                    Dictionary<string, string> versions = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(File.ReadAllText(path, Encoding.UTF8));
                    string version;
                    if (versions != null && versions.TryGetValue(item.Id, out version) && version == SellerCloudVersion2962(item))
                        return "sincronizada con " + mobileSyncProvider2940;
                }
            }
            catch { }
            return "pendiente de sincronizar con " + mobileSyncProvider2940;
        }
        private void RefreshSellerCloudStatus2962()
        {
            if (sellerCloudStatus2962 == null) return;
            EnsureLiquidacionMovilData2940();
            int pending = 0;
            foreach (SellerLiquidation2826 item in sellerSaved2826)
                if (!SellerRecordCloudStatus2962(item).StartsWith("sincronizada")) pending++;
            sellerCloudStatus2962.Text = "Nube: " + (string.IsNullOrWhiteSpace(mobileSyncProvider2940) ? "sin configurar" : mobileSyncProvider2940)
                + ". " + (mobileSyncBusy2940 ? "Sincronización en curso. " : "")
                + "Liquidaciones pendientes: " + pending + ". Última sincronización: "
                + (mobileLastSync2940 == DateTime.MinValue ? "sin confirmación" : mobileLastSync2940.ToString("dd/MM/yyyy HH:mm"))
                + ". " + mobileLastResult2962;
        }
    }
}
