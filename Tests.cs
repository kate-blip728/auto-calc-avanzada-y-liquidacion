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
        Check(calculatorTabs2824.TabPages.Count==8,"ocho páginas originales");
        calcTemplate.SelectedIndex=0;calculadoraInput.Text="(25 * 79,90) / 100";Calculate();
        Check(calculadoraResultado.Text=="19,975","cálculo básico y coma decimal");
        calcTemplate.SelectedIndex=1;calculadoraInput.Text="15;200";Calculate();
        Check(calculadoraResultado.Text=="30","porcentaje");
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
static class TestProgram {
    [STAThread] static int Main(){try { using(var f=new MainForm(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"test-data-"+Guid.NewGuid().ToString("N"))))f.RunTests();return 0; }catch(Exception ex){Console.WriteLine(ex);return 1;}}
}
}
