using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TodoAMano {
public partial class MainForm {
    internal static ProcessStartInfo CreateUpdateInstallerStartInfo(string package, int parentId, long parentStartTicks) {
        // All paths travel as base64 data, never as PowerShell source or arguments.
        string encodedPath = Convert.ToBase64String(Encoding.UTF8.GetBytes(Path.GetFullPath(package)));
        string script = @"
$ErrorActionPreference = 'Stop'
$package = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('" + encodedPath + @"'))
try {
    $parent = $null
    try { $parent = [Diagnostics.Process]::GetProcessById(" + parentId + @") }
    catch [ArgumentException] { }
    if ($null -ne $parent) {
        try {
            if ($parent.StartTime.ToUniversalTime().Ticks -eq " + parentStartTicks + @") {
                if (-not $parent.WaitForExit(60000)) {
                    throw 'Auto Calc sigue abierto. Cierra la calculadora antes de instalar.'
                }
            }
        } finally { $parent.Dispose() }
    }
    Start-Process -FilePath $package -WindowStyle Hidden -ErrorAction Stop | Out-Null
} catch {
    $_.Exception.Message | Set-Content -LiteralPath ($package + '.install-error.txt') -Encoding UTF8
    exit 1
}
";
        string powershell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe");
        return new ProcessStartInfo(powershell, "-NoLogo -NoProfile -NonInteractive -WindowStyle Hidden -EncodedCommand " + Convert.ToBase64String(Encoding.Unicode.GetBytes(script))) {
            UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden
        };
    }
}
}
