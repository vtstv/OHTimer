# Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

<#
.SYNOPSIS
    Builds, publishes, and packages releases for Once Human: Event Timer.

.DESCRIPTION
    Supports building:
    1. Portable (Framework-Dependent): ultra-lightweight (~0.38MB ZIP, requires .NET 8 runtime).
    2. Standalone (Self-Contained): plug-and-play (~63MB ZIP, zero dependencies).
    3. Windows Setup Installer: Inno Setup (.exe installer with shortcuts and uninstaller).
    4. -All: builds all 3 packages and extracts release notes in one command.

.PARAMETER Version
    The semantic version string (e.g., "1.5.0"). If omitted, read from UI csproj.

.PARAMETER Runtime
    Target runtime identifier (default: "win-x64").

.PARAMETER Standalone
    Builds the self-contained single-file portable archive.

.PARAMETER Installer
    Compiles the Inno Setup Windows installer.

.PARAMETER All
    Builds Portable, Standalone, and Windows Installer packages together.

.PARAMETER SkipTests
    Skips running automated xUnit tests.
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$Version = "",

    [string]$Runtime = "win-x64",

    [switch]$Standalone,

    [switch]$Installer,

    [switch]$All,

    [switch]$SkipTests = $false
)

$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptRoot
Set-Location $repoRoot

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Once Human — Event Timer : Release Packager              " -ForegroundColor Cyan
Write-Host " Author: Murr (https://github.com/vtstv)                  " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Resolve Version
$projectPath = Join-Path $repoRoot "src\OnceHumanEventTimer.UI\OnceHumanEventTimer.UI.csproj"
if ([string]::IsNullOrWhiteSpace($Version)) {
    [xml]$projXml = Get-Content $projectPath
    $Version = $projXml.Project.PropertyGroup.Version
    if ([string]::IsNullOrWhiteSpace($Version)) {
        $Version = "1.5.2"
    }
}

Write-Host "[1/6] Target Release Version: v$Version ($Runtime)" -ForegroundColor Green

# 2. Check .NET SDK
Write-Host "[2/6] Checking .NET SDK..." -ForegroundColor Yellow
$dotnetVer = & dotnet --version
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to execute 'dotnet'. Please ensure .NET 8+ SDK is installed."
    exit 1
}
Write-Host "      Detected .NET SDK version: $dotnetVer" -ForegroundColor Gray

# 3. Run Automated Tests
if (-not $SkipTests) {
    Write-Host "[3/6] Running automated test suite..." -ForegroundColor Yellow
    & dotnet test (Join-Path $repoRoot "OnceHumanEventTimer.sln") -c Release --nologo
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Unit tests failed. Release build aborted."
        exit 1
    }
    Write-Host "      All tests passed successfully!" -ForegroundColor Green
} else {
    Write-Host "[3/6] Skipping tests as requested (-SkipTests)." -ForegroundColor DarkYellow
}

# 4. Prepare Output Directories & Stop Running Instances
$releaseBaseDir = Join-Path $repoRoot "release"
if (-not (Test-Path $releaseBaseDir)) {
    New-Item -ItemType Directory -Force -Path $releaseBaseDir | Out-Null
}

Get-Process -Name "OnceHumanEventTimer", "OnceHumanStarfallTimer" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 500

# Helper function to find ISCC compiler
function Find-InnoSetupCompiler {
    $candidates = @(
        (Get-Command iscc.exe -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source -ErrorAction SilentlyContinue),
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
    )
    foreach ($path in $candidates) {
        if (![string]::IsNullOrEmpty($path) -and (Test-Path $path)) {
            return $path
        }
    }
    return $null
}

# Helper to publish a specific profile
function Publish-Target {
    param(
        [string]$TargetDir,
        [bool]$IsSelfContained
    )

    if (Test-Path $TargetDir) {
        Remove-Item -Recurse -Force $TargetDir
    }
    New-Item -ItemType Directory -Force -Path $TargetDir | Out-Null

    $selfContainedArg = if ($IsSelfContained) { "--self-contained" } else { "--no-self-contained" }
    $pArgs = @(
        "publish", $projectPath,
        "-c", "Release",
        "-r", $Runtime,
        $selfContainedArg,
        "-p:PublishSingleFile=true",
        "-p:IncludeNativeLibrariesForSelfExtract=true",
        "-p:DebugType=none",
        "-p:DebugSymbols=false",
        "-p:Version=$Version",
        "-p:AssemblyVersion=$Version.0",
        "-p:FileVersion=$Version.0",
        "-o", $TargetDir
    )
    if ($IsSelfContained) {
        $pArgs += "-p:EnableCompressionInSingleFile=true"
    }

    Write-Host "      Executing: dotnet $($pArgs -join ' ')" -ForegroundColor Gray
    & dotnet $pArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error "dotnet publish failed."
        exit 1
    }

    Copy-Item (Join-Path $repoRoot "LICENSE") -Destination $TargetDir -Force
    Copy-Item (Join-Path $repoRoot "README.md") -Destination $TargetDir -Force
    $changelogSrc = Join-Path $repoRoot "CHANGELOG.md"
    if (Test-Path $changelogSrc) {
        Copy-Item $changelogSrc -Destination $TargetDir -Force
    }
}

# 5. Extract Changelog & Release Notes
Write-Host "[4/6] Generating Release Notes..." -ForegroundColor Yellow
$changelogSrc = Join-Path $repoRoot "CHANGELOG.md"
if (Test-Path $changelogSrc) {
    $fullChangelog = Get-Content $changelogSrc -Raw
    Set-Content -Path (Join-Path $releaseBaseDir "CHANGELOG.txt") -Value $fullChangelog -Encoding UTF8

    $escapedVer = [regex]::Escape($Version)
    if ($fullChangelog -match "(?ms)(## \[$escapedVer\].*?)(?=---|\z)") {
        $versionNotes = $matches[1].Trim()
        Set-Content -Path (Join-Path $releaseBaseDir "RELEASE_NOTES_v$Version.md") -Value $versionNotes -Encoding UTF8
        Set-Content -Path (Join-Path $releaseBaseDir "RELEASE_NOTES.md") -Value $versionNotes -Encoding UTF8
        Write-Host "      Extracted release notes for v$Version." -ForegroundColor Gray
    }
}

$buildPortable = $All -or (-not $Standalone -and -not $Installer)
$buildStandalone = $All -or $Standalone
$buildInstaller = $All -or $Installer

# Build Portable
if ($buildPortable) {
    Write-Host "[5/6] Building Portable Package (Framework-Dependent)..." -ForegroundColor Yellow
    $portableDir = Join-Path $releaseBaseDir "OnceHumanTimer-v$Version-portable"
    Publish-Target -TargetDir $portableDir -IsSelfContained $false

    $portableZip = Join-Path $releaseBaseDir "OnceHumanTimer-v$Version-portable.zip"
    $defaultZip = Join-Path $releaseBaseDir "OnceHumanTimer-v$Version.zip"

    if (Test-Path $portableZip) { Remove-Item -Force $portableZip }
    if (Test-Path $defaultZip) { Remove-Item -Force $defaultZip }

    Compress-Archive -Path "$portableDir\*" -DestinationPath $portableZip -Force
    Copy-Item $portableZip -Destination $defaultZip -Force

    $pSize = [math]::Round((Get-Item $portableZip).Length / 1KB, 2)
    Write-Host "      Portable ZIP created: $portableZip ($pSize KB)" -ForegroundColor Green
}

# Build Standalone
$standaloneDir = Join-Path $releaseBaseDir "OnceHumanTimer-v$Version"
if ($buildStandalone -or $buildInstaller) {
    Write-Host "[5/6] Building Standalone Package (Self-Contained)..." -ForegroundColor Yellow
    Publish-Target -TargetDir $standaloneDir -IsSelfContained $true

    if ($buildStandalone) {
        $standaloneZip = Join-Path $releaseBaseDir "OnceHumanTimer-v$Version-standalone.zip"
        if (Test-Path $standaloneZip) { Remove-Item -Force $standaloneZip }
        Compress-Archive -Path "$standaloneDir\*" -DestinationPath $standaloneZip -Force
        $sSize = [math]::Round((Get-Item $standaloneZip).Length / 1MB, 2)
        Write-Host "      Standalone ZIP created: $standaloneZip ($sSize MB)" -ForegroundColor Green
    }
}

# Build Installer
if ($buildInstaller) {
    Write-Host "[6/6] Building Windows Setup Installer..." -ForegroundColor Yellow
    $iscc = Find-InnoSetupCompiler
    if ($iscc) {
        $issScript = Join-Path $repoRoot "scripts\installer\setup.iss"
        $installerArgs = @(
            "/Qp",
            "/DMyAppVersion=$Version",
            "/DSourceDir=$standaloneDir",
            "/DOutputDir=$releaseBaseDir",
            "/DOutputBaseFilename=OnceHumanTimer-v$Version-Setup",
            $issScript
        )
        Write-Host "      Compiling: $iscc $($installerArgs -join ' ')" -ForegroundColor Gray
        & $iscc $installerArgs
        if ($LASTEXITCODE -eq 0) {
            $setupPath = Join-Path $releaseBaseDir "OnceHumanTimer-v$Version-Setup.exe"
            $setupSize = [math]::Round((Get-Item $setupPath).Length / 1MB, 2)
            Write-Host "      Installer created: $setupPath ($setupSize MB)" -ForegroundColor Green
        } else {
            Write-Warning "Inno Setup compiler failed with exit code $LASTEXITCODE."
        }
    } else {
        Write-Warning "Inno Setup compiler (ISCC.exe) was not found. Skipping installer generation."
    }
}

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host " RELEASE BUILD COMPLETED SUCCESSFULLY!                   " -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
Get-ChildItem -Path $releaseBaseDir -Filter "*v$Version*" | Select-Object Name, @{Name="Size (MB)"; Expression={[math]::Round($_.Length/1MB, 2)}}, LastWriteTime | Format-Table -AutoSize
Write-Host "==========================================================`n" -ForegroundColor Green
