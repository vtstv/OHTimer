// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Localization.Catalogs;

internal static partial class RussianCatalog
{
    static partial void RegisterKillCounterEntries(Dictionary<string, string> dict)
    {
        dict["Settings_TabKillCounter"] = "Счётчик убийств";
        dict["KillCounter_CardTarget"] = "Цель и задачи забегов";
        dict["KillCounter_TargetName"] = "Имя цели / босса:";
        dict["KillCounter_QuickPresets"] = "Быстрые пресеты:";
        dict["KillCounter_CurrentCount"] = "Текущие убийства / забеги:";
        dict["KillCounter_EnableGoal"] = "Включить цель по убийствам / забегам";
        dict["KillCounter_GoalCount"] = "Количество цели:";
        dict["KillCounter_TotalAllTime"] = "Всего за всё время:";
        dict["KillCounter_ResetSession"] = "↺ Сброс сессии";

        dict["KillCounter_CardActivation"] = "Активация и горячие клавиши";
        dict["KillCounter_Enable"] = "Включить счётчик фарма и убийств";
        dict["KillCounter_IncrementHotkey"] = "Добавить убийство (+1):";
        dict["KillCounter_DecrementHotkey"] = "Убавить / Отмена (-1):";
        dict["KillCounter_HotkeyResetTooltip"] = "Сбросить на клавишу по умолчанию";
        dict["KillCounter_PlaySound"] = "Звуковой сигнал при добавлении / отмене";
        dict["KillCounter_AutoStartBossTimer"] = "Автозапуск таймера респауна босса при убийстве";
        dict["KillCounter_ShowInMiniHud"] = "Показывать значок счётчика в Мини-HUD";
        dict["KillCounter_FlashOnGoalReached"] = "Мигать HUD-карточкой при достижении цели";
        dict["KillCounter_Color"] = "Цвет акцента:";
        dict["KillCounter_ColorTooltip"] = "Выбрать цвет счётчика из палитры";

        dict["KillCounter_Status_GoalReached"] = "ЦЕЛЬ ДОСТИГНУТА! 🎉";
        dict["KillCounter_GoalReachedTitle"] = "Цель по убийствам достигнута!";
        dict["KillCounter_GoalReachedMsg"] = "Поздравляем! Вы завершили {0} забегов на {1}!";
        dict["Settings_HotkeyKillIncrement"] = "Счётчик убийств (+1)";
        dict["Settings_HotkeyKillIncrementDesc"] = "Добавляет 1 к активному счётчику убийств и опционально запускает таймер босса.";
        dict["Settings_HotkeyKillDecrement"] = "Счётчик убийств (-1 / Отмена)";
        dict["Settings_HotkeyKillDecrementDesc"] = "Отменяет последнее добавленное убийство при случайном нажатии.";
        dict["KillCounter_BadgeKills"] = "УБИЙСТВА";
        dict["KillCounter_BadgeGoal"] = "ЦЕЛЬ";
        dict["KillCounter_ResetPrompt"] = "Сбросить счётчик текущей сессии на 0? Общее количество за всё время сохранится.";
    }
}
