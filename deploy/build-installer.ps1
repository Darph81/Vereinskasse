#Requires -Version 5.1
<#
.SYNOPSIS
    Baut die Vereinskasse fuer Windows x64 und erstellt daraus
    ein Setup.exe (Inno Setup).

.DESCRIPTION
    1. dotnet publish -> self-contained, single-file win-x64 Build
    2. ISCC.exe (Inno Setup Compiler) -> deploy\Setup.iss -> deploy\Output\VereinskasseSetup.exe

.REQUIREMENTS
    - .NET 10 SDK            https://dotnet.microsoft.com/download
    - Inno Setup 6            https://jrsoftware.org/isinfo.php
      (ISCC.exe muss im PATH liegen oder im Standard-Installationsordner)

.USAGE
    Aus dem Projektstammverzeichnis oder aus deploy\ heraus in PowerShell ausfuehren:
        .\deploy\build-installer.ps1
#>

$ErrorActionPreference = "Stop"

$deployDir = $PSScriptRoot
$projectRoot = Split-Path -Parent $deployDir
$publishDir = Join-Path $projectRoot "bin\Release\net10.0\win-x64\publish"

Push-Location $projectRoot
try {
    Write-Host "==> dotnet publish (win-x64, self-contained, single file)" -ForegroundColor Cyan
    dotnet publish "Vereinskasse.csproj" `
        -c Release `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -o $publishDir

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish ist fehlgeschlagen (Exit Code $LASTEXITCODE)."
    }

    if (-not (Test-Path (Join-Path $publishDir "Vereinskasse.exe"))) {
        throw "Publish-Output nicht gefunden unter '$publishDir'."
    }

    Write-Host "==> Suche Inno Setup Compiler (ISCC.exe)" -ForegroundColor Cyan
    $isccCommand = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
    if ($isccCommand) {
        $iscc = $isccCommand.Source
    }
    else {
        $candidates = @(
            "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
            "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
        )
        $iscc = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
    }

    if (-not $iscc) {
        throw "ISCC.exe (Inno Setup Compiler) wurde nicht gefunden. Bitte Inno Setup 6 installieren: https://jrsoftware.org/isinfo.php"
    }

    Write-Host "==> Inno Setup Compiler: $iscc" -ForegroundColor Cyan
    Write-Host "==> Erstelle Setup.exe" -ForegroundColor Cyan
    & $iscc (Join-Path $deployDir "Setup.iss")

    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup Compiler ist fehlgeschlagen (Exit Code $LASTEXITCODE)."
    }

    Write-Host "==> Fertig. Setup liegt unter deploy\Output\VereinskasseSetup.exe" -ForegroundColor Green
}
finally {
    Pop-Location
}
