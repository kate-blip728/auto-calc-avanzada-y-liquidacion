using System;
using System.IO;
using System.Windows.Forms;
namespace TodoAMano {
static class SoundPackTests {
 public static void Run() {
  string dir=Path.Combine(Path.GetTempPath(),"ac-pack-"+Guid.NewGuid().ToString("N"));
  try {
   using(var form=new MainForm(dir)) {
    ToolStripMenuItem packs=null;
    foreach(ToolStripItem item in form.MainMenuStrip.Items)if(item.Text=="Pack de sonidos")packs=(ToolStripMenuItem)item;
    if(packs==null)throw new Exception("Falta selector de packs");
    ((ToolStripMenuItem)packs.DropDownItems[1]).PerformClick();
    if(File.ReadAllText(Path.Combine(dir,"pack_sonidos.txt"))!="curioso")throw new Exception("No guarda pack");
   }
   using(var form=new MainForm(dir)) {
    foreach(ToolStripItem item in form.MainMenuStrip.Items)if(item.Text=="Pack de sonidos") {
     var packs=(ToolStripMenuItem)item;
     if(!((ToolStripMenuItem)packs.DropDownItems[1]).Checked)throw new Exception("No restaura pack");
     ((ToolStripMenuItem)packs.DropDownItems[0]).PerformClick();
     if(File.ReadAllText(Path.Combine(dir,"pack_sonidos.txt"))!="clasico")throw new Exception("No restaura clásico");
    }
   }
   Console.WriteLine("OK: selector de sonidos, guardado, reapertura y vuelta al clásico");
  }finally { if(Directory.Exists(dir))Directory.Delete(dir,true); }
 }
}
}
