# Releases

This directory contains packaged release distributions for **Once Human: Starfall Inversion Timer**.

## Creating a New Release

To produce a new release with automated testing, optimization, and packaging:

```powershell
# Default release (version parsed from StarfallTimer.UI.csproj, self-contained single-file)
.\scripts\build-release.ps1

# Custom version
.\scripts\build-release.ps1 -Version "1.0.1"

# Framework-dependent build (requires .NET 8 Desktop Runtime, ultra-lightweight)
.\scripts\build-release.ps1 -Version "1.0.1" -Standalone:$false
```

Or double-click `scripts\build-release.cmd` on Windows.

### Output Artifacts
- Folder: `release/OnceHumanStarfallTimer-v<version>/`
- Single-File Executable: `OnceHumanStarfallTimer.exe` (no installation required)
- Bundled Zip: `release/OnceHumanStarfallTimer-v<version>.zip`
