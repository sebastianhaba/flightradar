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

    // Cached resources — created once, reused every frame
    private static readonly IBrush BlackBrush = new SolidColorBrush(Colors.Black);
    private static readonly IBrush WhiteBrush = new SolidColorBrush(Colors.White);
    private static readonly IBrush RedBrush = new SolidColorBrush(Colors.Red);
    private static readonly IBrush RingBrush = new SolidColorBrush(Color.FromRgb(40, 60, 40));
    private static readonly IBrush CrossBrush = new SolidColorBrush(Color.FromRgb(30, 50, 30));
    private static readonly IBrush RangeTextBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0));
    private static readonly IBrush AirplaneFill = new SolidColorBrush(Color.FromRgb(255, 80, 80));
    private static readonly IBrush HeliFill = new SolidColorBrush(Color.FromRgb(255, 200, 0));
    private static readonly IBrush RotorBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
    private static readonly IBrush CallsignBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
    private static readonly IBrush AltBrush = new SolidColorBrush(Color.FromRgb(150, 150, 150));

    private static readonly Pen RingPen = new(RingBrush, 1);
    private static readonly Pen CrossPen = new(CrossBrush, 0.5);
    private static readonly Pen RotorPen = new(RotorBrush, 1);

    private static readonly FormattedText LabelN = new("N", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 13, WhiteBrush);
    private static readonly FormattedText LabelS = new("S", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 13, WhiteBrush);
    private static readonly FormattedText LabelW = new("W", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 13, WhiteBrush);
    private static readonly FormattedText LabelE = new("E", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 13, WhiteBrush);
    private static readonly FormattedText Label25Km = new("25 km", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 11, RangeTextBrush);

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

    private static void DrawCardinals(DrawingContext ctx, double cx, double cy, double radius)
    {
        var gap = 6;

        ctx.DrawText(LabelN, new Point(cx - LabelN.Width / 2, cy - radius - gap - LabelN.Height));
        ctx.DrawText(LabelS, new Point(cx - LabelS.Width / 2, cy + radius + gap));
        ctx.DrawText(LabelW, new Point(cx - radius - gap - LabelW.Width, cy - LabelW.Height / 2));
        ctx.DrawText(LabelE, new Point(cx + radius + gap, cy - LabelE.Height / 2));
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
                ctx.DrawEllipse(RedBrush, null, new Point(dotX, dotY), 3, 3);
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
