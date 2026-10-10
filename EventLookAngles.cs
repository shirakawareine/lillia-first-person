using System;
using System.Collections.Generic;
using System.Globalization;

namespace FirstPersonTest;

internal sealed class EventLookAngles
{
    internal float Yaw { get; private set; }
    internal float Pitch { get; private set; }
    internal void Reset() { Yaw = Pitch = 0; }
    internal void Add(float x, float y, float sensitivity, bool invert)
    {
        if (float.IsFinite(x)) Yaw += Math.Clamp(x * sensitivity, -90, 90);
        if (float.IsFinite(y)) Pitch += Math.Clamp(y * sensitivity * (invert ? 1 : -1), -90, 90);
    }
    internal void Clamp(float yaw, float up, float down)
    {
        Yaw = Math.Clamp(Yaw, -yaw, yaw);
        Pitch = Math.Clamp(Pitch, -up, down);
    }
    internal static Dictionary<string, (float yaw, float up, float down)> Parse(string text, Action<string> log)
    {
        var result = new Dictionary<string, (float, float, float)>(StringComparer.Ordinal);
        foreach (string entry in text.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] parts = entry.Split('|');
            if (parts.Length != 4 || string.IsNullOrWhiteSpace(parts[0])
                || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float yaw)
                || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float up)
                || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float down)
                || !float.IsFinite(yaw) || !float.IsFinite(up) || !float.IsFinite(down)
                || yaw < 0 || up < 0 || down < 0 || yaw > 89 || up > 89 || down > 89)
            { log("Event view: invalid clip limit entry ignored: " + entry); continue; }
            result[parts[0].Trim()] = (yaw, up, down);
        }
        return result;
    }
}
