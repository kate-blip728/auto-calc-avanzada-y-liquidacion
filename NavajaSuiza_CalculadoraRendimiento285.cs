using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TodoAMano
{
    public partial class MainForm
    {
        private readonly object calculatorPersistenceLock285 = new object();
        private System.Windows.Forms.Timer sellerDraftSaveTimer285;
        private System.Windows.Forms.Timer calculatorHistorySaveTimer285;
        private int sellerDraftSaveGeneration285;
        private int sellerSavedSaveGeneration285;
        private int calculatorHistorySaveGeneration285;
        private bool calculatorPersistenceInitialized285;
        private bool sellerDraftDirty285;
        private bool calculatorHistoryDirty285;
        private DateTime lastSellerExplicitSave285 = DateTime.MinValue;

        private void EnsureCalculatorPersistence285()
        {
            if (calculatorPersistenceInitialized285) return;
            calculatorPersistenceInitialized285 = true;

            sellerDraftSaveTimer285 = new System.Windows.Forms.Timer();
            sellerDraftSaveTimer285.Interval = 180;
            sellerDraftSaveTimer285.Tick += delegate
            {
                sellerDraftSaveTimer285.Stop();
                if (!sellerDraftDirty285) return;
                sellerDraftDirty285 = false;
                QueueSellerDraftSnapshot285();
            };

            calculatorHistorySaveTimer285 = new System.Windows.Forms.Timer();
            calculatorHistorySaveTimer285.Interval = 240;
            calculatorHistorySaveTimer285.Tick += delegate
            {
                calculatorHistorySaveTimer285.Stop();
                if (!calculatorHistoryDirty285) return;
                calculatorHistoryDirty285 = false;
                QueueCalculatorHistorySnapshot285();
            };

            FormClosing += delegate { FlushCalculatorPersistence285(); };
        }

        private void ScheduleSellerDraftSave285()
        {
            if (sellerLoading2825 || string.IsNullOrWhiteSpace(sellerDraftPath2825)) return;
            EnsureCalculatorPersistence285();
            Interlocked.Increment(ref sellerDraftSaveGeneration285);
            sellerDraftDirty285 = true;
            sellerDraftSaveTimer285.Stop();
            sellerDraftSaveTimer285.Start();
        }

        private void QueueSellerDraftSnapshot285()
        {
            if (string.IsNullOrWhiteSpace(sellerDraftPath2825)) return;
            ArrayList snapshot = BuildSellerDraftSnapshot285();
            int generation = Thread.VolatileRead(ref sellerDraftSaveGeneration285);
            QueueCalculatorJsonWrite285(
                sellerDraftPath2825,
                snapshot,
                generation,
                delegate { return Thread.VolatileRead(ref sellerDraftSaveGeneration285); },
                "el borrador automático de la liquidación");
        }

        private ArrayList BuildSellerDraftSnapshot285()
        {
            ArrayList list = new ArrayList();
            foreach (SellerLine2825 line in sellerLines2825)
            {
                list.Add(SellerLineToDictionary2826(line));
            }
            return list;
        }

        private void QueueSellerLiquidationsSave285()
        {
            if (string.IsNullOrWhiteSpace(sellerSavedPath2826)) return;
            EnsureCalculatorPersistence285();
            ArrayList snapshot = BuildSellerLiquidationsSnapshot285();
            ScheduleLiquidacionMovilSync2940();
            int generation = Interlocked.Increment(ref sellerSavedSaveGeneration285);
            QueueCalculatorJsonWrite285(
                sellerSavedPath2826,
                snapshot,
                generation,
                delegate { return Thread.VolatileRead(ref sellerSavedSaveGeneration285); },
                "las liquidaciones guardadas");
        }

        private ArrayList BuildSellerLiquidationsSnapshot285()
        {
            ArrayList list = new ArrayList();
            foreach (SellerLiquidation2826 saved in sellerSaved2826)
            {
                Dictionary<string, object> data = new Dictionary<string, object>();
                data["id"] = saved.Id;
                data["fecha"] = saved.When;
                data["nota"] = saved.Note;
                data["updated_at"] = string.IsNullOrWhiteSpace(saved.UpdatedAt) ? saved.When : saved.UpdatedAt;
                data["deleted_at"] = string.IsNullOrWhiteSpace(saved.DeletedAt) ? null : (object)saved.DeletedAt;
                data["device_id"] = saved.DeviceId;

                ArrayList rows = new ArrayList();
                foreach (SellerLine2825 line in saved.Lines) rows.Add(SellerLineToDictionary2826(line));
                data["filas"] = rows;

                ArrayList revisions = new ArrayList();
                foreach (SellerRevision2827 revision in saved.Revisions)
                {
                    Dictionary<string, object> revisionData = new Dictionary<string, object>();
                    revisionData["fecha_correccion"] = revision.CorrectedAt;
                    revisionData["fecha_anterior"] = revision.PreviousWhen;
                    revisionData["nota_anterior"] = revision.PreviousNote;
                    revisionData["resumen"] = revision.ChangeSummary;
                    ArrayList previousRows = new ArrayList();
                    foreach (SellerLine2825 line in revision.PreviousLines) previousRows.Add(SellerLineToDictionary2826(line));
                    revisionData["filas_anteriores"] = previousRows;
                    revisions.Add(revisionData);
                }
                if (revisions.Count > 0) data["correcciones"] = revisions;
                list.Add(data);
            }
            return list;
        }

        private void ScheduleCalculatorHistorySave285()
        {
            if (calculatorHistoryLoading2824 || string.IsNullOrWhiteSpace(calculatorHistoryPath2824)) return;
            EnsureCalculatorPersistence285();
            Interlocked.Increment(ref calculatorHistorySaveGeneration285);
            calculatorHistoryDirty285 = true;
            calculatorHistorySaveTimer285.Stop();
            calculatorHistorySaveTimer285.Start();
        }

        private void QueueCalculatorHistorySnapshot285()
        {
            if (string.IsNullOrWhiteSpace(calculatorHistoryPath2824)) return;
            ArrayList snapshot = BuildCalculatorHistorySnapshot285();
            int generation = Thread.VolatileRead(ref calculatorHistorySaveGeneration285);
            QueueCalculatorJsonWrite285(
                calculatorHistoryPath2824,
                snapshot,
                generation,
                delegate { return Thread.VolatileRead(ref calculatorHistorySaveGeneration285); },
                "el historial de la calculadora");
        }

        private ArrayList BuildCalculatorHistorySnapshot285()
        {
            ArrayList list = new ArrayList();
            foreach (CalculatorHistoryItem2824 h in calculatorHistory2824)
            {
                Dictionary<string, object> item = new Dictionary<string, object>();
                item["fecha"] = h.When;
                item["tipo"] = h.Kind;
                item["entrada"] = h.Input;
                item["resultado"] = h.Result;
                item["destino"] = h.ReuseTarget;
                list.Add(item);
            }
            return list;
        }

        private void QueueCalculatorJsonWrite285(string path, object snapshot, int generation, Func<int> currentGeneration, string description)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    lock (calculatorPersistenceLock285)
                    {
                        if (generation != currentGeneration()) return;
                        string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(snapshot);
                        if (generation != currentGeneration()) return;
                        AtomicWriteCalculatorJson285(path, json);
                    }
                }
                catch (Exception ex)
                {
                    ReportCalculatorPersistenceError285(description, ex);
                }
            });
        }

        private void AtomicWriteCalculatorJson285(string path, string json)
        {
            string directory = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(directory)) directory = dataDir;
            Directory.CreateDirectory(directory);

            string temp = path + ".tmp." + Guid.NewGuid().ToString("N");
            string backup = path + ".bak";
            try
            {
                File.WriteAllText(temp, json ?? "", new UTF8Encoding(false));
                if (File.Exists(path))
                {
                    try { File.Copy(path, backup, true); } catch { }
                    try
                    {
                        File.Replace(temp, path, null, true);
                    }
                    catch
                    {
                        File.Copy(temp, path, true);
                        try { File.Delete(temp); } catch { }
                    }
                }
                else
                {
                    File.Move(temp, path);
                }
            }
            finally
            {
                try { if (File.Exists(temp)) File.Delete(temp); } catch { }
            }
        }

        private void ReportCalculatorPersistenceError285(string description, Exception ex)
        {
            string message = "No se pudo guardar " + description + ". " + (ex == null ? "Error de almacenamiento." : ex.Message);
            try
            {
                if (IsDisposed || Disposing) return;
                BeginInvoke((MethodInvoker)delegate
                {
                    PlayNamedSound("error");
                    SetStatus(message);
                    AnnounceToScreenReader(message);
                });
            }
            catch { }
        }

        private bool CanStartSellerLiquidationSave285()
        {
            DateTime now = DateTime.UtcNow;
            if (lastSellerExplicitSave285 != DateTime.MinValue && now - lastSellerExplicitSave285 < TimeSpan.FromMilliseconds(900))
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("La liquidación ya se está guardando. No hace falta pulsar Guardar otra vez.");
                return false;
            }
            lastSellerExplicitSave285 = now;
            return true;
        }

        private void RefreshSellerSavedIfVisible285()
        {
            if (calculatorTabs2824 != null && calculatorSellerHistoryPage2826 != null && calculatorTabs2824.SelectedTab == calculatorSellerHistoryPage2826)
                RefreshSellerSavedList2826();
        }

        private void RefreshCalculatorHistoryIfVisible285(int selected)
        {
            if (calculatorTabs2824 != null && calculatorHistoryPage2824 != null && calculatorTabs2824.SelectedTab == calculatorHistoryPage2824)
                RefreshHistoryList2824(selected);
        }

        private void FlushCalculatorPersistence285()
        {
            if (!calculatorPersistenceInitialized285) return;
            try { if (sellerDraftSaveTimer285 != null) sellerDraftSaveTimer285.Stop(); } catch { }
            try { if (calculatorHistorySaveTimer285 != null) calculatorHistorySaveTimer285.Stop(); } catch { }

            ArrayList draft = string.IsNullOrWhiteSpace(sellerDraftPath2825) ? null : BuildSellerDraftSnapshot285();
            ArrayList saved = string.IsNullOrWhiteSpace(sellerSavedPath2826) ? null : BuildSellerLiquidationsSnapshot285();
            ArrayList history = string.IsNullOrWhiteSpace(calculatorHistoryPath2824) ? null : BuildCalculatorHistorySnapshot285();

            Interlocked.Increment(ref sellerDraftSaveGeneration285);
            Interlocked.Increment(ref sellerSavedSaveGeneration285);
            Interlocked.Increment(ref calculatorHistorySaveGeneration285);

            lock (calculatorPersistenceLock285)
            {
                try
                {
                    System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    if (draft != null) AtomicWriteCalculatorJson285(sellerDraftPath2825, serializer.Serialize(draft));
                    if (saved != null) AtomicWriteCalculatorJson285(sellerSavedPath2826, serializer.Serialize(saved));
                    if (history != null) AtomicWriteCalculatorJson285(calculatorHistoryPath2824, serializer.Serialize(history));
                }
                catch (Exception ex)
                {
                    try { SetStatus("No se pudo completar el guardado final de la calculadora. " + ex.Message); } catch { }
                }
            }
            sellerDraftDirty285 = false;
            calculatorHistoryDirty285 = false;
        }
    }
}
