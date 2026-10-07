$ErrorActionPreference = 'Stop'
$base = $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$sourceRoot = Join-Path $base 'src'
if (-not (Test-Path -LiteralPath $sourceRoot)) { $sourceRoot = $base }
$sourceFiles = @(Get-ChildItem -LiteralPath $sourceRoot -Filter '*.cs' | Where-Object { $_.Name -ne 'Tests.cs' } | ForEach-Object { $_.FullName })
$sourceFiles += (Join-Path $base 'Tests.cs')
& $compiler /nologo /target:exe /main:TodoAMano.TestProgram /out:"$base/Tests.exe" /r:System.dll /r:System.Core.dll /r:System.Data.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll /r:System.Xml.dll /r:System.Security.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll $sourceFiles
if ($LASTEXITCODE -ne 0) { throw 'No se pudieron compilar las pruebas.' }
& "$base/Tests.exe"
if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas.' }
