#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Compila NetScaffold TUI per Windows, Linux e macOS.
.DESCRIPTION
    Esegue dotnet publish per i tre runtime target (win-x64, linux-x64, osx-x64)
    e produce eseguibili self-contained nella cartella ./publish/.
.PARAMETER Runtime
    Runtime specifico da compilare (win-x64, linux-x64, osx-x64, osx-arm64).
    Se omesso, compila tutti i target.
.PARAMETER Configuration
    Configurazione di build (Debug o Release). Default: Release.
.EXAMPLE
    ./build.ps1                          # compila tutti i target
    ./build.ps1 -Runtime linux-x64       # compila solo Linux x64
    ./build.ps1 -Configuration Debug     # compila tutti in Debug
#>
param(
    [ValidateSet("win-x64", "linux-x64", "osx-x64", "osx-arm64")]
    [string]$Runtime,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "src" "NetScaffoldTui" "NetScaffoldTui.csproj"
$outputRoot = Join-Path $PSScriptRoot "publish"

$allRuntimes = @("win-x64", "linux-x64", "osx-x64", "osx-arm64")
$targets = if ($Runtime) { @($Runtime) } else { $allRuntimes }

Write-Host "=== NetScaffold TUI Build ===" -ForegroundColor Cyan
Write-Host "Configuration : $Configuration"
Write-Host "Targets       : $($targets -join ', ')"
Write-Host ""

$failed = @()

foreach ($rid in $targets) {
    $outDir = Join-Path $outputRoot $rid
    Write-Host "[$rid] Pubblicazione in $outDir ..." -ForegroundColor Yellow

    dotnet publish $project `
        --configuration $Configuration `
        --runtime $rid `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:PublishTrimmed=false `
        --output $outDir

    if ($LASTEXITCODE -ne 0) {
        Write-Host "[$rid] ERRORE durante la compilazione." -ForegroundColor Red
        $failed += $rid
    }
    else {
        Write-Host "[$rid] OK" -ForegroundColor Green
    }
    Write-Host ""
}

Write-Host "=== Riepilogo ===" -ForegroundColor Cyan
foreach ($rid in $targets) {
    if ($failed -contains $rid) {
        Write-Host "  $rid : FALLITO" -ForegroundColor Red
    }
    else {
        $outDir = Join-Path $outputRoot $rid
        Write-Host "  $rid : $outDir" -ForegroundColor Green
    }
}

if ($failed.Count -gt 0) {
    Write-Host ""
    Write-Host "Build fallita per: $($failed -join ', ')" -ForegroundColor Red
    exit 1
}
