// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Localization.Catalogs;

internal static partial class RussianCatalog
{
    static partial void RegisterBossTimerEntries(Dictionary<string, string> dict)
    {
        dict["Settings_TabBossTimer"] = "Таймер боссов";
        dict["BossTimer_CardActivation"] = "Активация и горячая клавиша";
        dict["BossTimer_Enable"] = "Включить таймер респауна босса";
        dict["BossTimer_Hotkey"] = "Горячая клавиша:";
        dict["BossTimer_HotkeyRecorderPrompt"] = "[ Нажмите клавишу... ]";
        dict["BossTimer_HotkeyResetTooltip"] = "Сбросить на клавишу по умолчанию (Alt+NumPad 1)";
        dict["BossTimer_RepressBehavior"] = "При повторном нажатии:";
        dict["BossTimer_Repress_Restart"] = "Перезапуск отсчета";
        dict["BossTimer_Repress_Toggle"] = "Переключить Старт / Стоп";
        dict["BossTimer_Repress_Stop"] = "Остановить обратный отсчет";
        dict["BossTimer_PlayHotkeySound"] = "Звуковой сигнал при нажатии";

        dict["BossTimer_CardDuration"] = "Длительность и имя босса";
        dict["BossTimer_BossName"] = "Имя цели / босса:";
        dict["BossTimer_DurationMinutes"] = "Минуты:";
        dict["BossTimer_DurationSeconds"] = "Секунды:";
        dict["BossTimer_QuickPresets"] = "Быстрые пресеты:";
        dict["BossTimer_StartAction"] = "▶ Запустить отсчет";
        dict["BossTimer_StopAction"] = "⏹ Остановить отсчет";

        dict["BossTimer_CardAudio"] = "Звуковые и повторные оповещения";
        dict["BossTimer_WarningAlert"] = "Предупреждение перед респауном";
        dict["BossTimer_WarningSeconds"] = "За (сек.):";
        dict["BossTimer_SpawnAlert"] = "Оповещение в 00:00 (Респаун)";
        dict["BossTimer_SoundPreset"] = "Звуковой пресет респауна:";
        dict["BossTimer_Volume"] = "Громкость оповещения:";
        dict["BossTimer_TestSound"] = "Тест";
        dict["BossTimer_EnableRepeatAlert"] = "Повторять оповещение после респауна";
        dict["BossTimer_RepeatSettings"] = "Параметры:";
        dict["BossTimer_RepeatCount"] = "Повторов:";
        dict["BossTimer_RepeatInterval"] = "Интервал (сек.):";

        dict["BossTimer_CardVisuals"] = "Визуальные эффекты и оформление";
        dict["BossTimer_ShowToast"] = "Показывать уведомление Windows";
        dict["BossTimer_FlashCard"] = "Мигать карточкой HUD при респауне";
        dict["BossTimer_AutoHide"] = "Скрывать после респауна через (сек.):";
        dict["BossTimer_Color"] = "Цвет акцента:";
        dict["BossTimer_ColorTooltip"] = "Выберите цвет таймера босса из палитры";

        dict["BossTimer_Status_Respawn"] = "РЕСПАУН";
        dict["BossTimer_Status_Warning"] = "ПРИБЛИЖАЕТСЯ";
        dict["BossTimer_Status_Spawned"] = "ПОЯВИЛСЯ!";
        dict["BossTimer_SpawnToastTitle"] = "Босс возродился!";
        dict["BossTimer_SpawnToastMsg"] = "возродился! Готовьтесь к бою.";
    }
}
