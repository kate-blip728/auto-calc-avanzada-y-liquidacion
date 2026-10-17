using System;
using System.IO;
using System.Windows.Forms;
namespace TodoAMano {
public partial class MainForm {
    private string soundPack = "clasico";
    private void InstallSoundPackMenu(MenuStrip menu) {
        string preference=Path.Combine(dataDir,"pack_sonidos.txt");
        try { if(File.Exists(preference)) { string saved=File.ReadAllText(preference).Trim(); if(saved=="curioso" || saved=="telefono")soundPack=saved; } }catch{}
        ToolStripMenuItem packs=new ToolStripMenuItem("Pack de sonidos");
        ToolStripMenuItem classic=new ToolStripMenuItem("Clásico");
        ToolStripMenuItem curious=new ToolStripMenuItem("Curioso: burbujas y arcade");
        ToolStripMenuItem phone=new ToolStripMenuItem("Teléfono: marcación y llamadas");
        phone.Checked=soundPack=="telefono";
        classic.Checked=soundPack=="clasico"; curious.Checked=soundPack=="curioso";
        Action<string> select=delegate(string value) {
            try { File.WriteAllText(preference,value); }
            catch(Exception ex) { MessageBox.Show(this,"No se pudo guardar el pack: "+ex.Message,"Sonidos"); return; }
            soundPack=value; phone.Checked=value=="telefono"; classic.Checked=value=="clasico"; curious.Checked=value=="curioso";
            PlayCalculatorSound1554("calc_resultado");
            AnnounceToScreenReader("Pack de sonidos "+(value=="curioso"?"Curioso":value=="telefono"?"Teléfono":"Clásico")+" seleccionado.");
        };
        classic.Click+=delegate { select("clasico"); };
        curious.Click+=delegate { select("curioso"); };
        phone.Click+=delegate { select("telefono"); };
        packs.DropDownItems.Add(classic); packs.DropDownItems.Add(curious); packs.DropDownItems.Add(phone);
        packs.DropDownItems.Add("Escuchar muestra del pack",null,delegate { PlayCalculatorSound1554("calc_resultado"); });
        menu.Items.Add(packs);
    }
}
}
