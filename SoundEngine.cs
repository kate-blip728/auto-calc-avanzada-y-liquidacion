using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
namespace TodoAMano {
// One loader and one pending effect: repeated input never builds an audio backlog.
internal sealed class SoundEngine {
    private readonly object gate=new object();
    private readonly AutoResetEvent ready=new AutoResetEvent(false);
    private readonly Thread worker;
    private string pending,closingSound;
    private bool closing,reset,muted;
    internal SoundEngine() { worker=new Thread(Run);worker.Name="Auto Calc audio";worker.IsBackground=false;worker.Start(); }
    internal void Play(string file) { lock(gate) { if(closing || muted)return;pending=file;ready.Set(); } }
    internal void Reset() { lock(gate) { if(closing)return;pending=null;reset=true;ready.Set(); } }
    internal void SetMuted(bool value) { lock(gate) { if(closing)return;muted=value;if(value) {pending=null;reset=true;}ready.Set(); } }
    internal void Finish(string file) { lock(gate) { if(closing)return;closing=true;pending=null;closingSound=muted?null:file;ready.Set(); } }
    private void Run() {
        var cache=new Dictionary<string,SoundPlayer>(StringComparer.OrdinalIgnoreCase);
        var order=new Queue<string>();var sizes=new Dictionary<string,long>(StringComparer.OrdinalIgnoreCase);long bytes=0;SoundPlayer active=null;
        try {
            while(true) {
                ready.WaitOne();string file;bool finish,clear;
                lock(gate) { finish=closing;clear=reset;reset=false;file=finish?closingSound:pending;pending=null; }
                if(clear) { if(active!=null)try{active.Stop();}catch{} foreach(var sound in cache.Values)sound.Dispose();cache.Clear();order.Clear();sizes.Clear();bytes=0;active=null; }
                if(file!=null)try {
                    SoundPlayer player;
                    if(!cache.TryGetValue(file,out player)) {
                        long size=new FileInfo(file).Length;
                        if(size>5*1024*1024) { if(finish)break;continue; }
                        while(cache.Count>0 && (cache.Count>=48 || bytes+size>10*1024*1024)) {
                            string oldest=order.Dequeue();cache[oldest].Dispose();cache.Remove(oldest);bytes-=sizes[oldest];sizes.Remove(oldest);
                        }
                        player=new SoundPlayer(file);
                        try { player.Load(); }catch { player.Dispose();throw; }
                        cache.Add(file,player);order.Enqueue(file);sizes[file]=size;bytes+=size;
                    }
                    if(finish)player.PlaySync();
                    else {
                        lock(gate) {
                            // Do not play a stale load after a newer request, mute or pack change.
                            if(!closing && !muted && !reset && pending==null) { active=player;player.Play(); }
                        }
                    }
                }catch { }
                if(finish)break;
            }
        }finally { foreach(var sound in cache.Values)sound.Dispose();ready.Dispose(); }
    }
}
}
