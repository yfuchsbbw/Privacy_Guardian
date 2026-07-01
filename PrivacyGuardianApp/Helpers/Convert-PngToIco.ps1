param(
    [Parameter(Mandatory = $true)]
    [string] $InputImage,

    [string] $OutputIcon = "Assets\app.ico"
)

Add-Type -AssemblyName System.Drawing

$sourcePath = Resolve-Path -LiteralPath $InputImage
$outputPath = Join-Path (Get-Location) $OutputIcon
$outputDirectory = Split-Path -Parent $outputPath
if (-not (Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
}

$sizes = @(256, 128, 64, 48, 32, 16)
$source = [System.Drawing.Image]::FromFile($sourcePath)
$memory = New-Object System.IO.MemoryStream
$writer = New-Object System.IO.BinaryWriter($memory)

$writer.Write([UInt16]0)
$writer.Write([UInt16]1)
$writer.Write([UInt16]$sizes.Count)

$images = @()
$offset = 6 + (16 * $sizes.Count)
foreach ($size in $sizes) {
    $bitmap = New-Object System.Drawing.Bitmap $size, $size
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.DrawImage($source, 0, 0, $size, $size)
    $graphics.Dispose()

    $pngStream = New-Object System.IO.MemoryStream
    $bitmap.Save($pngStream, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
    $images += ,@($size, $pngStream.ToArray())
}

foreach ($entry in $images) {
    $size = [int]$entry[0]
    $data = [byte[]]$entry[1]
    $writer.Write([byte]($(if ($size -eq 256) { 0 } else { $size })))
    $writer.Write([byte]($(if ($size -eq 256) { 0 } else { $size })))
    $writer.Write([byte]0)
    $writer.Write([byte]0)
    $writer.Write([UInt16]1)
    $writer.Write([UInt16]32)
    $writer.Write([UInt32]$data.Length)
    $writer.Write([UInt32]$offset)
    $offset += $data.Length
}

foreach ($entry in $images) {
    $writer.Write([byte[]]$entry[1])
}

$writer.Flush()
[System.IO.File]::WriteAllBytes($outputPath, $memory.ToArray())
$writer.Dispose()
$memory.Dispose()
$source.Dispose()

Write-Host "Icon created: $outputPath"
