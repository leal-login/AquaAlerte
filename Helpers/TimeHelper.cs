using System.Globalization;

namespace AquaAlerte.Helpers;

public static class TimeHelper
{
    public static readonly CultureInfo Fr = new("fr-FR");

    // 13 h 50
    public static string Heure(DateTime d) => $"{d.Hour:00} h {d.Minute:00}";

    // 40 min  ou  6 h 20
    public static string FormatDuree(TimeSpan d)
    {
        if (d.TotalMinutes < 1) return "moins d’1 min";
        if (d.TotalMinutes < 60) return $"{(int)d.TotalMinutes} min";
        return $"{(int)d.TotalHours} h {d.Minutes:00}";
    }

        // Lit « 08 h 10 » et renvoie 8 heures 10 minutes
    public static bool TryHeure(string? text, out TimeSpan result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(text)) return false;

        var parts = text.Split('h');
        if (parts.Length != 2) return false;

        if (int.TryParse(parts[0].Trim(), out int h) && int.TryParse(parts[1].Trim(), out int m)
            && h is >= 0 and <= 23 && m is >= 0 and <= 59)
        {
            result = new TimeSpan(h, m, 0);
            return true;
        }
        return false;
    }
}