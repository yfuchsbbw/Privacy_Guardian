param(
    [string] $InnoCompilerPath = ""
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectFile = Join-Path $projectRoot "PrivacyGuardianApp.csproj"
$installerScript = Join-Path $projectRoot "Installer\PrivacyGuardian.iss"
$publishDir = Join-Path $projectRoot "bin\Release\net8.0-windows\win-x64\publish"
$innoCompilerCandidates = @(
    $InnoCompilerPath,
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
    "${env:LocalAppData}\Programs\Inno Setup 6\ISCC.exe"
) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

$innoCommand = Get-Command ISCC.exe -ErrorAction SilentlyContinue
$innoCompiler = if ($innoCommand) { $innoCommand.Source } else { "" }
if ([string]::IsNullOrWhiteSpace($innoCompiler)) {
    $innoCompiler = $innoCompilerCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}

if ([string]::IsNullOrWhiteSpace($innoCompiler)) {
    $innoCompiler = Get-ChildItem -Path "${env:ProgramFiles}", "${env:ProgramFiles(x86)}", "${env:LocalAppData}\Programs" -Filter ISCC.exe -Recurse -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty FullName -First 1
}
$dotnet = "dotnet"
if (-not (Get-Command $dotnet -ErrorAction SilentlyContinue)) {
    $dotnet = "${env:ProgramFiles}\dotnet\dotnet.exe"
}
if (-not (Test-Path -LiteralPath $dotnet) -and -not (Get-Command $dotnet -ErrorAction SilentlyContinue)) {
    throw ".NET SDK was not found. Install the .NET 8 SDK first."
}

Write-Host "Publishing Privacy Guardian..." -ForegroundColor Cyan
& $dotnet publish $projectFile /p:PublishProfile=FolderProfile

if (-not (Test-Path -LiteralPath (Join-Path $publishDir "PrivacyGuardian.exe"))) {
    throw "Publish failed: PrivacyGuardian.exe was not found in $publishDir"
}

if (Test-Path -LiteralPath $innoCompiler) {
    Write-Host "Building installer..." -ForegroundColor Cyan
    & $innoCompiler $installerScript
    Write-Host "Installer created in: $projectRoot\Publish" -ForegroundColor Green
}
else {
    Write-Host "Inno Setup 6 was not found." -ForegroundColor Yellow
    Write-Host "Install it from https://jrsoftware.org/isdl.php and run this script again." -ForegroundColor Yellow
    Write-Host "Published app folder: $publishDir" -ForegroundColor Green
}
