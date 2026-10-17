using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
namespace TodoAMano {
public partial class MainForm {
    private string soundPack = "clasico";
    private static readonly string[] SoundPackIds = { "clasico", "curioso", "telefono", "policial", "ordenador", "personalizado" };
    private static readonly string[] SoundPackLabels = { "Clásico", "Curioso: burbujas y arcade", "Teléfono: marcación y llamadas", "Policial: radio y sirenas breves", "Ordenador: teclas y electrónica", "Personalizado" };
    private string CustomSoundsDirectory { get { return Path.Combine(dataDir,"sonidos_personalizados"); } }
    private static bool IsSoundPack(string value) { return Array.IndexOf(SoundPackIds,value)>=0; }
    private static bool IsCustomSoundName(string name) {
        if(name==null || name!=Path.GetFileName(name))return false;
        string stem=Path.GetFileNameWithoutExtension(name);
        if(!name.EndsWith(".wav",StringComparison.OrdinalIgnoreCase))return false;
        string[] names={ "calc_numero","calc_decimal","calc_operacion","calc_borrar","calc_igual","calc_resultado","calc_conversion","calc_copiar","calc_eliminar_resultado","calc_historial_limpiar","calc_historial","calc_liquidacion_anadir","calc_liquidacion_borrar","calc_liquidacion_total","calc_memoria" };
        return Array.IndexOf(names,stem)>=0 || (stem.Length==7 && stem.StartsWith("tecla_",StringComparison.Ordinal) && stem[6]>='0' && stem[6]<='9');
    }
    private string ResolveSoundFile(string name) {
        string root=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"sounds");
        string fallback=Path.Combine(root,name+".wav");
        if(name.StartsWith("tecla_",StringComparison.Ordinal))fallback=Path.Combine(root,"calc_numero.wav");
        if(soundPack=="personalizado" && IsCustomSoundName(name+".wav")) {
            string custom=Path.Combine(CustomSoundsDirectory,name+".wav");
            if(File.Exists(custom))return custom;
            if(name.StartsWith("tecla_",StringComparison.Ordinal)) {
                custom=Path.Combine(CustomSoundsDirectory,"calc_numero.wav");
                if(File.Exists(custom))return custom;
            }
        } else if(soundPack!="clasico" && IsSoundPack(soundPack) && (name.StartsWith("calc_",StringComparison.Ordinal) || name.StartsWith("tecla_",StringComparison.Ordinal))) {
            string alternate=Path.Combine(root,soundPack+"_"+name+".wav");
            if(File.Exists(alternate))return alternate;
            if(name.StartsWith("tecla_",StringComparison.Ordinal)) {
                alternate=Path.Combine(root,soundPack+"_calc_numero.wav");
                if(File.Exists(alternate))return alternate;
            }
        }
        return fallback;
    }
    // Accept short, uncompressed WAV effects; reject malformed chunks before copying.
    private static bool IsSupportedCustomWave(string file) {
        try {
            using(var stream=File.OpenRead(file))using(var reader=new BinaryReader(stream)) {
                if(stream.Length<44 || stream.Length>5*1024*1024)return false;
                if(Encoding.ASCII.GetString(reader.ReadBytes(4))!="RIFF")return false;
                uint riffSize=reader.ReadUInt32();
                if((long)riffSize+8!=stream.Length || Encoding.ASCII.GetString(reader.ReadBytes(4))!="WAVE")return false;
                bool format=false, data=false;uint rate=0;ushort alignment=0;long dataSize=0;
                while(stream.Position+8<=stream.Length) {
                    string chunk=Encoding.ASCII.GetString(reader.ReadBytes(4));uint size=reader.ReadUInt32();long end=stream.Position+size;
                    if(end>stream.Length)return false;
                    if(chunk=="fmt ") {
                        if(size<16)return false;
                        ushort encoding=reader.ReadUInt16(),channels=reader.ReadUInt16();uint sampleRate=reader.ReadUInt32();rate=reader.ReadUInt32();alignment=reader.ReadUInt16();ushort bits=reader.ReadUInt16();
                        if(encoding!=1 || (channels!=1 && channels!=2) || (bits!=8 && bits!=16) || sampleRate<8000 || sampleRate>96000 || alignment!=channels*(bits/8) || rate!=sampleRate*alignment)return false;
                        format=true;
                    } else if(chunk=="data") { dataSize+=size;data=true; }
                    stream.Position=end+(size%2);
                    if(stream.Position>stream.Length)return false;
                }
                return format && data && dataSize>0 && alignment>0 && dataSize%alignment==0 && rate>0 && dataSize<=rate*5L && stream.Position==stream.Length;
            }
        }catch{return false;}
    }
    private void ImportCustomSoundPack(Action<string> select) {
        using(var picker=new FolderBrowserDialog()) {
            picker.Description="Elige una carpeta con tus WAV. Se copiarán a Auto Calc conservando los nombres. Máximo 5 segundos por sonido. Los nombres ausentes usarán Clásico.";
            if(picker.ShowDialog(this)!=DialogResult.OK)return;
            string stage=Path.Combine(dataDir,"sonidos_importando_"+Guid.NewGuid().ToString("N"));
            string backup=Path.Combine(dataDir,"sonidos_anteriores_"+Guid.NewGuid().ToString("N"));
            bool installed=false;
            try {
                var files=new List<string>();
                foreach(string file in Directory.GetFiles(picker.SelectedPath))if(IsCustomSoundName(Path.GetFileName(file)))files.Add(file);
                if(files.Count==0)throw new InvalidDataException("No hay WAV con nombres reconocidos. Usa Crear plantilla para consultar los nombres.");
                Directory.CreateDirectory(stage);
                foreach(string file in files) {
                    string destination=Path.Combine(stage,Path.GetFileName(file));
                    if(new FileInfo(file).Length>5*1024*1024)throw new InvalidDataException("Archivo demasiado grande: "+Path.GetFileName(file));
                    File.Copy(file,destination,false);
                    if(!IsSupportedCustomWave(destination))throw new InvalidDataException("WAV no compatible: "+Path.GetFileName(file)+". Usa PCM de 8 o 16 bits, mono o estéreo, hasta 5 segundos.");
                }
                if(Directory.Exists(CustomSoundsDirectory))Directory.Move(CustomSoundsDirectory,backup);
                try { Directory.Move(stage,CustomSoundsDirectory); installed=true; }
                catch { if(Directory.Exists(backup))Directory.Move(backup,CustomSoundsDirectory);throw; }
                select("personalizado");
                AnnounceToScreenReader(files.Count+" sonidos importados. Los sonidos que faltan usarán Clásico.");
            }catch(Exception ex) { MessageBox.Show(this,ex.Message,"Importar sonidos"); }
            finally {
                try { if(Directory.Exists(stage))Directory.Delete(stage,true); }catch{}
                try { if(installed && Directory.Exists(backup))Directory.Delete(backup,true); }catch{}
            }
        }
    }
    private void ExportCustomSoundTemplate() {
        using(var picker=new FolderBrowserDialog()) {
            picker.Description="Elige dónde crear una carpeta nueva con la plantilla de sonidos.";
            if(picker.ShowDialog(this)!=DialogResult.OK)return;
            try {
                string target=Path.Combine(picker.SelectedPath,"AutoCalc-plantilla-"+Guid.NewGuid().ToString("N").Substring(0,8));
                Directory.CreateDirectory(target);
                foreach(string file in Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"sounds"),"calc_*.wav"))if(IsCustomSoundName(Path.GetFileName(file)))File.Copy(file,Path.Combine(target,Path.GetFileName(file)));
                File.WriteAllText(Path.Combine(target,"LEEME.txt"),CustomSoundsHelp,new UTF8Encoding(false));
                MessageBox.Show(this,"Plantilla creada en: "+target+". Sustituye los WAV conservando sus nombres y usa Importar pack personalizado.","Plantilla de sonidos");
            }catch(Exception ex) { MessageBox.Show(this,ex.Message,"Plantilla de sonidos"); }
        }
    }
    private const string CustomSoundsHelp = "Sustituye los WAV conservando sus nombres. No basta con renombrar un MP3: exporta WAV PCM de 8 o 16 bits, mono o estéreo, 8000 a 96000 Hz, hasta 5 segundos y 5 MB por archivo.\r\n\r\ncalc_numero.wav: números\r\ncalc_decimal.wav: coma o punto\r\ncalc_operacion.wav: operadores\r\ncalc_borrar.wav: borrar\r\ncalc_igual.wav: calcular\r\ncalc_resultado.wav: resultado\r\ncalc_conversion.wav: conversión\r\ncalc_copiar.wav: copiar\r\ncalc_eliminar_resultado.wav: eliminar resultado\r\ncalc_historial.wav: historial\r\ncalc_historial_limpiar.wav: limpiar historial\r\ncalc_liquidacion_anadir.wav: añadir artículo\r\ncalc_liquidacion_borrar.wav: borrar artículo\r\ncalc_liquidacion_total.wav: total\r\ncalc_memoria.wav: memoria\r\n\r\nOpcional: tecla_0.wav hasta tecla_9.wav para un sonido por cifra. Si falta una tecla se usa calc_numero.wav; si falta cualquier otro efecto se usa Clásico.\r\n\r\nImportar copia los archivos a los datos de usuario y sustituye el pack personalizado anterior. Las actualizaciones conservan esta carpeta. Los archivos con otros nombres se ignoran. No incluyas información privada en tus audios si vas a compartirlos.";
    private void InstallSoundPackMenu(MenuStrip menu) {
        string preference=Path.Combine(dataDir,"pack_sonidos.txt");
        try { if(File.Exists(preference)) { string saved=File.ReadAllText(preference).Trim();if(IsSoundPack(saved))soundPack=saved; } }catch{}
        ToolStripMenuItem packs=new ToolStripMenuItem("Pack de sonidos");
        var entries=new Dictionary<string,ToolStripMenuItem>();
        Action<string> select=delegate(string value) {
            try { File.WriteAllText(preference,value); }
            catch(Exception ex) { MessageBox.Show(this,"No se pudo guardar el pack: "+ex.Message,"Sonidos");return; }
            soundPack=value;
            foreach(var item in entries)item.Value.Checked=item.Key==value;
            PlayCalculatorSound1554("calc_resultado");
            AnnounceToScreenReader("Pack "+SoundPackLabels[Array.IndexOf(SoundPackIds,value)]+" seleccionado.");
        };
        for(int i=0;i<SoundPackIds.Length;i++) {
            string id=SoundPackIds[i];var entry=new ToolStripMenuItem(SoundPackLabels[i]);entry.Checked=soundPack==id;
            entry.Click+=delegate { select(id); };entries.Add(id,entry);packs.DropDownItems.Add(entry);
        }
        packs.DropDownItems.Add(new ToolStripSeparator());
        packs.DropDownItems.Add("Escuchar muestra del pack",null,delegate { PlayCalculatorSound1554("calc_resultado"); });
        packs.DropDownItems.Add("Importar pack personalizado...",null,delegate { ImportCustomSoundPack(select); });
        packs.DropDownItems.Add("Crear plantilla con los nombres...",null,delegate { ExportCustomSoundTemplate(); });
        packs.DropDownItems.Add("Ayuda para sonidos propios",null,delegate { MessageBox.Show(this,CustomSoundsHelp,"Sonidos propios"); });
        menu.Items.Add(packs);
    }
}
}
