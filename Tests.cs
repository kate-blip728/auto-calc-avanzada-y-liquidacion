using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
namespace TodoAMano {
public partial class MainForm {
    private void Check(bool pass,string name){if(!pass)throw new Exception("FALLO: "+name);Console.WriteLine("OK: "+name);}
    public void RunTests(){
        soundsEnabled=false;mobileAutoSync2940=false;
        foreach(Control control in Controls) Check(!(control is TextBox && control.AccessibleName == "Estado"), "sin cuadro de edición de estado");
        ToolStripMenuItem help = null;
        foreach(ToolStripItem entry in MainMenuStrip.Items) if(entry.Text == "Mostrar mensajes de ayuda") help = (ToolStripMenuItem)entry;
        Check(help != null && helpControls.Count > 0, "opción de ayuda disponible");
        help.Checked = false; Check(!helpEnabled && File.Exists(Path.Combine(dataDir, "ocultar_ayuda.txt")), "ocultar ayuda guarda preferencia");
        using(var second = new MainForm(dataDir)) Check(!second.helpEnabled, "ayuda oculta al reabrir");
        help.Checked = true; Check(helpEnabled && !File.Exists(Path.Combine(dataDir, "ocultar_ayuda.txt")), "reactivar ayuda");
        AnnounceToScreenReader("Calculadora iniciada");
        Check(File.ReadAllText(Path.Combine(dataDir, "nvda_speech.json")).Contains("Calculadora iniciada"), "inicio enviado al lector");
        Check(calculatorTabs2824.TabPages.Count==8,"ocho páginas originales");
        AnnounceToScreenReader("aviso previo");
        string speechBefore = File.ReadAllText(Path.Combine(dataDir, "nvda_speech.json"));
        for(int tab=1; tab<calculatorTabs2824.TabPages.Count; tab++) calculatorTabs2824.SelectedIndex=tab;
        calculatorTabs2824.SelectedIndex=0;
        Check(File.ReadAllText(Path.Combine(dataDir, "nvda_speech.json")) == speechBefore, "cambiar pestañas no genera avisos de voz duplicados");
        calcTemplate.SelectedIndex=0;calculadoraInput.Text="(25 * 79,90) / 100";Calculate();
        Check(calculadoraResultado.Text=="19,975","cálculo básico y coma decimal");
        calcTemplate.SelectedIndex=1;calculadoraInput.Text="15;200";Calculate();
        Check(calculadoraResultado.Text=="30","porcentaje");
        calcTemplate.SelectedIndex=0; calculadoraInput.Text="147 / 3"; Calculate();
        Check(calculadoraResultado.Text=="49", "división de gemas");
        int historyCount = calculatorHistory2824.Count;
        calculadoraInput.Text="147/3";
        Check(TryAnnounceRepeatedCalculation() && calculadoraResultado.Text=="49", "operación repetida sin calcular");
        Check(File.ReadAllText(Path.Combine(dataDir,"nvda_speech.json")).Contains("Operación ya realizada. Resultado: 49."), "aviso enviado a NVDA");
        Check(!TryAnnounceRepeatedCalculation() && calculatorHistory2824.Count==historyCount, "sin avisos ni entradas duplicadas");
        calculadoraInput.Text="147/30"; Check(!TryAnnounceRepeatedCalculation(), "no confunde una cantidad más larga");
        calculadoraInput.Text="1 47/3"; Check(!TryAnnounceRepeatedCalculation(), "no elimina espacios dentro de números");
        calcTemplate.SelectedIndex=1; calculadoraInput.Text="147/3"; Check(!TryAnnounceRepeatedCalculation(), "operación rápida diferente no reutiliza división");
        calcTemplate.SelectedIndex=0; calculadoraInput.Text="147/3";
        repeatedCalculationEnabled.Checked=false; Check(!TryAnnounceRepeatedCalculation(), "aviso desactivable");
        using(var second = new MainForm(dataDir)) Check(!second.repeatedCalculationEnabled.Checked, "preferencia de avisos persistente");
        repeatedCalculationEnabled.Checked=true;
        calculadoraInput.Text="98/2"; Calculate();
        calculadoraInput.Text="98 / 2";
        Check(TryAnnounceRepeatedCalculation(), "guarda operaciones distintas con el mismo resultado");
        SaveCalculatorHistory2824();
        using(var second = new MainForm(dataDir)) {
            second.soundsEnabled=false; second.calcTemplate.SelectedIndex=0; second.calculadoraInput.Text="147 / 3";
            Check(second.TryAnnounceRepeatedCalculation() && second.calculadoraResultado.Text=="49", "operación reconocida después de reabrir");
        }
        Check(Math.Abs(ConvertTemperature1545(32,"Grados Fahrenheit","Grados Celsius"))<0.000001,"conversión temperatura");
        SellerLiquidation2826 item=new SellerLiquidation2826();item.Id="test-id";item.When="2026-10-07T09:00:00Z";item.UpdatedAt=item.When;item.DeviceId="test-device";
        item.Lines.Add(new SellerLine2825 { Type="Monedas", Denomination="1 €", Quantity=2, UnitValue=1, UnitsPerContainer=25 });sellerSaved2826.Add(item);
        ArrayList persisted=BuildSellerLiquidationsSnapshot285();var row=(Dictionary<string,object>)persisted[0];
        Check(Convert.ToString(row["updated_at"])==item.UpdatedAt&&Convert.ToString(row["device_id"])=="test-device","metadatos conservados en guardado diferido");
        mobileSyncProvider2940="Dropbox";
        Check(SellerRecordCloudStatus2962(item).StartsWith("pendiente"),"sin envío confirmado permanece pendiente");
        var receipt=CaptureSellerCloudVersions2962();SaveSellerCloudVersions2962(receipt,"Dropbox");
        Check(SellerRecordCloudStatus2962(item).StartsWith("sincronizada"),"confirmación por versión exacta");
        item.Note="corregida";
        Check(SellerRecordCloudStatus2962(item).StartsWith("pendiente"),"cambio posterior al envío queda pendiente");
        mobileSyncProvider2940="Google Drive";
        Check(SellerRecordCloudStatus2962(item).StartsWith("pendiente"),"confirmaciones separadas por proveedor");
        mobileSyncProvider2940="Dropbox";
        item.DeletedAt="2026-10-07T10:00:00Z";item.UpdatedAt=item.DeletedAt;RecordSellerLiquidationSyncTombstone2878(item);sellerSaved2826.Remove(item);
        var snapshot=BuildLiquidacionMovilSnapshot2940();
        Check(((IList)snapshot["liquidaciones"]).Count==1,"eliminación incluida en envío");
        var old=new Dictionary<string,object>(row);old["updated_at"]="2026-10-07T09:00:00Z";old["deleted_at"]=null;
        MergeSellerLiquidations2940(new ArrayList{old});
        Check(sellerSaved2826.Count==0,"registro remoto antiguo no resucita eliminación");
        var incoming=new Dictionary<string,object>(row);incoming["id"]="remote-new";incoming["updated_at"]="2026-10-07T11:00:00Z";incoming["deleted_at"]=null;
        MergeSellerLiquidations2940(new ArrayList{incoming});Check(sellerSaved2826.Count==1,"registro remoto nuevo se incorpora");
        incoming["updated_at"]="2026-10-07T12:00:00Z";incoming["deleted_at"]="2026-10-07T12:00:00Z";
        MergeSellerLiquidations2940(new ArrayList{incoming});Check(sellerSaved2826.Count==0,"eliminación remota retira guardado local");
        string secret="fixture-token";Check(UnprotectCloud(ProtectCloud(secret))==secret,"protección de credenciales Windows");
        SetLiquidacionMovilStatus2940("No se pudo sincronizar: sin conexión",false);LoadLiquidacionMovilSettings2940();Check(mobileLastResult2962.Contains("sin conexión"),"resultado conservado en ajustes");
    }
}
class EscapeTestForm : EscapeForm {
    public bool EscapeForTest() { return ProcessDialogKey(Keys.Escape); }
}
static class EscapeTests {
    public static void Run() {
        bool accepted = false;
        using(var modal = new EscapeTestForm()) {
            var accept = new Button { DialogResult = DialogResult.OK };
            accept.Click += delegate { accepted = true; };
            modal.Controls.Add(accept); modal.AcceptButton = accept;
            var timer = new System.Windows.Forms.Timer { Interval = 30 };
            timer.Tick += delegate { timer.Stop(); modal.EscapeForTest(); };
            modal.Shown += delegate { timer.Start(); };
            var result = modal.ShowDialog();
            timer.Dispose();
            if(result != DialogResult.Cancel || accepted) throw new Exception("Escape confirmó el formulario");
        }
        using(var window = new EscapeTestForm()) {
            bool closed = false; window.FormClosed += delegate { closed = true; };
            window.Show(); window.EscapeForTest();
            if(!closed) throw new Exception("Escape no cerró la ventana");
        }
        Console.WriteLine("OK: Escape cancela modal sin aceptar y cierra ventana independiente");
    }
}
static class TestProgram {
    [STAThread] static int Main(){try { EscapeTests.Run(); using(var f=new MainForm(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"test-data-"+Guid.NewGuid().ToString("N"))))f.RunTests();return 0; }catch(Exception ex){Console.WriteLine(ex);return 1;}}
}
}
