# Genera assets\icon.ico (16-256 px) y assets\discord-logo.png: un 嶺 (de rinshan) dorado sobre morado.
# Cada tamaño se dibuja por separado para que el ícono chico siga siendo nítido.
Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
$assets = Join-Path $root 'assets'
New-Item -ItemType Directory -Force $assets | Out-Null

function New-Logo([int]$size) {
    $bmp = New-Object Drawing.Bitmap $size, $size
    $g = [Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $g.TextRenderingHint = 'AntiAliasGridFit'
    $g.Clear([Drawing.Color]::Transparent)

    $r = [Math]::Max(4, [int]($size * 0.2))
    $rect = New-Object Drawing.Rectangle 0, 0, $size, $size
    $path = New-Object Drawing.Drawing2D.GraphicsPath
    $path.AddArc(0, 0, $r, $r, 180, 90)
    $path.AddArc($size - $r - 1, 0, $r, $r, 270, 90)
    $path.AddArc($size - $r - 1, $size - $r - 1, $r, $r, 0, 90)
    $path.AddArc(0, $size - $r - 1, $r, $r, 90, 90)
    $path.CloseFigure()

    $bg = New-Object Drawing.Drawing2D.LinearGradientBrush $rect, ([Drawing.Color]::FromArgb(0x3d, 0x2b, 0x5c)), ([Drawing.Color]::FromArgb(0x1a, 0x13, 0x26)), 45
    $g.FillPath($bg, $path)
    if ($size -ge 48) {
        $glow = New-Object Drawing.Drawing2D.PathGradientBrush $path
        $glow.CenterColor = [Drawing.Color]::FromArgb(70, 0xe6, 0xc3, 0x6a)
        $glow.SurroundColors = @([Drawing.Color]::FromArgb(0, 0xe6, 0xc3, 0x6a))
        $g.FillPath($glow, $path)
        $pen = New-Object Drawing.Pen ([Drawing.Color]::FromArgb(150, 0xe6, 0xc3, 0x6a)), ([Math]::Max(1, $size / 72))
        $g.DrawPath($pen, $path)
    }

    # Small icons get a bigger glyph so it stays readable in the taskbar and tray
    $scale = if ($size -le 32) { 0.78 } else { 0.55 }
    $font = New-Object Drawing.Font 'Yu Gothic UI', ($size * $scale), ([Drawing.FontStyle]::Bold), ([Drawing.GraphicsUnit]::Pixel)
    $fmt = New-Object Drawing.StringFormat
    $fmt.Alignment = 'Center'; $fmt.LineAlignment = 'Center'
    $gold = New-Object Drawing.Drawing2D.LinearGradientBrush $rect, ([Drawing.Color]::FromArgb(0xf3, 0xd6, 0x8a)), ([Drawing.Color]::FromArgb(0xc9, 0x97, 0x3b)), 90
    $textRect = New-Object Drawing.RectangleF 0, ($size * 0.03), $size, $size
    $g.DrawString([string][char]0x5DBA, $font, $gold, $textRect, $fmt)
    $g.Dispose()
    return $bmp
}

function Get-PngBytes($bmp) {
    $ms = New-Object IO.MemoryStream
    $bmp.Save($ms, [Drawing.Imaging.ImageFormat]::Png)
    return , $ms.ToArray()
}

# Classic 32-bit DIB entry (bottom-up BGRA plus an empty AND mask); every reader understands these
function Get-DibBytes($bmp) {
    $size = $bmp.Width
    $data = $bmp.LockBits((New-Object Drawing.Rectangle 0, 0, $size, $size), 'ReadOnly', ([Drawing.Imaging.PixelFormat]::Format32bppArgb))
    $pixels = New-Object byte[] ($size * $size * 4)
    [Runtime.InteropServices.Marshal]::Copy($data.Scan0, $pixels, 0, $pixels.Length)
    $bmp.UnlockBits($data)

    $ms = New-Object IO.MemoryStream
    $w = New-Object IO.BinaryWriter $ms
    $w.Write([uint32]40); $w.Write([int32]$size); $w.Write([int32]($size * 2))
    $w.Write([uint16]1); $w.Write([uint16]32); $w.Write([uint32]0)
    $w.Write([uint32]($pixels.Length)); $w.Write([int32]0); $w.Write([int32]0); $w.Write([uint32]0); $w.Write([uint32]0)
    for ($y = $size - 1; $y -ge 0; $y--) { $w.Write($pixels, $y * $size * 4, $size * 4) }
    $maskRow = [int]([Math]::Ceiling($size / 32.0) * 4)
    $w.Write((New-Object byte[] ($maskRow * $size)))
    return , $ms.ToArray()
}

# ICO container: DIB entries for the small sizes, PNG for 256 px
$sizes = 16, 24, 32, 48, 64, 128, 256
$images = foreach ($s in $sizes) {
    $logo = New-Logo $s
    if ($s -ge 256) { , (Get-PngBytes $logo) } else { , (Get-DibBytes $logo) }
}
$out = New-Object IO.MemoryStream
$w = New-Object IO.BinaryWriter $out
$w.Write([uint16]0); $w.Write([uint16]1); $w.Write([uint16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $dim = if ($sizes[$i] -ge 256) { 0 } else { $sizes[$i] }
    $w.Write([byte]$dim); $w.Write([byte]$dim); $w.Write([byte]0); $w.Write([byte]0)
    $w.Write([uint16]1); $w.Write([uint16]32)
    $w.Write([uint32]$images[$i].Length); $w.Write([uint32]$offset)
    $offset += $images[$i].Length
}
foreach ($img in $images) { $w.Write($img) }
[IO.File]::WriteAllBytes((Join-Path $assets 'icon.ico'), $out.ToArray())

(New-Logo 1024).Save((Join-Path $assets 'discord-logo.png'), [Drawing.Imaging.ImageFormat]::Png)
Write-Host "OK -> $assets\icon.ico, discord-logo.png"
