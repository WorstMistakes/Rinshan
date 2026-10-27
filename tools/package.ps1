# Empaqueta dist\ en release\Rinshan-vX.Y.Z.zip (una carpeta Rinshan\ con el exe y sus DLLs).
# Ejecuta build.ps1 antes. El workflow de GitHub Actions usa este mismo script.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot
$dist = Join-Path $root 'dist'
$v = [Version](Get-Item "$dist\Rinshan.exe").VersionInfo.FileVersion
$version = "$($v.Major).$($v.Minor).$($v.Build)"

$stage = Join-Path $root 'release\stage'
$folder = Join-Path $stage 'Rinshan'
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Force $folder | Out-Null
foreach ($file in 'Rinshan.exe', 'Microsoft.Web.WebView2.Core.dll', 'Microsoft.Web.WebView2.WinForms.dll', 'WebView2Loader.dll') {
    Copy-Item (Join-Path $dist $file) $folder
}

$zip = Join-Path $root "release\Rinshan-v$version.zip"
if (Test-Path $zip) { Remove-Item $zip }
Compress-Archive -Path $folder -DestinationPath $zip
Remove-Item $stage -Recurse -Force
Write-Host "OK -> $zip"
# Para el workflow: la versión y la ruta del zip
if ($env:GITHUB_OUTPUT) {
    "version=$version" | Out-File $env:GITHUB_OUTPUT -Append -Encoding utf8
    "zip=$zip" | Out-File $env:GITHUB_OUTPUT -Append -Encoding utf8
}
