<#
.SYNOPSIS
    Publishes the Come Home app and builds the installer.

.DESCRIPTION
    1. Publishes ComeHome.App as a self-contained, single-file executable.
    2. Invokes Inno Setup to produce the installer EXE.

.PARAMETER SkipPublish
    Skip the dotnet publish step (use existing artifacts\publish output).

.PARAMETER InnoSetupPath
    Path to the Inno Setup compiler (ISCC.exe). When omitted the script
    searches common install locations automatically.
#>
param(
    [switch]$SkipPublish,
    [string]$InnoSetupPath
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $repoRoot 'artifacts\publish'

# ── Resolve Inno Setup compiler ──────────────────────────────────────────
if (-not $InnoSetupPath) {
    $candidates = @(
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
    )
    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            $InnoSetupPath = $candidate
            break
        }
    }
    if (-not $InnoSetupPath) {
        $InnoSetupPath = "ISCC.exe"  # fall back to PATH
    }
}

# ── 1. Publish ────────────────────────────────────────────────────────────
if (-not $SkipPublish) {
    Write-Host "Publishing ComeHome.App..." -ForegroundColor Cyan
    dotnet publish "$repoRoot\ComeHome.App\ComeHome.App.csproj" `
        -c Release `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -o $publishDir

    if ($LASTEXITCODE -ne 0) {
        Write-Error "dotnet publish failed."
        exit 1
    }
    Write-Host "Publish complete: $publishDir" -ForegroundColor Green
}

# ── 2. Build installer ───────────────────────────────────────────────────
if (-not (Test-Path $InnoSetupPath)) {
    Write-Warning "Inno Setup compiler (ISCC.exe) was not found."
    Write-Warning "Searched:"
    Write-Warning "  - $env:LOCALAPPDATA\Programs\Inno Setup 6\"
    Write-Warning "  - ${env:ProgramFiles(x86)}\Inno Setup 6\"
    Write-Warning "  - $env:ProgramFiles\Inno Setup 6\"
    Write-Warning "  - PATH"
    Write-Warning ""
    Write-Warning "Download it from https://jrsoftware.org/isdl.php"
    Write-Warning "Or pass -InnoSetupPath <path to ISCC.exe> to specify a custom location."
    exit 1
}

$issFile = Join-Path $PSScriptRoot 'ComeHome.iss'
Write-Host "Building installer..." -ForegroundColor Cyan
& $InnoSetupPath $issFile

if ($LASTEXITCODE -ne 0) {
    Write-Error "Inno Setup compilation failed."
    exit 1
}

Write-Host "Installer created: $repoRoot\artifacts\ComeHomeSetup.exe" -ForegroundColor Green
