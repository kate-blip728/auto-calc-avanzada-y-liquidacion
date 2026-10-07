using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace TodoAMano {
public partial class MainForm : Form {
    private string dataDir;
    private bool soundsEnabled = true;
    private TextBox calculadoraInput, calculadoraResultado, dadosInput, fechaInput;
    private NumericUpDown diceCount1532, diceModifier1532;
    private ComboBox diceSides1532;
    private bool diceControlsUpdating1532;
    private ComboBox calcTemplate;
    private Random random = new Random();
    private JavaScriptSerializer json = new JavaScriptSerializer();
    private System.Windows.Forms.Timer sellerDraftSaveTimer2930;
    private bool sellerDraftSavePending2930, sellerPerformanceInstalled2930;
    private bool helpEnabled = true;
    private readonly List<Control> helpControls = new List<Control>();
    private Label statusLabel;
    [DllImport("nvdaControllerClient64.dll", CharSet=CharSet.Unicode)]
    private static extern int nvdaController_speakText(string text);
    [DllImport("nvdaControllerClient32.dll", EntryPoint="nvdaController_speakText", CharSet=CharSet.Unicode)]
    private static extern int Speak32(string text);
    public MainForm() : this(null) {}
    public MainForm(string directory) {
        dataDir = directory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoCalcAvanzadaLiquidacion");
        Directory.CreateDirectory(dataDir);
        Text = "Auto Calc Avanzada y Liquidación"; Width=960; Height=780;
        MinimumSize=new Size(700,500); StartPosition=FormStartPosition.CenterScreen;
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        TabPage page=new TabPage(); page.Dock=DockStyle.Fill;
        helpEnabled = !File.Exists(Path.Combine(dataDir, "ocultar_ayuda.txt")); BuildCalculatorSuite2824(page);
        Controls.Add(calculatorTabs2824); page.Dispose();
        MenuStrip menu=new MenuStrip();
        menu.Items.Add("Inventario y nube",null,delegate { ShowLiquidacionMovil2940(); });
        ToolStripMenuItem connections = new ToolStripMenuItem("Conexiones");
        connections.DropDownItems.Add("Google Drive: conectar o importar de Navaja", null, delegate { ShowCloudConfiguration("Google Drive"); });
        connections.DropDownItems.Add("Dropbox: conectar o importar de Navaja", null, delegate { ShowCloudConfiguration("Dropbox"); });
        menu.Items.Add(connections);
        ToolStripMenuItem help = new ToolStripMenuItem("Mostrar mensajes de ayuda") { Checked = helpEnabled, CheckOnClick = true };
        help.CheckedChanged += delegate {
            helpEnabled = help.Checked;
            foreach(Control control in helpControls) control.Visible = helpEnabled;
            string preference = Path.Combine(dataDir, "ocultar_ayuda.txt");
            if(helpEnabled) { if(File.Exists(preference))File.Delete(preference); } else File.WriteAllText(preference, "1");
        };
        menu.Items.Add(help);
        menu.Items.Add("Buscar actualizaciones", null, delegate { CheckGitHubUpdate(); });
        ToolStripMenuItem sound=new ToolStripMenuItem("Sonidos") { Checked=true, CheckOnClick=true };
        sound.CheckedChanged+=delegate { soundsEnabled=sound.Checked; }; menu.Items.Add(sound);
        Controls.Add(menu); MainMenuStrip=menu;
        Shown+=delegate { AnnounceToScreenReader("Auto Calc Avanzada y Liquidación. Lista."); };
        FormClosed+=delegate { if(mobileSyncPeriodicTimer2940!=null)mobileSyncPeriodicTimer2940.Dispose(); if(mobileSyncDebounceTimer2940!=null)mobileSyncDebounceTimer2940.Dispose(); };
    }
    private void SetStatus(string text) { AnnounceToScreenReader(text); }
    private readonly object speechLock = new object();
    private void AnnounceToScreenReader(string text) {
        lock(speechLock) { try { string path=Path.Combine(dataDir,"nvda_speech.json");
            File.WriteAllText(path+".tmp",new JavaScriptSerializer().Serialize(new Dictionary<string,object>{{"id",Guid.NewGuid().ToString("N")},{"text",text}}),new UTF8Encoding(false));
            if(File.Exists(path))File.Replace(path+".tmp",path,null);else File.Move(path+".tmp",path);
        }catch{} }
    }
    private void AnnounceResults(Control control,string text) { AnnounceToScreenReader(text); }
    private bool IsHelpLabel1602(string text,TextBoxBase box){return text == "Ayuda:" || text == "Uso:" || text == "Uso rápido:" || text == "Información:";}
    private void MarkHelpRow1602(Label label,TextBoxBase box){ helpControls.Add(label); helpControls.Add(box); label.Visible = helpEnabled; box.Visible = helpEnabled; }
    private void AppendBoundedPerformanceLine2917(string path,string line){File.AppendAllText(path,line+Environment.NewLine);}
    private void ExportSellerLiquidationStandalone2850(){
        using(SaveFileDialog f=new SaveFileDialog()){f.Filter="Calculadora independiente|*.zip";f.FileName="AutoCalc-independiente.zip";
            if(f.ShowDialog(this)!=DialogResult.OK)return;
            try {
                string temporary=f.FileName+".tmp";
                using(var archive=System.IO.Compression.ZipFile.Open(temporary,System.IO.Compression.ZipArchiveMode.Create)){
                    string baseDir=AppDomain.CurrentDomain.BaseDirectory;
                    System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(archive,Path.Combine(baseDir,"AutoCalc.exe"),"AutoCalc.exe");
                    foreach(string file in Directory.GetFiles(Path.Combine(baseDir,"sounds"),"*.wav"))System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(archive,file,"sounds/"+Path.GetFileName(file));
                }
                if(File.Exists(f.FileName))File.Replace(temporary,f.FileName,null);else File.Move(temporary,f.FileName);
                AnnounceToScreenReader("Calculadora independiente exportada sin datos personales ni credenciales.");
            }catch(Exception ex){MessageBox.Show(this,ex.Message,"Exportar calculadora");}
        }
    }
    private SoundPlayer soundPlayer;
    private void PlayNamedSound(string name) {
        if(!soundsEnabled)return;
        string file=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"sounds",name+".wav");
        if(!File.Exists(file))return;
        try { if(soundPlayer!=null)soundPlayer.Dispose(); soundPlayer=new SoundPlayer(file);soundPlayer.Play(); }catch{}
    }
    private WebClient NewWebClient(){return new NavajaWebClient();}
    private string NetworkHost153(string url){return new Uri(url).Host;}
    private string DescribeNetworkException153(Exception ex){return ex.Message;}
    private string MailValue292(Dictionary<string,object> row,string key){return GetString(row,key);}
    private void PublishLiquidacionMovilUpdate2940(){MessageBox.Show(this,"Las actualizaciones del complemento se distribuyen mediante GitHub.");}
    private void ShowCloudHub2878(){ShowCloudConfiguration("Dropbox");}
    private void ShowCloudBrowser2878(string provider){ShowCloudConfiguration(provider);}
    private void ShowGoogleDriveModule2851(){ShowCloudConfiguration("Google Drive");}
}
public class NavajaWebClient : WebClient {
    public int TimeoutMilliseconds=15000;
    protected override WebRequest GetWebRequest(Uri address){WebRequest r=base.GetWebRequest(address);r.Timeout=TimeoutMilliseconds;return r;}
}
static class Program {
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern IntPtr FindWindow(string className,string title);
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr handle);
    [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr handle,int command);
    [STAThread] static void Main(string[] args){
        bool created;using(var gate=new System.Threading.Mutex(true,"Local\\AutoCalcAvanzadaLiquidacion",out created)){
            if(!created){IntPtr window=FindWindow(null,"Auto Calc Avanzada y Liquidación");if(window!=IntPtr.Zero){ShowWindow(window,9);SetForegroundWindow(window);}return;}
            Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);string directory=args.Length==2&&args[0]=="--data-dir"?args[1]:null;Application.Run(new MainForm(directory));
        }
    }
}
}
