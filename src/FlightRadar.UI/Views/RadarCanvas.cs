using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using FlightRadar.Shared;

namespace FlightRadar.UI.Views;

public class RadarCanvas : Control
{
    public static readonly DirectProperty<RadarCanvas, IList<AircraftData>?> AircraftProperty =
        AvaloniaProperty.RegisterDirect<RadarCanvas, IList<AircraftData>?>(
            nameof(Aircraft), o => o.Aircraft, (o, v) => o.Aircraft = v);

    private IList<AircraftData>? _aircraft;
    public IList<AircraftData>? Aircraft
    {
        get => _aircraft;
        set
        {
            if (_aircraft is INotifyCollectionChanged oldColl)
                oldColl.CollectionChanged -= OnAircraftCollectionChanged;

            SetAndRaise(AircraftProperty, ref _aircraft, value);

            if (_aircraft is INotifyCollectionChanged newColl)
                newColl.CollectionChanged += OnAircraftCollectionChanged;

            InvalidateVisual();
        }
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

    private void OnAircraftCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private const double MaxRangeKm = 25;

    private static readonly IBrush BlackBrush = new SolidColorBrush(Colors.Black);
    private static readonly IBrush WhiteBrush = new SolidColorBrush(Color.FromRgb(128, 255, 128));
    private static readonly IBrush OutOfRangeBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0));
    private static readonly IBrush RingBrush = new SolidColorBrush(Color.FromRgb(40, 60, 40));
    private static readonly IBrush CrossBrush = new SolidColorBrush(Color.FromRgb(30, 50, 30));
    private static readonly IBrush TickBrush = new SolidColorBrush(Color.FromRgb(45, 70, 45));
    private static readonly IBrush DegBrush = new SolidColorBrush(Color.FromRgb(80, 140, 80));
    private static readonly IBrush RangeTextBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0));
    private static readonly IBrush AirplaneFill = new SolidColorBrush(Color.FromRgb(0, 255, 128));
    private static readonly IBrush HeliFill = new SolidColorBrush(Color.FromRgb(0, 204, 96));
    private static readonly IBrush RotorBrush = new SolidColorBrush(Color.FromRgb(74, 138, 74));
    private static readonly IBrush CallsignBrush = new SolidColorBrush(Color.FromRgb(128, 255, 128));
    private static readonly IBrush AltBrush = new SolidColorBrush(Color.FromRgb(74, 138, 74));

    private static readonly Pen RingPen = new(RingBrush, 1);
    private static readonly Pen CrossPen = new(CrossBrush, 0.5);
    private static readonly Pen TickPen = new(TickBrush, 1);
    private static readonly Pen RotorPen = new(RotorBrush, 1);

    private static readonly FormattedText LabelN = new("N", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 13, WhiteBrush);
    private static readonly FormattedText LabelS = new("S", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 13, WhiteBrush);
    private static readonly FormattedText LabelW = new("W", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 13, WhiteBrush);
    private static readonly FormattedText LabelE = new("E", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 13, WhiteBrush);
    private static readonly FormattedText Label25Km = new("25 km", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 11, RangeTextBrush);

    private static readonly FormattedText[] DegreeLabels = InitDegreeLabels();

    private static FormattedText[] InitDegreeLabels()
    {
        var labels = new FormattedText[360 / 45];
        for (int i = 0; i < labels.Length; i++)
        {
            var deg = i * 45;
            labels[i] = new FormattedText($"{deg}°", CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, Typeface.Default, 10, DegBrush);
        }
        return labels;
    }

    static RadarCanvas()
    {
        AffectsRender<RadarCanvas>(AircraftProperty, CenterLatProperty, CenterLonProperty);
    }

    public override void Render(DrawingContext ctx)
    {
        var size = Math.Min(Bounds.Width, Bounds.Height);
        var cx = Bounds.Width / 2;
        var cy = Bounds.Height / 2;
        var radius = size / 2 - 30;
        var pxPerKm = radius / MaxRangeKm;

        DrawBackground(ctx, cx, cy, radius);
        DrawRings(ctx, cx, cy, radius, pxPerKm);
        DrawTicks(ctx, cx, cy, radius);
        DrawCardinals(ctx, cx, cy, radius);

        if (Aircraft is not null)
            DrawAircraft(ctx, cx, cy, pxPerKm, radius);
    }

    private static void DrawBackground(DrawingContext ctx, double cx, double cy, double radius)
    {
        ctx.DrawEllipse(BlackBrush, null, new Point(cx, cy), radius, radius);
    }

    private static void DrawRings(DrawingContext ctx, double cx, double cy, double radius, double pxPerKm)
    {
        for (int i = 1; i <= 5; i++)
        {
            var r = i * 5 * pxPerKm;
            ctx.DrawEllipse(null, RingPen, new Point(cx, cy), r, r);
        }

        ctx.DrawLine(CrossPen, new Point(cx - radius, cy), new Point(cx + radius, cy));
        ctx.DrawLine(CrossPen, new Point(cx, cy - radius), new Point(cx, cy + radius));

        ctx.DrawText(Label25Km, new Point(cx + radius - Label25Km.Width - 4, cy + 2));
    }

    private static void DrawTicks(DrawingContext ctx, double cx, double cy, double radius)
    {
        for (int deg = 0; deg < 360; deg += 5)
        {
            var angleRad = deg * Math.PI / 180;
            var tickLen = deg % 45 == 0 ? 6 : 2;
            var inner = radius - tickLen;

            var x1 = cx + Math.Sin(angleRad) * inner;
            var y1 = cy - Math.Cos(angleRad) * inner;
            var x2 = cx + Math.Sin(angleRad) * radius;
            var y2 = cy - Math.Cos(angleRad) * radius;

            ctx.DrawLine(TickPen, new Point(x1, y1), new Point(x2, y2));
        }
    }

    private static void DrawCardinals(DrawingContext ctx, double cx, double cy, double radius)
    {
        var degGap = 6;
        var cardGap = 24;

        DrawCardinal(ctx, LabelN, 0, cx, cy, radius, degGap, cardGap);
        DrawCardinal(ctx, LabelE, 90, cx, cy, radius, degGap, cardGap);
        DrawCardinal(ctx, LabelS, 180, cx, cy, radius, degGap, cardGap);
        DrawCardinal(ctx, LabelW, 270, cx, cy, radius, degGap, cardGap);

        for (int i = 0; i < 8; i++)
        {
            var deg = i * 45;
            var angleRad = deg * Math.PI / 180;
            var label = DegreeLabels[i];
            var dr = radius + degGap;
            var dx = cx + Math.Sin(angleRad) * dr - label.Width / 2;
            var dy = cy - Math.Cos(angleRad) * dr - label.Height / 2;
            ctx.DrawText(label, new Point(dx, dy));
        }
    }

    private static void DrawCardinal(DrawingContext ctx, FormattedText label, int deg,
        double cx, double cy, double radius, double degGap, double cardGap)
    {
        var angleRad = deg * Math.PI / 180;
        var cr = radius + cardGap;
        var dx = cx + Math.Sin(angleRad) * cr - label.Width / 2;
        var dy = cy - Math.Cos(angleRad) * cr - label.Height / 2;
        ctx.DrawText(label, new Point(dx, dy));
    }

    private void DrawAircraft(DrawingContext ctx, double cx, double cy, double pxPerKm, double radius)
    {
        foreach (var ac in Aircraft!)
        {
            var (distKm, bearing) = GeoMath.DistanceAndBearing(CenterLat, CenterLon, ac.Latitude, ac.Longitude);

            if (distKm > MaxRangeKm)
            {
                var rimAngle = bearing * Math.PI / 180;
                var dotX = cx + Math.Sin(rimAngle) * (radius - 8);
                var dotY = cy - Math.Cos(rimAngle) * (radius - 8);
                ctx.DrawEllipse(OutOfRangeBrush, null, new Point(dotX, dotY), 3, 3);
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
                ctx.DrawLine(RotorPen, new Point(x - 6, y), new Point(x + 6, y));
            }
            else
            {
                sgc.BeginFigure(tip, true);
                sgc.LineTo(left);
                sgc.LineTo(right);
                sgc.EndFigure(true);
            }
        }
        ctx.DrawGeometry(isHeli ? HeliFill : AirplaneFill, null, geo);
    }

    private static void DrawAircraftLabel(DrawingContext ctx, double x, double y, AircraftData ac)
    {
        var callsign = !string.IsNullOrWhiteSpace(ac.Callsign) ? ac.Callsign
            : ac.IcaoHex.Length >= 6 ? ac.IcaoHex[..6] : ac.IcaoHex;

        var cs = new FormattedText(callsign, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            Typeface.Default, 10, CallsignBrush);
        var alt = new FormattedText($"{ac.Altitude}' | {ac.Heading:F0}°", CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            Typeface.Default, 9, AltBrush);

        ctx.DrawText(cs, new Point(x - cs.Width / 2, y + 10));
        ctx.DrawText(alt, new Point(x - alt.Width / 2, y + 22));
    }
}
