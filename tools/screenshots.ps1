# Genera docs\*.png: el menú F2 con partidas de ejemplo, renderizado con Edge sin ventana.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$docs = Join-Path $root 'docs'
New-Item -ItemType Directory -Force $docs | Out-Null
$edge = @("${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe", "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe") |
    Where-Object { Test-Path $_ } | Select-Object -First 1

$sample = @'
render({muted:false, fullscreen:false, zoom:1, minimizeToTray:true, confirmClose:true, keepAwake:true, panelOnTop:false,
  discord:true, discordAppId:'', discordBuiltIn:true, games:[
  {id:'2026102621gm-00a9-0000-1f2e3d4c', when:'26/10 21:00', sanma:false, players:'4 jugadores'},
  {id:'2026102620gm-00b9-0000-5a6b7c8d', when:'26/10 20:00', sanma:true,  players:'3 jugadores'},
  {id:'2026102519gm-00a9-0000-9e8d7c6b', when:'25/10 19:00', sanma:false, players:'4 jugadores'},
  {id:'2026102518gm-00a9-0000-1a2b3c4d', when:'25/10 18:00', sanma:false, players:'4 jugadores'},
  {id:'2026102422gm-00a9-0000-4d3c2b1a', when:'24/10 22:00', sanma:false, players:'4 jugadores'},
  {id:'2026102421gm-00a9-0000-0f1e2d3c', when:'24/10 21:00', sanma:false, players:'4 jugadores'}]});
'@
$html = Get-Content (Join-Path $root 'src\panel.html') -Raw -Encoding UTF8

foreach ($tab in 'games', 'display', 'options') {
    # Headless captures the first frame, so turn off the fade-in and switch transitions
    $page = $html.Replace("selectTab('games');", "$sample selectTab('$tab');").
        Replace('</style>', '* { animation: none !important; transition: none !important; }</style>')
    $tmp = Join-Path $env:TEMP "tenhou-panel-$tab.html"
    [IO.File]::WriteAllText($tmp, $page, (New-Object Text.UTF8Encoding $false))
    $png = Join-Path $docs "menu-$tab.png"
    & $edge --headless=new --disable-gpu --hide-scrollbars --force-device-scale-factor=1.5 `
        --window-size=940,600 "--screenshot=$png" "file:///$($tmp -replace '\\','/')" | Out-Null
    Start-Sleep -Milliseconds 500
    Remove-Item $tmp
    Write-Host "OK -> $png"
}
