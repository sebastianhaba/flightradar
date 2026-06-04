using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using FlightRadar.Shared;

namespace FlightRadar.UI.Views;

public class RadarCanvas : Control
{
    public static readonly DirectProperty<RadarCanvas, List<AircraftData>?> AircraftProperty =
        AvaloniaProperty.RegisterDirect<RadarCanvas, List<AircraftData>?>(
            nameof(Aircraft), o => o.Aircraft, (o, v) => o.Aircraft = v);

    private List<AircraftData>? _aircraft;
    public List<AircraftData>? Aircraft
    {
        get => _aircraft;
        set { SetAndRaise(AircraftProperty, ref _aircraft, value); InvalidateVisual(); }
    }

    public static readonly StyledProperty<double> CenterLatProperty =
        AvaloniaProperty.Register<RadarCanvas, double>(nameof(CenterLat));

    public static readonly StyledProperty<double> CenterLonProperty =
        AvaloniaProperty.Register<RadarCanvas, double>(nameof(CenterLon));

    public double CenterLat
    {
        get => GetValue(CenterLatProperty);
        set => SetValue(CenterLatProperty, value);
    }

    public double CenterLon
    {
        get => GetValue(CenterLonProperty);
        set => SetValue(CenterLonProperty, value);
    }

    private const double MaxRangeKm = 25;

    static RadarCanvas()
    {
        AffectsRender<RadarCanvas>(AircraftProperty, CenterLatProperty, CenterLonProperty);
    }

    public override void Render(DrawingContext ctx)
    {
        var size = Math.Min(Bounds.Width, Bounds.Height);
        var cx = Bounds.Width / 2;
        var cy = Bounds.Height / 2;
        var radius = size / 2 - 20;
        var pxPerKm = radius / MaxRangeKm;

        DrawBackground(ctx, cx, cy, radius);
        DrawRings(ctx, cx, cy, radius, pxPerKm);
        DrawCardinals(ctx, cx, cy, radius);

        if (Aircraft is not null)
            DrawAircraft(ctx, cx, cy, pxPerKm, radius);
    }

    private static void DrawBackground(DrawingContext ctx, double cx, double cy, double radius)
    {
        ctx.DrawEllipse(new SolidColorBrush(Colors.Black), null, new Point(cx, cy), radius, radius);
    }

    private static void DrawRings(DrawingContext ctx, double cx, double cy, double radius, double pxPerKm)
    {
        var ringPen = new Pen(new SolidColorBrush(Color.FromRgb(40, 60, 40)), 1);
        for (int i = 1; i <= 5; i++)
        {
            var r = i * 5 * pxPerKm;
            ctx.DrawEllipse(null, ringPen, new Point(cx, cy), r, r);
        }

        var crossPen = new Pen(new SolidColorBrush(Color.FromRgb(30, 50, 30)), 0.5);
        ctx.DrawLine(crossPen, new Point(cx - radius, cy), new Point(cx + radius, cy));
        ctx.DrawLine(crossPen, new Point(cx, cy - radius), new Point(cx, cy + radius));

        var ft = new FormattedText("25 km", CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            Typeface.Default, 11, new SolidColorBrush(Color.FromRgb(0, 255, 0)));
        ctx.DrawText(ft, new Point(cx + radius - ft.Width - 4, cy + 2));
    }

    private static void DrawCardinals(DrawingContext ctx, double cx, double cy, double radius)
    {
        var white = new SolidColorBrush(Colors.White);
        var gap = 6;

        var n = new FormattedText("N", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 13, white);
        var s = new FormattedText("S", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 13, white);
        var w = new FormattedText("W", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 13, white);
        var e = new FormattedText("E", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 13, white);

        ctx.DrawText(n, new Point(cx - n.Width / 2, cy - radius - gap - n.Height));
        ctx.DrawText(s, new Point(cx - s.Width / 2, cy + radius + gap));
        ctx.DrawText(w, new Point(cx - radius - gap - w.Width, cy - w.Height / 2));
        ctx.DrawText(e, new Point(cx + radius + gap, cy - e.Height / 2));
    }

    private void DrawAircraft(DrawingContext ctx, double cx, double cy, double pxPerKm, double radius)
    {
        foreach (var ac in Aircraft!)
        {
            var dx = (ac.Longitude - CenterLon) * 111320 * Math.Cos(CenterLat * Math.PI / 180);
            var dy = (ac.Latitude - CenterLat) * 111320;
            var distKm = Math.Sqrt(dx * dx + dy * dy) / 1000;
            var bearing = Math.Atan2(dx, dy) * 180 / Math.PI;
            if (bearing < 0) bearing += 360;

            if (distKm > MaxRangeKm)
            {
                var rimAngle = bearing * Math.PI / 180;
                var dotX = cx + Math.Sin(rimAngle) * (radius - 8);
                var dotY = cy - Math.Cos(rimAngle) * (radius - 8);
                ctx.DrawEllipse(new SolidColorBrush(Colors.Red), null, new Point(dotX, dotY), 3, 3);
                continue;
            }

            var px = distKm * pxPerKm;
            var angleRad = bearing * Math.PI / 180;
            var x = cx + Math.Sin(angleRad) * px;
            var y = cy - Math.Cos(angleRad) * px;

            DrawAircraftIcon(ctx, x, y, ac.Heading, ac.IsHelicopter);
            DrawAircraftLabel(ctx, x, y, ac);
        }
    }

    private static void DrawAircraftIcon(DrawingContext ctx, double x, double y, double heading, bool isHeli)
    {
        var headingRad = heading * Math.PI / 180;
        var size = 7;

        var tip = new Point(x + Math.Sin(headingRad) * size, y - Math.Cos(headingRad) * size);
        var left = new Point(x + Math.Sin(headingRad + 2.4) * size * 0.55, y - Math.Cos(headingRad + 2.4) * size * 0.55);
        var right = new Point(x + Math.Sin(headingRad - 2.4) * size * 0.55, y - Math.Cos(headingRad - 2.4) * size * 0.55);

        var color = isHeli ? Color.FromRgb(255, 200, 0) : Color.FromRgb(255, 80, 80);
        var fill = new SolidColorBrush(color);

        var geo = new StreamGeometry();
        using (var sgc = geo.Open())
        {
            if (isHeli)
            {
                sgc.BeginFigure(new Point(x - 4, y - 3), true);
                sgc.LineTo(new Point(x + 4, y - 3));
                sgc.LineTo(new Point(x + 4, y + 3));
                sgc.LineTo(new Point(x - 4, y + 3));
                sgc.EndFigure(true);
                var rotorPen = new Pen(new SolidColorBrush(Color.FromRgb(200, 200, 200)), 1);
                ctx.DrawLine(rotorPen, new Point(x - 6, y), new Point(x + 6, y));
            }
            else
            {
                sgc.BeginFigure(tip, true);
                sgc.LineTo(left);
                sgc.LineTo(right);
                sgc.EndFigure(true);
            }
        }
        ctx.DrawGeometry(fill, null, geo);
    }

    private static void DrawAircraftLabel(DrawingContext ctx, double x, double y, AircraftData ac)
    {
        var callsign = !string.IsNullOrWhiteSpace(ac.Callsign) ? ac.Callsign
            : ac.IcaoHex.Length >= 6 ? ac.IcaoHex[..6] : ac.IcaoHex;

        var gray1 = new SolidColorBrush(Color.FromRgb(200, 200, 200));
        var gray2 = new SolidColorBrush(Color.FromRgb(150, 150, 150));

        var cs = new FormattedText(callsign, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            Typeface.Default, 10, gray1);
        var alt = new FormattedText($"{ac.Altitude}' | {ac.Heading:F0}°", CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            Typeface.Default, 9, gray2);

        ctx.DrawText(cs, new Point(x - cs.Width / 2, y + 10));
        ctx.DrawText(alt, new Point(x - alt.Width / 2, y + 22));
    }
}
