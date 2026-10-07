using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace TodoAMano
{
    public partial class MainForm
    {
        private readonly List<Dictionary<string, object>> mobileInventory2940 = new List<Dictionary<string, object>>();
        private readonly List<Dictionary<string, object>> mobileScratchBooks2940 = new List<Dictionary<string, object>>();
        private bool mobileDataLoaded2940;
        private string mobileSyncProvider2940 = "";
        private bool mobileAutoSync2940 = true;
        private DateTime mobileLastSync2940 = DateTime.MinValue;

        private string LiquidacionMovilDataPath2940()
        {
            return Path.Combine(dataDir, "liquidacion_movil_datos.json");
        }

        private string LiquidacionMovilSettingsPath2940()
        {
            return Path.Combine(dataDir, "liquidacion_movil_ajustes.json");
        }

        private string LiquidacionMovilDevicePath2940()
        {
            return Path.Combine(dataDir, "liquidacion_movil_device_id.txt");
        }

        private string LiquidacionMovilDeviceId2940()
        {
            try
            {
                Directory.CreateDirectory(dataDir);
                string path = LiquidacionMovilDevicePath2940();
                if (File.Exists(path))
                {
                    string existing = File.ReadAllText(path, Encoding.UTF8).Trim();
                    if (existing.Length > 0) return existing;
                }
                string created = "windows-" + Guid.NewGuid().ToString("N");
                File.WriteAllText(path, created, new UTF8Encoding(false));
                return created;
            }
            catch
            {
                return "windows-" + Environment.MachineName;
            }
        }

        private string MobileText2940(Dictionary<string, object> data, string key)
        {
            object value;
            return data != null && data.TryGetValue(key, out value) && value != null
                ? Convert.ToString(value, CultureInfo.InvariantCulture) ?? ""
                : "";
        }

        private int MobileInt2940(Dictionary<string, object> data, string key, int fallback)
        {
            object value;
            if (data == null || !data.TryGetValue(key, out value) || value == null) return fallback;
            try { return Convert.ToInt32(value, CultureInfo.InvariantCulture); }
            catch { return fallback; }
        }

        private DateTime MobileDate2940(Dictionary<string, object> data, string key)
        {
            string raw = MobileText2940(data, key);
            DateTime parsed;
            return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed)
                ? parsed.ToUniversalTime()
                : DateTime.MinValue;
        }

        private bool MobileDeleted2940(Dictionary<string, object> data)
        {
            return !string.IsNullOrWhiteSpace(MobileText2940(data, "deleted_at"));
        }

        private Dictionary<string, object> CloneMobileDictionary2940(Dictionary<string, object> source)
        {
            if (source == null) return new Dictionary<string, object>();
            string json = new JavaScriptSerializer().Serialize(source);
            return new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(json);
        }

        private void LoadLiquidacionMovilSettings2940()
        {
            try
            {
                string path = LiquidacionMovilSettingsPath2940();
                if (!File.Exists(path)) return;
                Dictionary<string, object> root = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(File.ReadAllText(path, Encoding.UTF8));
                mobileSyncProvider2940 = MobileText2940(root, "provider");
                mobileLastResult2962 = MobileText2940(root, "last_result");
                object auto;
                if (root != null && root.TryGetValue("auto_sync", out auto) && auto != null)
                    mobileAutoSync2940 = Convert.ToBoolean(auto, CultureInfo.InvariantCulture);
                DateTime last;
                if (DateTime.TryParse(MobileText2940(root, "last_sync_at"), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out last))
                    mobileLastSync2940 = last.ToLocalTime();
            }
            catch { }
        }

        private void SaveLiquidacionMovilSettings2940()
        {
            try
            {
                Directory.CreateDirectory(dataDir);
                Dictionary<string, object> root = new Dictionary<string, object>();
                root["provider"] = mobileSyncProvider2940 ?? "";
                root["auto_sync"] = mobileAutoSync2940;
                root["last_result"] = mobileLastResult2962;
                root["last_sync_at"] = mobileLastSync2940 == DateTime.MinValue ? "" : mobileLastSync2940.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
                File.WriteAllText(LiquidacionMovilSettingsPath2940(), new JavaScriptSerializer().Serialize(root), new UTF8Encoding(false));
            }
            catch { }
        }

        private void EnsureLiquidacionMovilData2940()
        {
            if (mobileDataLoaded2940) return;
            mobileDataLoaded2940 = true;
            LoadLiquidacionMovilSettings2940();
            mobileInventory2940.Clear();
            mobileScratchBooks2940.Clear();
            try
            {
                string path = LiquidacionMovilDataPath2940();
                if (!File.Exists(path)) return;
                Dictionary<string, object> root = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(File.ReadAllText(path, Encoding.UTF8));
                LoadMobileCollection2940(root, "inventario", mobileInventory2940);
                LoadMobileCollection2940(root, "rascas_libros", mobileScratchBooks2940);
            }
            catch { }
        }

        private void LoadMobileCollection2940(Dictionary<string, object> root, string key, List<Dictionary<string, object>> target)
        {
            object raw;
            if (root == null || !root.TryGetValue(key, out raw)) return;
            IEnumerable list = raw as IEnumerable;
            if (list == null) return;
            foreach (object entry in list)
            {
                Dictionary<string, object> item = entry as Dictionary<string, object>;
                if (item != null) target.Add(item);
            }
        }

        private void SaveLiquidacionMovilData2940()
        {
            try
            {
                EnsureLiquidacionMovilData2940();
                Directory.CreateDirectory(dataDir);
                Dictionary<string, object> root = new Dictionary<string, object>();
                root["version"] = 2;
                root["inventario"] = mobileInventory2940;
                root["rascas_libros"] = mobileScratchBooks2940;
                File.WriteAllText(LiquidacionMovilDataPath2940(), new JavaScriptSerializer().Serialize(root), new UTF8Encoding(false));
            }
            catch { }
        }

        private void StampMobileItem2940(Dictionary<string, object> item)
        {
            if (item == null) return;
            item["updated_at"] = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            item["device_id"] = LiquidacionMovilDeviceId2940();
        }

        private int MergeMobileCollection2940(List<Dictionary<string, object>> local, IList remote)
        {
            int changed = 0;
            Dictionary<string, int> index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < local.Count; i++)
            {
                string id = MobileText2940(local[i], "id");
                if (id.Length > 0) index[id] = i;
            }

            if (remote == null) return changed;
            foreach (object raw in remote)
            {
                Dictionary<string, object> incoming = raw as Dictionary<string, object>;
                if (incoming == null) continue;

                string id = MobileText2940(incoming, "id");
                if (id.Length == 0) continue;

                int pos;
                if (!index.TryGetValue(id, out pos))
                {
                    local.Add(CloneMobileDictionary2940(incoming));
                    index[id] = local.Count - 1;
                    changed++;
                    continue;
                }

                DateTime remoteUpdated = MobileDate2940(incoming, "updated_at");
                DateTime localUpdated = MobileDate2940(local[pos], "updated_at");
                if (remoteUpdated > localUpdated)
                {
                    local[pos] = CloneMobileDictionary2940(incoming);
                    changed++;
                }
            }
            return changed;
        }

        private Dictionary<string, object> SellerRevisionToSync2940(SellerRevision2827 revision)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["fecha_correccion"] = revision.CorrectedAt;
            data["fecha_anterior"] = revision.PreviousWhen;
            data["nota_anterior"] = revision.PreviousNote;
            data["resumen"] = revision.ChangeSummary;
            ArrayList rows = new ArrayList();
            foreach (SellerLine2825 line in revision.PreviousLines) rows.Add(SellerLineToDictionary2826(line));
            data["filas_anteriores"] = rows;
            return data;
        }

        private Dictionary<string, object> SellerLiquidationToSync2940(SellerLiquidation2826 item)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["id"] = item.Id;
            data["fecha"] = item.When;
            data["nota"] = item.Note ?? "";
            ArrayList rows = new ArrayList();
            foreach (SellerLine2825 line in item.Lines) rows.Add(SellerLineToDictionary2826(line));
            data["filas"] = rows;
            ArrayList revisions = new ArrayList();
            foreach (SellerRevision2827 revision in item.Revisions) revisions.Add(SellerRevisionToSync2940(revision));
            if (revisions.Count > 0) data["correcciones"] = revisions;
            data["updated_at"] = string.IsNullOrWhiteSpace(item.UpdatedAt) ? item.When : item.UpdatedAt;
            data["deleted_at"] = string.IsNullOrWhiteSpace(item.DeletedAt) ? null : (object)item.DeletedAt;
            data["device_id"] = item.DeviceId ?? "";
            return data;
        }

        private SellerLiquidation2826 SellerLiquidationFromSync2940(Dictionary<string, object> data)
        {
            if (data == null) return null;
            SellerLiquidation2826 item = new SellerLiquidation2826();
            item.Id = MobileText2940(data, "id");
            if (item.Id.Length == 0) return null;
            item.When = MobileText2940(data, "fecha");
            item.Note = MobileText2940(data, "nota");
            item.UpdatedAt = MobileText2940(data, "updated_at");
            if (item.UpdatedAt.Length == 0) item.UpdatedAt = item.When;
            item.DeletedAt = MobileText2940(data, "deleted_at");
            item.DeviceId = MobileText2940(data, "device_id");

            object rawRows;
            if (data.TryGetValue("filas", out rawRows))
            {
                IEnumerable rows = rawRows as IEnumerable;
                if (rows != null) foreach (object raw in rows)
                {
                    SellerLine2825 line = SellerLineFromDictionary2826(raw as Dictionary<string, object>);
                    if (line != null) item.Lines.Add(line);
                }
            }

            object rawCorrections;
            if (data.TryGetValue("correcciones", out rawCorrections))
            {
                IEnumerable corrections = rawCorrections as IEnumerable;
                if (corrections != null) foreach (object raw in corrections)
                {
                    Dictionary<string, object> source = raw as Dictionary<string, object>;
                    if (source == null) continue;
                    SellerRevision2827 revision = new SellerRevision2827();
                    revision.CorrectedAt = MobileText2940(source, "fecha_correccion");
                    revision.PreviousWhen = MobileText2940(source, "fecha_anterior");
                    revision.PreviousNote = MobileText2940(source, "nota_anterior");
                    revision.ChangeSummary = MobileText2940(source, "resumen");
                    object previousRaw;
                    if (source.TryGetValue("filas_anteriores", out previousRaw))
                    {
                        IEnumerable previous = previousRaw as IEnumerable;
                        if (previous != null) foreach (object rowRaw in previous)
                        {
                            SellerLine2825 line = SellerLineFromDictionary2826(rowRaw as Dictionary<string, object>);
                            if (line != null) revision.PreviousLines.Add(line);
                        }
                    }
                    item.Revisions.Add(revision);
                }
            }
            return item;
        }

        private DateTime SellerUpdatedDate2940(SellerLiquidation2826 item)
        {
            DateTime parsed;
            string raw = item == null ? "" : (string.IsNullOrWhiteSpace(item.UpdatedAt) ? item.When : item.UpdatedAt);
            return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed)
                ? parsed.ToUniversalTime()
                : DateTime.MinValue;
        }

        private void CopySellerLiquidation2940(SellerLiquidation2826 target, SellerLiquidation2826 source)
        {
            target.When = source.When;
            target.Note = source.Note;
            target.UpdatedAt = source.UpdatedAt;
            target.DeletedAt = source.DeletedAt;
            target.DeviceId = source.DeviceId;
            target.Lines.Clear();
            foreach (SellerLine2825 line in source.Lines) target.Lines.Add(CloneSellerLine2826(line));
            target.Revisions.Clear();
            foreach (SellerRevision2827 srcRevision in source.Revisions)
            {
                SellerRevision2827 revision = new SellerRevision2827();
                revision.CorrectedAt = srcRevision.CorrectedAt;
                revision.PreviousWhen = srcRevision.PreviousWhen;
                revision.PreviousNote = srcRevision.PreviousNote;
                revision.ChangeSummary = srcRevision.ChangeSummary;
                foreach (SellerLine2825 line in srcRevision.PreviousLines) revision.PreviousLines.Add(CloneSellerLine2826(line));
                target.Revisions.Add(revision);
            }
        }

        private int MergeSellerLiquidations2940(IList remote)
        {
            if (remote == null) return 0;
            int changed = 0;
            Dictionary<string, DateTime> deletedDates = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            string deletedPath = Path.Combine(dataDir, "navaja-liquidacion-tombstones.json");
            if (File.Exists(deletedPath)) { IList deleted = new JavaScriptSerializer().DeserializeObject(File.ReadAllText(deletedPath, Encoding.UTF8)) as IList;
                if (deleted != null) foreach (object rawDeleted in deleted) { Dictionary<string, object> row = rawDeleted as Dictionary<string, object>; if (row != null) deletedDates[MobileText2940(row, "id")] = MobileDate2940(row, "updated_at"); } }
            Dictionary<string, SellerLiquidation2826> index = new Dictionary<string, SellerLiquidation2826>(StringComparer.OrdinalIgnoreCase);
            foreach (SellerLiquidation2826 local in sellerSaved2826)
                if (!string.IsNullOrWhiteSpace(local.Id)) index[local.Id] = local;

            foreach (object raw in remote)
            {
                SellerLiquidation2826 incoming = SellerLiquidationFromSync2940(raw as Dictionary<string, object>);
                if (incoming == null) continue;
                DateTime deletedDate;
                if (deletedDates.TryGetValue(incoming.Id, out deletedDate) && deletedDate >= SellerUpdatedDate2940(incoming)) continue;
                SellerLiquidation2826 local;
                if (!index.TryGetValue(incoming.Id, out local))
                {
                    if (!string.IsNullOrWhiteSpace(incoming.DeletedAt)) { RecordSellerLiquidationSyncTombstone2878(incoming); changed++; continue; }
                    sellerSaved2826.Add(incoming);
                    index[incoming.Id] = incoming;
                    changed++;
                    continue;
                }
                if (SellerUpdatedDate2940(incoming) > SellerUpdatedDate2940(local))
                {
                    if (!string.IsNullOrWhiteSpace(incoming.DeletedAt)) { RecordSellerLiquidationSyncTombstone2878(incoming); sellerSaved2826.Remove(local); index.Remove(incoming.Id); }
                    else CopySellerLiquidation2940(local, incoming);
                    changed++;
                }
            }
            return changed;
        }

        private Dictionary<string, object> BuildLiquidacionMovilSnapshot2940()
        {
            EnsureLiquidacionMovilData2940();
            Dictionary<string, object> root = new Dictionary<string, object>();
            root["version"] = 2;
            root["device_id"] = LiquidacionMovilDeviceId2940();
            root["generated_at"] = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            root["inventario"] = mobileInventory2940;
            root["rascas_libros"] = mobileScratchBooks2940;
            ArrayList liquidations = new ArrayList();
            foreach (SellerLiquidation2826 item in sellerSaved2826)
                liquidations.Add(SellerLiquidationToSync2940(item));
            string tombstonePath = Path.Combine(dataDir, "navaja-liquidacion-tombstones.json");
            if (File.Exists(tombstonePath)) { IList deleted = new JavaScriptSerializer().DeserializeObject(File.ReadAllText(tombstonePath, Encoding.UTF8)) as IList; if (deleted != null) liquidations.AddRange(deleted); }
            root["liquidaciones"] = liquidations;
            return root;
        }

        private int MergeLiquidacionMovilSnapshot2940(Dictionary<string, object> root)
        {
            EnsureLiquidacionMovilData2940();
            if (root == null) return 0;
            int changed = 0;
            object raw;
            if (root.TryGetValue("inventario", out raw))
                changed += MergeMobileCollection2940(mobileInventory2940, raw as IList);
            if (root.TryGetValue("rascas_libros", out raw))
                changed += MergeMobileCollection2940(mobileScratchBooks2940, raw as IList);
            if (root.TryGetValue("liquidaciones", out raw))
                changed += MergeSellerLiquidations2940(raw as IList);

            if (changed > 0)
            {
                // Sólo persiste el modelo aquí. El refresco de WinForms se
                // realiza en el hilo de interfaz desde el coordinador de nube.
                SaveLiquidacionMovilData2940();
            }
            return changed;
        }
    }
}
