// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace OnceHumanEventTimer.UI.Controls;

/// <summary>
/// Interactive hotkey recording control styled in Once Human Stardust aesthetic.
/// </summary>
public partial class HotkeyBox : UserControl
{
    private static readonly Brush NormalBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"));
    private static readonly Brush RecordingBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00F0FF"));
    private static readonly Brush NormalTextBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00F0FF"));
    private static readonly Brush RecordingTextBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38BDF8"));

    private static readonly DropShadowEffect NeonGlow = new()
    {
        Color = (Color)ColorConverter.ConvertFromString("#00F0FF"),
        BlurRadius = 8,
        ShadowDepth = 0,
        Opacity = 0.75
    };

    public static readonly DependencyProperty HotkeyProperty =
        DependencyProperty.Register(
            nameof(Hotkey),
            typeof(string),
            typeof(HotkeyBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHotkeyChanged));

    public static readonly DependencyProperty PromptTextProperty =
        DependencyProperty.Register(
            nameof(PromptText),
            typeof(string),
            typeof(HotkeyBox),
            new PropertyMetadata("[ Press a key... ]"));

    public static readonly DependencyProperty IsRecordingProperty =
        DependencyProperty.Register(
            nameof(IsRecording),
            typeof(bool),
            typeof(HotkeyBox),
            new PropertyMetadata(false, OnIsRecordingChanged));

    public string Hotkey
    {
        get => (string)GetValue(HotkeyProperty);
        set => SetValue(HotkeyProperty, value);
    }

    public string PromptText
    {
        get => (string)GetValue(PromptTextProperty);
        set => SetValue(PromptTextProperty, value);
    }

    public bool IsRecording
    {
        get => (bool)GetValue(IsRecordingProperty);
        set => SetValue(IsRecordingProperty, value);
    }

    public HotkeyBox()
    {
        InitializeComponent();
        Loaded += (s, e) => UpdateDisplay();
        MouseLeftButtonDown += (s, e) => { Focus(); StartRecording(); e.Handled = true; };
        LostFocus += (s, e) => CancelRecording();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!IsRecording && (e.Key == Key.Enter || e.Key == Key.Space))
        {
            StartRecording();
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (!IsRecording) return;

        if (e.Key == Key.Escape)
        {
            CancelRecording();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Tab)
        {
            CancelRecording();
            return;
        }

        Key realKey = e.Key == Key.System ? e.SystemKey : e.Key;

        // Wait for non-modifier key
        if (realKey is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or
            Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin)
        {
            return;
        }

        var parts = new List<string>();
        var modifiers = Keyboard.Modifiers;

        if ((modifiers & ModifierKeys.Control) != 0) parts.Add("Ctrl");
        if ((modifiers & ModifierKeys.Alt) != 0) parts.Add("Alt");
        if ((modifiers & ModifierKeys.Shift) != 0) parts.Add("Shift");
        if ((modifiers & ModifierKeys.Windows) != 0) parts.Add("Win");

        parts.Add(FormatKeyName(realKey));

        Hotkey = string.Join("+", parts);
        StopRecording();
        e.Handled = true;
    }

    public void StartRecording()
    {
        IsRecording = true;
    }

    public void CancelRecording()
    {
        IsRecording = false;
        UpdateDisplay();
    }

    public void StopRecording()
    {
        IsRecording = false;
        UpdateDisplay();
    }

    private static void OnHotkeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HotkeyBox box && !box.IsRecording)
        {
            box.UpdateDisplay();
        }
    }

    private static void OnIsRecordingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HotkeyBox box)
        {
            box.ApplyVisualState((bool)e.NewValue);
        }
    }

    private void ApplyVisualState(bool recording)
    {
        if (recording)
        {
            RootBorder.BorderBrush = RecordingBorderBrush;
            RootBorder.Effect = NeonGlow;
            HotkeyText.Text = PromptText;
            HotkeyText.Foreground = RecordingTextBrush;
        }
        else
        {
            RootBorder.BorderBrush = NormalBorderBrush;
            RootBorder.Effect = null;
            HotkeyText.Foreground = NormalTextBrush;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (!IsRecording)
        {
            HotkeyText.Text = !string.IsNullOrWhiteSpace(Hotkey) ? Hotkey : "None";
        }
    }

    private static string FormatKeyName(Key key) => key switch
    {
        Key.NumPad0 => "NumPad 0",
        Key.NumPad1 => "NumPad 1",
        Key.NumPad2 => "NumPad 2",
        Key.NumPad3 => "NumPad 3",
        Key.NumPad4 => "NumPad 4",
        Key.NumPad5 => "NumPad 5",
        Key.NumPad6 => "NumPad 6",
        Key.NumPad7 => "NumPad 7",
        Key.NumPad8 => "NumPad 8",
        Key.NumPad9 => "NumPad 9",
        Key.Multiply => "NumPad *",
        Key.Add => "NumPad +",
        Key.Subtract => "NumPad -",
        Key.Decimal => "NumPad .",
        Key.Divide => "NumPad /",
        Key.D0 => "0",
        Key.D1 => "1",
        Key.D2 => "2",
        Key.D3 => "3",
        Key.D4 => "4",
        Key.D5 => "5",
        Key.D6 => "6",
        Key.D7 => "7",
        Key.D8 => "8",
        Key.D9 => "9",
        _ => key.ToString()
    };
}
