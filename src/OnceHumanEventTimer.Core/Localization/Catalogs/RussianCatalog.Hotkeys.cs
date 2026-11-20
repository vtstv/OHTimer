// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Localization.Catalogs;

internal static partial class RussianCatalog
{
    static partial void RegisterHotkeyEntries(Dictionary<string, string> dict)
    {
        dict["Settings_HotkeysCardTitle"] = "Глобальные горячие клавиши";
        dict["Settings_HotkeysHint"] = "Нажмите на поле, чтобы назначить новую клавишу или комбинацию. Esc для отмены.";
        dict["Settings_HotkeyToggle"] = "Хоткей скрытия/показа оверлея:";
        dict["Settings_HotkeyToggleDesc"] = "Быстрое скрытие или отображение оверлея во время игры.";
        dict["Settings_HotkeyLock"] = "Хоткей сквозного клика/блокировки:";
        dict["Settings_HotkeyLockDesc"] = "Блокировка позиции и включение сквозного клика в игру.";
        dict["Settings_HotkeyPauseAlerts"] = "Хоткей паузы оповещений:";
        dict["Settings_HotkeyPauseAlertsDesc"] = "Временная пауза или включение всех звуков и уведомлений.";
        dict["Settings_HotkeyBossTimer"] = "Хоткей таймера босса:";
        dict["Settings_HotkeyBossTimerDesc"] = "Запуск, перезапуск или переключение таймера респауна босса.";
        dict["Settings_HotkeyResetTooltip"] = "Сбросить на клавишу по умолчанию";
    }
}
