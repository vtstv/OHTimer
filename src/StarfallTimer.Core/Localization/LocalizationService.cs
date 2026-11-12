// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using StarfallTimer.Core.Models;
using StarfallTimer.Core.Services;

namespace StarfallTimer.Core.Localization;

/// <summary>
/// Thread-safe multilingual localization provider with built-in English, Russian, and Chinese dictionaries.
/// </summary>
public sealed class LocalizationService : ILocalizationService
{
    private static readonly IReadOnlyList<LanguageInfo> _supportedLanguages = new List<LanguageInfo>
    {
        new("en-US", "English", "English"),
        new("ru-RU", "Russian", "Русский"),
        new("zh-CN", "Chinese (Simplified)", "简体中文")
    }.AsReadOnly();

    private readonly Dictionary<string, Dictionary<string, string>> _catalogs = new(StringComparer.OrdinalIgnoreCase);
    private string _currentLanguage = "en-US";
    private readonly object _lock = new();

    public string CurrentLanguage => _currentLanguage;
    public IReadOnlyList<LanguageInfo> SupportedLanguages => _supportedLanguages;

    public event EventHandler? LanguageChanged;

    public LocalizationService()
    {
        InitializeDefaultCatalogs();
    }

    public void SetLanguage(string cultureCode)
    {
        lock (_lock)
        {
            if (_supportedLanguages.Any(l => string.Equals(l.Code, cultureCode, StringComparison.OrdinalIgnoreCase)))
            {
                _currentLanguage = cultureCode;
            }
            else
            {
                _currentLanguage = "en-US";
            }
        }

        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public string GetString(string key)
    {
        lock (_lock)
        {
            if (_catalogs.TryGetValue(_currentLanguage, out var currentMap) &&
                currentMap.TryGetValue(key, out var localized) &&
                !string.IsNullOrWhiteSpace(localized))
            {
                return localized;
            }

            // Fallback to English
            if (_catalogs.TryGetValue("en-US", out var fallbackMap) &&
                fallbackMap.TryGetValue(key, out var fallback))
            {
                return fallback;
            }

            return $"[{key}]";
        }
    }

    public string Format(string key, params object[] args)
    {
        string pattern = GetString(key);
        try
        {
            return string.Format(pattern, args);
        }
        catch (FormatException)
        {
            return pattern;
        }
    }

    public string this[string key] => GetString(key);

    /// <summary>
    /// Loads custom translations from a JSON file.
    /// </summary>
    public void LoadCustomCatalog(string cultureCode, string jsonContent)
    {
        try
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
            if (data == null) return;

            lock (_lock)
            {
                if (!_catalogs.TryGetValue(cultureCode, out var dict))
                {
                    dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    _catalogs[cultureCode] = dict;
                }

                foreach (var (k, v) in data)
                {
                    dict[k] = v;
                }
            }

            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
            // Ignore malformed custom json
        }
    }

    private void InitializeDefaultCatalogs()
    {
        // 1. English (Default)
        _catalogs["en-US"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["App_Title"] = "Once Human — Starfall Inversion Timer",
            ["Status_Waiting"] = "WAITING",
            ["Status_StartingSoon"] = "STARTING SOON",
            ["Status_Active"] = "INVERSION ACTIVE",
            ["Status_EndingSoon"] = "ENDING SOON",
            ["HUD_UntilStart"] = "STARFALL INVERSION IN",
            ["HUD_UntilEnd"] = "INVERSION ENDS IN",
            ["HUD_ClickThrough"] = "Click-Through",
            ["HUD_Locked"] = "Locked",
            ["HUD_Unlock"] = "Unlock",
            ["HUD_Settings"] = "Settings",
            ["HUD_Close"] = "Hide HUD",

            ["Notification_StartingSoon_Title"] = "Starfall Inversion Alert",
            ["Notification_StartingSoon_Body"] = "Starfall Inversion begins in {0} minutes! Get ready for low gravity tide.",
            ["Notification_Started_Title"] = "Starfall Inversion Active!",
            ["Notification_Started_Body"] = "Low gravity tide and Stardust Crystals are now active!",
            ["Notification_EndingSoon_Title"] = "Starfall Inversion Ending Soon",
            ["Notification_EndingSoon_Body"] = "Event concludes in {0} minutes. Finish collecting your loot!",

            ["Settings_Title"] = "Starfall Inversion Timer — Settings",
            ["Settings_TabGeneral"] = "General",
            ["Settings_TabSchedule"] = "Schedule",
            ["Settings_TabNotifications"] = "Alerts & Audio",
            ["Settings_TabAppearance"] = "Appearance",
            ["Settings_TabHotkeys"] = "Hotkeys",
            ["Settings_TabAbout"] = "About",

            ["Settings_StartMinute"] = "Cycle Start Minute (past the hour):",
            ["Settings_DurationMinutes"] = "Event Duration (minutes):",
            ["Settings_AlertBeforeStart"] = "Alert Before Start (minutes):",
            ["Settings_AlertBeforeEnd"] = "Warning Before End (minutes):",
            ["Settings_Volume"] = "Notification Volume:",
            ["Settings_PlaySound"] = "Enable Audio Alerts",
            ["Settings_PlayOnStart"] = "Play Sound When Inversion Begins",
            ["Settings_ShowToast"] = "Show Windows Desktop Notifications",
            ["Settings_CustomSound"] = "Custom Alert Audio File (.wav):",
            ["Settings_Browse"] = "Browse...",
            ["Settings_TestAudio"] = "Test Audio",
            ["Settings_Opacity"] = "Overlay Opacity:",
            ["Settings_Scale"] = "UI Scale:",
            ["Settings_AlwaysOnTop"] = "Always on Top",
            ["Settings_ClickThrough"] = "Enable Click-Through (clicks pass to game)",
            ["Settings_LockPosition"] = "Lock Overlay Position",
            ["Settings_CompactMode"] = "Compact Mini-HUD Mode",
            ["Settings_Language"] = "Language:",
            ["Settings_ResetPosition"] = "Reset HUD Position to Top-Left",
            ["Settings_Save"] = "Save & Apply",
            ["Settings_SavedSuccess"] = "Settings saved successfully!",

            ["Settings_HotkeyToggle"] = "Toggle Overlay Visibility Hotkey:",
            ["Settings_HotkeyLock"] = "Toggle Click-Through / Lock Hotkey:",

            ["Tray_OpenHUD"] = "Show Overlay",
            ["Tray_HideHUD"] = "Hide Overlay",
            ["Tray_Settings"] = "Settings...",
            ["Tray_ToggleLock"] = "Lock / Unlock Position",
            ["Tray_ToggleClickThrough"] = "Toggle Click-Through",
            ["Tray_Exit"] = "Exit",

            ["About_Desc"] = "A stylish, low-latency HUD overlay timer for the Starfall Inversion event in Once Human.",
            ["About_Author"] = "Created for the Once Human community."
        };

        // 2. Russian
        _catalogs["ru-RU"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["App_Title"] = "Once Human — Таймер Инверсии Звездопада",
            ["Status_Waiting"] = "ОЖИДАНИЕ",
            ["Status_StartingSoon"] = "СКОРО НАЧАЛО",
            ["Status_Active"] = "ИНВЕРСИЯ АКТИВНА",
            ["Status_EndingSoon"] = "СКОРО КОНЕЦ",
            ["HUD_UntilStart"] = "ИНВЕРСИЯ ЗВЕЗДОПАДА ЧЕРЕЗ",
            ["HUD_UntilEnd"] = "КОНЕЦ ИНВЕРСИИ ЧЕРЕЗ",
            ["HUD_ClickThrough"] = "Сквозной клик",
            ["HUD_Locked"] = "Закреплено",
            ["HUD_Unlock"] = "Открепить",
            ["HUD_Settings"] = "Настройки",
            ["HUD_Close"] = "Скрыть HUD",

            ["Notification_StartingSoon_Title"] = "Инверсия Звездопада скоро!",
            ["Notification_StartingSoon_Body"] = "Инверсия Звездопада начнется через {0} мин! Приготовьтесь к смене гравитации.",
            ["Notification_Started_Title"] = "Инверсия Звездопада началась!",
            ["Notification_Started_Body"] = "Гравитационный прилив и Сгустки Звездной Пыли активны прямо сейчас!",
            ["Notification_EndingSoon_Title"] = "Инверсия Звездопада завершается",
            ["Notification_EndingSoon_Body"] = "До конца события осталось {0} мин. Завершайте сбор добычи!",

            ["Settings_Title"] = "Таймер Инверсии Звездопада — Настройки",
            ["Settings_TabGeneral"] = "Основные",
            ["Settings_TabSchedule"] = "Расписание",
            ["Settings_TabNotifications"] = "Звук и алерты",
            ["Settings_TabAppearance"] = "Внешний вид",
            ["Settings_TabHotkeys"] = "Горячие клавиши",
            ["Settings_TabAbout"] = "О программе",

            ["Settings_StartMinute"] = "Минута старта (каждого часа):",
            ["Settings_DurationMinutes"] = "Длительность события (мин):",
            ["Settings_AlertBeforeStart"] = "Оповещать до начала за (мин):",
            ["Settings_AlertBeforeEnd"] = "Предупреждать до конца за (мин):",
            ["Settings_Volume"] = "Громкость оповещений:",
            ["Settings_PlaySound"] = "Звуковые сигналы",
            ["Settings_PlayOnStart"] = "Звук в момент начала инверсии",
            ["Settings_ShowToast"] = "Всплывающие уведомления Windows",
            ["Settings_CustomSound"] = "Пользовательский аудиофайл (.wav):",
            ["Settings_Browse"] = "Обзор...",
            ["Settings_TestAudio"] = "Тест звука",
            ["Settings_Opacity"] = "Прозрачность оверлея:",
            ["Settings_Scale"] = "Масштаб интерфейса:",
            ["Settings_AlwaysOnTop"] = "Поверх всех окон",
            ["Settings_ClickThrough"] = "Сквозной клик (клики проходят в игру)",
            ["Settings_LockPosition"] = "Заблокировать позицию оверлея",
            ["Settings_CompactMode"] = "Компактный режим (мини-виджет)",
            ["Settings_Language"] = "Язык интерфейса:",
            ["Settings_ResetPosition"] = "Сбросить позицию в левый верхний угол",
            ["Settings_Save"] = "Сохранить и применить",
            ["Settings_SavedSuccess"] = "Настройки успешно сохранены!",

            ["Settings_HotkeyToggle"] = "Хоткей скрытия/показа оверлея:",
            ["Settings_HotkeyLock"] = "Хоткей сквозного клика/блокировки:",

            ["Tray_OpenHUD"] = "Показать оверлей",
            ["Tray_HideHUD"] = "Скрыть оверлей",
            ["Tray_Settings"] = "Настройки...",
            ["Tray_ToggleLock"] = "Заблокировать/Разблокировать",
            ["Tray_ToggleClickThrough"] = "Сквозной клик",
            ["Tray_Exit"] = "Выход",

            ["About_Desc"] = "Стильный и быстрый HUD-оверлей таймер для события Инверсия Звездопада в игре Once Human.",
            ["About_Author"] = "Разработано для сообщества Once Human."
        };

        // 3. Simplified Chinese
        _catalogs["zh-CN"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["App_Title"] = "七日世界 — 星降反转计时器",
            ["Status_Waiting"] = "等待中",
            ["Status_StartingSoon"] = "即将开始",
            ["Status_Active"] = "反转进行中",
            ["Status_EndingSoon"] = "即将结束",
            ["HUD_UntilStart"] = "距离星降反转还有",
            ["HUD_UntilEnd"] = "距离反转结束还有",
            ["HUD_ClickThrough"] = "鼠标穿透",
            ["HUD_Locked"] = "锁定位置",
            ["HUD_Unlock"] = "解锁",
            ["HUD_Settings"] = "设置",
            ["HUD_Close"] = "隐藏HUD",

            ["Notification_StartingSoon_Title"] = "星降反转事件提醒",
            ["Notification_StartingSoon_Body"] = "星降反转将在 {0} 分钟后开始！准备好重力反转装备。",
            ["Notification_Started_Title"] = "星降反转现已开始！",
            ["Notification_Started_Body"] = "失重潮汐与星尘结晶现已开启，速去收集！",
            ["Notification_EndingSoon_Title"] = "星降反转即将结束",
            ["Notification_EndingSoon_Body"] = "事件将在 {0} 分钟后结束，抓紧时间完成搜寻！",

            ["Settings_Title"] = "星降反转计时器 — 设置",
            ["Settings_TabGeneral"] = "常规",
            ["Settings_TabSchedule"] = "时间安排",
            ["Settings_TabNotifications"] = "声音与提醒",
            ["Settings_TabAppearance"] = "外观",
            ["Settings_TabHotkeys"] = "快捷键",
            ["Settings_TabAbout"] = "关于",

            ["Settings_StartMinute"] = "每小时开始分钟 (默认22分):",
            ["Settings_DurationMinutes"] = "事件持续时间 (分钟):",
            ["Settings_AlertBeforeStart"] = "开始前提前提醒 (分钟):",
            ["Settings_AlertBeforeEnd"] = "结束前提前警告 (分钟):",
            ["Settings_Volume"] = "提示音量:",
            ["Settings_PlaySound"] = "开启声音警报",
            ["Settings_PlayOnStart"] = "事件开始时播放提示音",
            ["Settings_ShowToast"] = "显示系统通知弹窗",
            ["Settings_CustomSound"] = "自定义音频文件 (.wav):",
            ["Settings_Browse"] = "浏览...",
            ["Settings_TestAudio"] = "测试音效",
            ["Settings_Opacity"] = "窗口不透明度:",
            ["Settings_Scale"] = "界面缩放:",
            ["Settings_AlwaysOnTop"] = "始终置顶",
            ["Settings_ClickThrough"] = "鼠标穿透 (点击穿透至游戏)",
            ["Settings_LockPosition"] = "锁定悬浮窗位置",
            ["Settings_CompactMode"] = "紧凑迷你模式",
            ["Settings_Language"] = "语言:",
            ["Settings_ResetPosition"] = "重置窗口到左上角",
            ["Settings_Save"] = "保存并应用",
            ["Settings_SavedSuccess"] = "设置已成功保存！",

            ["Settings_HotkeyToggle"] = "显示/隐藏悬浮窗快捷键:",
            ["Settings_HotkeyLock"] = "穿透/锁定快捷键:",

            ["Tray_OpenHUD"] = "显示悬浮窗",
            ["Tray_HideHUD"] = "隐藏悬浮窗",
            ["Tray_Settings"] = "设置...",
            ["Tray_ToggleLock"] = "锁定/解锁位置",
            ["Tray_ToggleClickThrough"] = "切换鼠标穿透",
            ["Tray_Exit"] = "退出",

            ["About_Desc"] = "专为《七日世界》打造的星降反转事件高精度HUD悬浮计时器。",
            ["About_Author"] = "为七日世界玩家社区制作。"
        };
    }
}
