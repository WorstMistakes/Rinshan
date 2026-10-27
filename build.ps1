# Compila dist\Rinshan.exe con el compilador de .NET Framework que ya trae Windows.
# La primera vez descarga la librería de WebView2 desde NuGet a packages\.
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$webView2Version = '1.0.4258.31'
$wv   = Join-Path $root 'packages\webview2'
$out  = Join-Path $root 'dist'
$csc  = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'

if (-not (Test-Path "$wv\lib\net462\Microsoft.Web.WebView2.Core.dll")) {
    Write-Host "Descargando Microsoft.Web.WebView2 $webView2Version..."
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    New-Item -ItemType Directory -Force $wv | Out-Null
    $zip = Join-Path $root 'packages\webview2.zip'
    Invoke-WebRequest "https://api.nuget.org/v3-flatcontainer/microsoft.web.webview2/$webView2Version/microsoft.web.webview2.$webView2Version.nupkg" -OutFile $zip -UseBasicParsing
    Expand-Archive $zip $wv -Force
    Remove-Item $zip
}

New-Item -ItemType Directory -Force $out | Out-Null
# DLLs are locked while the app runs; they only change when the WebView2 package does
foreach ($dll in "$wv\lib\net462\Microsoft.Web.WebView2.Core.dll",
                 "$wv\lib\net462\Microsoft.Web.WebView2.WinForms.dll",
                 "$wv\runtimes\win-x64\native\WebView2Loader.dll") {
    $target = Join-Path $out (Split-Path $dll -Leaf)
    if (-not (Test-Path $target) -or (Get-Item $target).Length -ne (Get-Item $dll).Length) { Copy-Item $dll $out -Force }
}
# A running exe can't be overwritten but can be renamed, so the new build is used on the next launch
$exe = Join-Path $out 'Rinshan.exe'
$old = Join-Path $out 'Rinshan.old.exe'
try { Remove-Item $old -Force -ErrorAction Stop } catch { }
if (Test-Path $exe) { Rename-Item $exe 'Rinshan.old.exe' -ErrorAction SilentlyContinue }

& $csc /nologo /target:winexe /platform:x64 /optimize+ `
    "/out:$exe" `
    "/win32icon:$root\assets\icon.ico" `
    "/resource:$root\src\panel.html,Rinshan.panel.html" `
    "/resource:$root\src\translate.js,Rinshan.translate.js" `
    "/r:$out\Microsoft.Web.WebView2.Core.dll" `
    "/r:$out\Microsoft.Web.WebView2.WinForms.dll" `
    /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll `
    "$root\src\*.cs"
if ($LASTEXITCODE -ne 0) { throw "La compilación falló" }
Write-Host "OK -> $exe"
