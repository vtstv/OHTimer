// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Localization.Catalogs;

internal static partial class RussianCatalog
{
    static partial void RegisterOnboardingEntries(Dictionary<string, string> dict)
    {
        dict["Onboarding_WindowTitle"] = "Быстрый старт — Once Human: Event Timer";
        dict["Onboarding_Header"] = "ИНТЕРАКТИВНОЕ ОБУЧЕНИЕ";
        dict["Onboarding_StepIndicator"] = "Шаг {0} из {1}";
        dict["Onboarding_Skip"] = "Пропустить";
        dict["Onboarding_Back"] = "Назад";
        dict["Onboarding_Next"] = "Далее";
        dict["Onboarding_Finish"] = "Завершить и начать";

        dict["Onboarding_Step1_Title"] = "Безоконный HUD-оверлей и управление";
        dict["Onboarding_Step1_Subtitle"] = "Интерактивный виджет без потери FPS";
        dict["Onboarding_Step1_Desc"] = "Once Human: Event Timer работает поверх игры с нулевой потерей кадров (DirectFlip/MPO). В разблокированном состоянии перетащите оверлей в любое место экрана. Кнопка 🗖 переключает между полным HUD и компактным Mini-HUD.";
        dict["Onboarding_Step1_Tip"] = "Включите сквозные клики (Ctrl+Alt+L), чтобы мышь кликала прямо сквозь оверлей в игру без потери фокуса.";
        dict["Onboarding_Step2_Title"] = "Мульти-таймер и приоритет пресетов";
        dict["Onboarding_Step2_Subtitle"] = "Выберите события для отслеживания прямо на оверлее";
        dict["Onboarding_Step2_Desc"] = "Выберите события, которые хотите отслеживать на оверлее. По умолчанию включено S5: The Echo. Отметьте нужные события:\n• S5: The Echo (Диссонансный резонанс)\n• Заварушка с грузом (Грузовик со спорками)\n• Дрейфующая теплица (Ручной Sync)\nОтметьте несколько галочек для одновременного отслеживания в режиме Multi-Timer!";
        dict["Onboarding_Step2_Tip"] = "Используйте кнопки 🔝, ▲, ▼ в настройках для сортировки или «⧉ Дублировать» для создания копий.";
        dict["Onboarding_Step3_Title"] = "Сезон 5: Эхо и портал босса";
        dict["Onboarding_Step3_Subtitle"] = "Старт в XX:15:00 и тревога открытия портала босса на 15-й минуте";
        dict["Onboarding_Step3_Desc"] = "Событие Эхо спускается каждый час в XX:15:00. Ровно через 15 минут (XX:30:00) открывается портал группового данжа The Echo War на 4 игрока!\nВключите «Таймер только до портала босса», чтобы видеть отсчёт прямо до босса, с отдельной сиреной тревоги на 15-й минуте.";
        dict["Onboarding_Step3_Tip"] = "Вы можете отключить стартовый колокол и оставить звук только на открытие босса.";
        dict["Onboarding_Step4_Title"] = "Быстрая калибровка по карте игры";
        dict["Onboarding_Step4_Subtitle"] = "Синхронизация секунда в секунду с сервером Once Human";
        dict["Onboarding_Step4_Desc"] = "Время серверов игры может расходиться с часами ПК. Просто посмотрите на таймер на карте или в игре (например, «1ч 3м 29с» или «01:03:29»), введите его в поле калибровки и нажмите «Откалибровать». Таймер мгновенно рассчитает точные координаты цикла!";
        dict["Onboarding_Step4_Tip"] = "Поддерживает естественный ввод на русском, английском и немецком («1ч 3м 29с», «1h 3m 29s», «1 Std 3 Min 29 Sek»).";
        dict["Onboarding_Step5_Title"] = "Горячие клавиши и системный трей";
        dict["Onboarding_Step5_Subtitle"] = "Управление в бою и тихий фоновый режим";
        dict["Onboarding_Step5_Desc"] = "Управляйте оверлеем в игре без открытия окон:\n• Ctrl+Alt+S — Показать или скрыть оверлей.\n• Ctrl+Alt+L — Заблокировать позицию и включить сквозные клики (клики идут в игру).\n• Ctrl+Alt+P — Заглушить/возобновить звуковые оповещения (индикатор: 🔕).\nРаботает в системном трее Windows с нагрузкой < 0.1% CPU.";
        dict["Onboarding_Step5_Tip"] = "Комбинации клавиш можно изменить во вкладке «Горячие клавиши». Это обучение можно запустить снова во вкладке «О программе».";
        dict["Settings_TutorialTitle"] = "Интерактивное обучение";
        dict["Settings_TutorialDesc"] = "Запустить пошаговое руководство по управлению оверлеем, хоткеям, таймеру босса S5 и калибровке времени.";
        dict["Settings_LaunchTutorial"] = "Запустить обучение";
        dict["Onboarding_SelectEvents"] = "Выбор событий для отслеживания";
        dict["Onboarding_MultiTimerHint"] = "✓ Оверлей отображает все отмеченные события, когда они активны или приближаются";
    }
}
