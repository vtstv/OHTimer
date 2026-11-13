// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OnceHumanEventTimer.UI.Controls;

public partial class CircularTimerProgress : UserControl
{
    public static readonly DependencyProperty ProgressProperty =
        DependencyProperty.Register(
            nameof(Progress),
            typeof(double),
            typeof(CircularTimerProgress),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, OnProgressChanged));

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public CircularTimerProgress()
    {
        InitializeComponent();
        SizeChanged += (s, e) => UpdateArc();
    }

    private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CircularTimerProgress control)
        {
            control.UpdateArc();
        }
    }

    private void UpdateArc()
    {
        double width = ActualWidth - 12;
        double height = ActualHeight - 12;
        if (width <= 0 || height <= 0) return;

        double radius = Math.Min(width, height) / 2.0;
        Point center = new(radius, radius);

        double progress = Math.Clamp(Progress, 0.0, 0.9999);
        double angle = progress * 360.0;

        // Start at 12 o'clock (-90 degrees)
        double startRad = -Math.PI / 2.0;
        double endRad = startRad + (angle * Math.PI / 180.0);

        Point startPoint = new(
            center.X + radius * Math.Cos(startRad),
            center.Y + radius * Math.Sin(startRad));

        Point endPoint = new(
            center.X + radius * Math.Cos(endRad),
            center.Y + radius * Math.Sin(endRad));

        bool isLargeArc = angle > 180.0;

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(startPoint, false, false);
            ctx.ArcTo(endPoint, new Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise, true, false);
        }
        geometry.Freeze();

        ArcPath.Data = geometry;
    }
}
