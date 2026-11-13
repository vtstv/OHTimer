# Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

<#
.SYNOPSIS
    Builds, publishes, and packages a release of Once Human: Event Timer.

.DESCRIPTION
    Runs automated unit tests, publishes optimized binaries (lightweight framework-dependent or standalone),
    bundles documentation and licenses, and generates a distributable ZIP package inside the 'release' directory
    with version suffix.

.PARAMETER Version
    The semantic version string for the release (e.g., "1.1.0").
    If omitted, extracts the version from OnceHumanEventTimer.UI.csproj.

.PARAMETER Runtime
    The target runtime identifier (default: "win-x64").

.PARAMETER Standalone
    When specified as $true, produces a self-contained single-file executable.
    Default: $false (framework-dependent for ultra-lightweight ~2-4MB distribution; requires .NET 8 Desktop Runtime).

.PARAMETER SkipTests
    Skips running xUnit tests prior to packaging.

.EXAMPLE
    .\scripts\build-release.ps1 -Version "1.1.0"
    .\scripts\build-release.ps1 -Version "1.1.0" -Standalone:$true
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$Version = "",

    [string]$Runtime = "win-x64",

    [bool]$Standalone = $false,

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
        $Version = "1.2.1"
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

# 4. Prepare Output Directories
$releaseBaseDir = Join-Path $repoRoot "release"
$releaseFolder = "OnceHumanTimer-v$Version"
$releaseTargetDir = Join-Path $releaseBaseDir $releaseFolder
$zipFilePath = Join-Path $releaseBaseDir "$releaseFolder.zip"

if (Test-Path $releaseTargetDir) {
    Write-Host "      Cleaning existing target directory: $releaseTargetDir" -ForegroundColor Gray
    Remove-Item -Recurse -Force $releaseTargetDir
}
if (Test-Path $zipFilePath) {
    Remove-Item -Force $zipFilePath
}
New-Item -ItemType Directory -Force -Path $releaseTargetDir | Out-Null

# 5. Publish Application
Write-Host "[4/6] Publishing application..." -ForegroundColor Yellow

$selfContainedArg = if ($Standalone) { "--self-contained" } else { "--no-self-contained" }
$publishArgs = @(
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
    "-o", $releaseTargetDir
)

if ($Standalone) {
    $publishArgs += "-p:EnableCompressionInSingleFile=true"
}

Write-Host "      Executing: dotnet $($publishArgs -join ' ')" -ForegroundColor Gray
& dotnet $publishArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed."
    exit 1
}

# 6. Copy Bundled Documentation and Assets
Write-Host "[5/6] Bundling documentation, changelog, and licenses..." -ForegroundColor Yellow
Copy-Item (Join-Path $repoRoot "LICENSE") -Destination $releaseTargetDir -Force
Copy-Item (Join-Path $repoRoot "README.md") -Destination $releaseTargetDir -Force

$changelogSrc = Join-Path $repoRoot "CHANGELOG.md"
if (Test-Path $changelogSrc) {
    Copy-Item $changelogSrc -Destination $releaseTargetDir -Force
    # Generate release/CHANGELOG.txt for GitHub releases
    $changelogContent = Get-Content $changelogSrc -Raw
    Set-Content -Path (Join-Path $releaseBaseDir "CHANGELOG.txt") -Value $changelogContent -Encoding UTF8
    Set-Content -Path (Join-Path $releaseTargetDir "CHANGELOG.txt") -Value $changelogContent -Encoding UTF8
}

# 7. Create ZIP Package
Write-Host "[6/6] Creating ZIP archive..." -ForegroundColor Yellow
Compress-Archive -Path "$releaseTargetDir\*" -DestinationPath $zipFilePath -Force

$zipItem = Get-Item $zipFilePath
$zipSizeMb = [math]::Round($zipItem.Length / 1MB, 2)
$sha256 = (Get-FileHash -Path $zipFilePath -Algorithm SHA256).Hash

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host " RELEASE BUILD SUCCESSFUL!                               " -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
Write-Host " Package Directory : $releaseTargetDir" -ForegroundColor White
Write-Host " ZIP Archive       : $zipFilePath" -ForegroundColor White
Write-Host " Archive Size      : $zipSizeMb MB" -ForegroundColor White
Write-Host " SHA256 Checksum   : $sha256" -ForegroundColor White
Write-Host " Executable        : OnceHumanEventTimer.exe" -ForegroundColor White
Write-Host " Mode              : $(if ($Standalone) { 'Self-Contained (Plug & Play)' } else { 'Framework-Dependent (Lightweight)' })" -ForegroundColor White
Write-Host "==========================================================`n" -ForegroundColor Green
