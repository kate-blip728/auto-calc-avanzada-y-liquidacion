using System;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace TodoAMano
{
    public partial class MainForm
    {
        private object LoadCalculatorJsonObject285(string path, string description)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            try
            {
                return serializer.DeserializeObject(File.ReadAllText(path));
            }
            catch (Exception primaryError)
            {
                string backup = path + ".bak";
                if (File.Exists(backup))
                {
                    try
                    {
                        object recovered = serializer.DeserializeObject(File.ReadAllText(backup));
                        string broken = path + ".corrupto-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
                        try { File.Copy(path, broken, false); } catch { }
                        try { File.Copy(backup, path, true); } catch { }
                        string message = "Se recuperó " + description + " desde la última copia válida. El archivo dañado se ha conservado para diagnóstico cuando ha sido posible.";
                        try
                        {
                            SetStatus(message);
                            AnnounceToScreenReader(message);
                        }
                        catch { }
                        return recovered;
                    }
                    catch { }
                }

                ReportCalculatorLoadError285(description, primaryError);
                return null;
            }
        }

        private void ReportCalculatorLoadError285(string description, Exception ex)
        {
            string message = "No se pudo cargar " + description + ". " + (ex == null ? "El archivo no es válido." : ex.Message);
            try
            {
                if (IsDisposed || Disposing) return;
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        PlayNamedSound("error");
                        SetStatus(message);
                        AnnounceToScreenReader(message);
                    });
                }
                else
                {
                    PlayNamedSound("error");
                    SetStatus(message);
                    AnnounceToScreenReader(message);
                }
            }
            catch { }
        }
    }
}

