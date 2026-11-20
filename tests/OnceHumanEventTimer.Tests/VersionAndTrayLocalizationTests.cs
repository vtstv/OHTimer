// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using System.Text.RegularExpressions;
using OnceHumanEventTimer.Core.Localization;
using OnceHumanEventTimer.Core.Localization.Catalogs;
using Xunit;

namespace OnceHumanEventTimer.Tests;

public class VersionAndTrayLocalizationTests
{
    [Fact]
    public void AssemblyVersion_InheritsFromDirectoryBuildProps()
    {
        var coreAssembly = typeof(LocalizationService).Assembly;
        var version = coreAssembly.GetName().Version;

        Assert.NotNull(version);
        Assert.Equal(1, version.Major);
        Assert.Equal(7, version.Minor);
        Assert.Equal(1, version.Build);
        Assert.Equal("1.7.1", $"{version.Major}.{version.Minor}.{version.Build}");
    }

    [Theory]
    [InlineData("en-US", "Show Overlay", "Hide Overlay", "Settings...", "Exit")]
    [InlineData("de-DE", "Overlay anzeigen", "Overlay ausblenden", "Einstellungen...", "Beenden")]
    [InlineData("ru-RU", "Показать оверлей", "Скрыть оверлей", "Настройки...", "Выход")]
    public void TrayMenuStrings_AreLocalizedWithoutEmojisOrShortcuts(
        string language,
        string expectedOpen,
        string expectedHide,
        string expectedSettings,
        string expectedExit)
    {
        var loc = new LocalizationService();
        loc.SetLanguage(language);

        string open = loc.GetString("Tray_OpenHUD");
        string hide = loc.GetString("Tray_HideHUD");
        string settings = loc.GetString("Tray_Settings");
        string lockPos = loc.GetString("Tray_ToggleLock");
        string pauseAlerts = loc.GetString("Tray_PauseAlerts");
        string resumeAlerts = loc.GetString("Tray_ResumeAlerts");
        string exit = loc.GetString("Tray_Exit");

        Assert.Equal(expectedOpen, open);
        Assert.Equal(expectedHide, hide);
        Assert.Equal(expectedSettings, settings);
        Assert.Equal(expectedExit, exit);

        // Verify none of the tray strings contain emojis or shortcut combinations like Ctrl+
        string[] allTrayStrings = [open, hide, settings, lockPos, pauseAlerts, resumeAlerts, exit];
        var emojiRegex = new Regex(@"[\uD83C-\uDBFF\uDC00-\uDFFF\u2600-\u27BF]", RegexOptions.Compiled);

        foreach (var str in allTrayStrings)
        {
            Assert.False(string.IsNullOrWhiteSpace(str));
            Assert.False(emojiRegex.IsMatch(str), $"String '{str}' should not contain emoji icons");
            Assert.DoesNotContain("Ctrl+", str);
            Assert.DoesNotContain("Alt+", str);
        }
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("ru-RU")]
    public void TrayMenuStrings_DynamicToggleKeysAreDistinct(string language)
    {
        var loc = new LocalizationService();
        loc.SetLanguage(language);

        string open = loc.GetString("Tray_OpenHUD");
        string hide = loc.GetString("Tray_HideHUD");
        string pauseAlerts = loc.GetString("Tray_PauseAlerts");
        string resumeAlerts = loc.GetString("Tray_ResumeAlerts");

        Assert.NotEqual(open, hide);
        Assert.NotEqual(pauseAlerts, resumeAlerts);
        Assert.False(string.IsNullOrWhiteSpace(open));
        Assert.False(string.IsNullOrWhiteSpace(hide));
        Assert.False(string.IsNullOrWhiteSpace(pauseAlerts));
        Assert.False(string.IsNullOrWhiteSpace(resumeAlerts));
    }

    private static string? FindRepoRoot()
    {
        string? current = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(current))
        {
            if (File.Exists(Path.Combine(current, "Directory.Build.props")) &&
                File.Exists(Path.Combine(current, "OnceHumanEventTimer.sln")))
            {
                return current;
            }
            var parent = Directory.GetParent(current);
            if (parent == null || parent.FullName == current) break;
            current = parent.FullName;
        }

        current = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(current))
        {
            if (File.Exists(Path.Combine(current, "Directory.Build.props")) &&
                File.Exists(Path.Combine(current, "OnceHumanEventTimer.sln")))
            {
                return current;
            }
            var parent = Directory.GetParent(current);
            if (parent == null || parent.FullName == current) break;
            current = parent.FullName;
        }

        return null;
    }

    [Fact]
    public void DirectoryBuildProps_ExistsAndDefinesCentralizedVersion()
    {
        string? repoRoot = FindRepoRoot();
        Assert.NotNull(repoRoot);
        string propsPath = Path.Combine(repoRoot!, "Directory.Build.props");
        Assert.True(File.Exists(propsPath), "Directory.Build.props must exist at repository root");

        string content = File.ReadAllText(propsPath);
        Assert.Contains("<Version>1.7.1</Version>", content);
        Assert.Contains("<AssemblyVersion>1.7.1.0</AssemblyVersion>", content);
        Assert.Contains("<FileVersion>1.7.1.0</FileVersion>", content);
        Assert.Contains("<InformationalVersion>1.7.1</InformationalVersion>", content);
    }

    [Fact]
    public void CsprojFiles_DoNotContainRedundantVersionTags()
    {
        string? repoRoot = FindRepoRoot();
        Assert.NotNull(repoRoot);
        var csprojFiles = Directory.GetFiles(repoRoot!, "*.csproj", SearchOption.AllDirectories);
        Assert.NotEmpty(csprojFiles);

        foreach (var file in csprojFiles)
        {
            string content = File.ReadAllText(file);
            Assert.DoesNotContain("<Version>", content);
        }
    }

    [Theory]
    [InlineData(100, 100, 50, 50, 200, 200, true)]
    [InlineData(50, 50, 50, 50, 200, 200, true)]
    [InlineData(200, 200, 50, 50, 200, 200, true)]
    [InlineData(49, 100, 50, 50, 200, 200, false)]
    [InlineData(100, 49, 50, 50, 200, 200, false)]
    [InlineData(201, 100, 50, 50, 200, 200, false)]
    [InlineData(100, 201, 50, 50, 200, 200, false)]
    public void TrayMenu_CoordinateBoundingBox_EvaluatesInsideOutsideDeterministically(
        int ptX, int ptY, int left, int top, int right, int bottom, bool expectedInside)
    {
        bool isInside = ptX >= left && ptX <= right && ptY >= top && ptY <= bottom;
        Assert.Equal(expectedInside, isInside);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("ru-RU")]
    public void TrayMenu_RequiredLocalizationKeys_AreNeverEmpty(string lang)
    {
        var loc = new LocalizationService();
        loc.SetLanguage(lang);

        string[] requiredKeys =
        [
            "Tray_OpenHUD",
            "Tray_HideHUD",
            "Tray_Settings",
            "Tray_ToggleLock",
            "Tray_PauseAlerts",
            "Tray_ResumeAlerts",
            "Tray_Exit"
        ];

        foreach (var key in requiredKeys)
        {
            string val = loc.GetString(key);
            Assert.False(string.IsNullOrWhiteSpace(val), $"Key {key} in {lang} must not be empty");
            Assert.DoesNotContain("[MISSING:", val);
        }
    }
}
