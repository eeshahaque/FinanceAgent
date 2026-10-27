using System.Globalization;
using System.Text;

namespace FinanceAgent.Components.Shared;

// Geometry helpers for the server-rendered SVG charts
public static class ChartMath
{
    public static string F(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);

    // Smooth line through the points (Catmull-Rom converted to cubic Béziers), clamped so the
    // curve never overshoots below the baseline or above the top of the plot.
    public static string SmoothLine(IReadOnlyList<(double X, double Y)> pts, double minY, double maxY)
    {
        if (pts.Count == 0) return "";
        var sb = new StringBuilder($"M{F(pts[0].X)},{F(pts[0].Y)}");
        for (int i = 0; i < pts.Count - 1; i++)
        {
            var p0 = pts[Math.Max(0, i - 1)];
            var p1 = pts[i];
            var p2 = pts[i + 1];
            var p3 = pts[Math.Min(pts.Count - 1, i + 2)];
            const double t = 6; // higher = tighter curve
            var c1 = (p1.X + (p2.X - p0.X) / t, Math.Clamp(p1.Y + (p2.Y - p0.Y) / t, minY, maxY));
            var c2 = (p2.X - (p3.X - p1.X) / t, Math.Clamp(p2.Y - (p3.Y - p1.Y) / t, minY, maxY));
            sb.Append($" C{F(c1.Item1)},{F(c1.Item2)} {F(c2.Item1)},{F(c2.Item2)} {F(p2.X)},{F(p2.Y)}");
        }
        return sb.ToString();
    }

    public static string SmoothArea(IReadOnlyList<(double X, double Y)> pts, double baseline, double minY) =>
        pts.Count == 0 ? "" :
        $"{SmoothLine(pts, minY, baseline)} L{F(pts[^1].X)},{F(baseline)} L{F(pts[0].X)},{F(baseline)} Z";

    // A "nice" axis maximum (1, 2, 2.5, 5 × 10^n) at or above the value
    public static double NiceMax(double value)
    {
        if (value <= 0) return 100;
        var exp = Math.Pow(10, Math.Floor(Math.Log10(value)));
        foreach (var step in new[] { 1, 1.2, 1.5, 2, 2.5, 3, 4, 5, 6, 8, 10 })
            if (step * exp >= value) return step * exp;
        return 10 * exp;
    }

    public static string Compact(double v) =>
        Math.Abs(v) >= 1000 ? $"${(v / 1000).ToString(v % 1000 == 0 ? "0" : "0.#", CultureInfo.InvariantCulture)}k" : $"${v:0}";
}
