# Releases

This directory contains packaged release distributions for **Once Human: Event Timer**.

## Creating a New Release

To produce a new release with automated testing, optimization, and packaging:

```powershell
# Default release (version parsed from StarfallTimer.UI.csproj, lightweight framework-dependent ~0.7 MB)
.\scripts\build-release.ps1

# Custom version
.\scripts\build-release.ps1 -Version "1.1.0"

# Self-contained standalone build (requires no .NET installation, larger bundle)
.\scripts\build-release.ps1 -Version "1.1.0" -Standalone:$true
```

Or double-click `scripts\build-release.cmd` on Windows.

### Output Artifacts
- Folder: `release/OnceHumanTimer-v<version>/`
- Single-File Executable: `OnceHumanStarfallTimer.exe` (< 1 MB framework-dependent)
- Bundled Zip: `release/OnceHumanTimer-v<version>.zip`
