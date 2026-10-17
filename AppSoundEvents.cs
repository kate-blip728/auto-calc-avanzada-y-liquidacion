using System;
using System.Diagnostics;
using System.Windows.Forms;
namespace TodoAMano { public partial class MainForm {
    private readonly System.Runtime.CompilerServices.ConditionalWeakTable<Control,object> soundControls=new System.Runtime.CompilerServices.ConditionalWeakTable<Control,object>();
    private readonly System.Runtime.CompilerServices.ConditionalWeakTable<Control,object> typingSoundControls=new System.Runtime.CompilerServices.ConditionalWeakTable<Control,object>();
    private Timer navigationSoundTimer;
    private bool soundEventsReady,appSoundsFinished;
    private void QueueNavigationSound() {
        if(!soundEventsReady || !soundsEnabled || !calculatorSoundsEnabled1554 || Stopwatch.GetTimestamp()<soundPriorityUntil)return;
        navigationSoundTimer.Stop();navigationSoundTimer.Start();
    }
    private void PlayTypingSound(char key) {
        if(char.IsDigit(key))PlayCalculatorSound1554((soundPack=="telefono" || soundPack=="personalizado") && key>='0' && key<='9'?"tecla_"+key:"calc_numero");
        else if(key==',' || key=='.')PlayCalculatorSound1554("calc_decimal");
        else if("+-*/%();".IndexOf(key)>=0)PlayCalculatorSound1554("calc_operacion");
        else if(!char.IsControl(key))PlayCalculatorSound1554("calc_escribir");
    }
    private void AttachNavigationSounds(Control root) {
        object marker;if(soundControls.TryGetValue(root,out marker))return;soundControls.Add(root,new object());
        TextBoxBase text=root as TextBoxBase;
        if(text!=null && !text.ReadOnly && !typingSoundControls.TryGetValue(text,out marker)) {
            typingSoundControls.Add(text,new object());
            text.KeyPress+=delegate(object sender,KeyPressEventArgs e) {PlayTypingSound(e.KeyChar);};
            text.KeyDown+=delegate(object sender,KeyEventArgs e) {if(e.KeyCode==Keys.Back || e.KeyCode==Keys.Delete)PlayCalculatorSound1554("calc_borrar");};
        }
        if(root is ButtonBase || root is TextBoxBase || root is ComboBox || root is ListBox || root is NumericUpDown || root is TabControl)root.Enter+=delegate { QueueNavigationSound(); };
        ComboBox combo=root as ComboBox;if(combo!=null)combo.SelectedIndexChanged+=delegate {if(combo.Focused)QueueNavigationSound();};
        ListBox list=root as ListBox;if(list!=null)list.SelectedIndexChanged+=delegate {if(list.Focused)QueueNavigationSound();};
        root.ControlAdded+=delegate(object sender,ControlEventArgs e) {AttachNavigationSounds(e.Control);};
        foreach(Control child in root.Controls)AttachNavigationSounds(child);
    }
    private void InstallAppSoundEvents() {
        navigationSoundTimer=new Timer {Interval=65};
        navigationSoundTimer.Tick+=delegate { navigationSoundTimer.Stop();if(soundEventsReady && Stopwatch.GetTimestamp()>=soundPriorityUntil)PlayCalculatorSound1554("calc_navegar"); };
        AttachNavigationSounds(this);
        calculatorTabs2824.SelectedIndexChanged+=delegate { if(soundEventsReady)PlayCalculatorSound1554("calc_pantalla"); };
        Shown+=delegate {soundEventsReady=true;PlayCalculatorSound1554("calc_abrir");};
    }
    internal void NotifySoundScreen(Form screen,bool opened) {
        if(!soundEventsReady)return;
        if(opened)AttachNavigationSounds(screen);
        PlayCalculatorSound1554(opened?"calc_pantalla":"calc_salir_pantalla");
    }
    private void FinishAppSounds() {
        if(appSoundsFinished)return;appSoundsFinished=true;
        bool wasReady=soundEventsReady;soundEventsReady=false;
        if(navigationSoundTimer!=null) {navigationSoundTimer.Stop();navigationSoundTimer.Dispose();}
        if(pendingCalculatorSoundTimer1554!=null) {pendingCalculatorSoundTimer1554.Stop();pendingCalculatorSoundTimer1554.Dispose();}
        string exit=wasReady && soundsEnabled && calculatorSoundsEnabled1554?CachedSoundFile("calc_salir"):null;
        if(soundEngine==null && exit!=null)soundEngine=new SoundEngine();
        if(soundEngine!=null)soundEngine.Finish(exit);
    }
} }
