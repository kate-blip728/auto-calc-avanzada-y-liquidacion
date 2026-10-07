$ErrorActionPreference = 'Stop'
$base = $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$sourceRoot = Join-Path $base 'src'
if (-not (Test-Path -LiteralPath $sourceRoot)) { $sourceRoot = $base }
$sourceFiles = @(Get-ChildItem -LiteralPath $sourceRoot -Filter '*.cs' | Where-Object { $_.Name -ne 'Tests.cs' } | ForEach-Object { $_.FullName })
& $compiler /nologo /target:winexe /platform:anycpu /out:"$base/AutoCalc.exe" /r:System.dll /r:System.Core.dll /r:System.Data.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll /r:System.Xml.dll /r:System.Security.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll $sourceFiles
if ($LASTEXITCODE -ne 0) { throw 'No se pudo compilar Auto Calc.' }

$sounds = Join-Path $base 'sounds'
New-Item -ItemType Directory -Path $sounds -Force | Out-Null
Get-ChildItem -LiteralPath $base -Filter '*.wav' | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $sounds -Force }
