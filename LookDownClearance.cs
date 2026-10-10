using System;

namespace FirstPersonTest;

// Pure math so limits and continuity can be verified without a Unity runtime.
internal static class LookDownClearance
{
    internal static float Amount(float pitch)
    {
        if (!float.IsFinite(pitch)) return 0;
        float t = Math.Clamp((pitch - 30f) / 50f, 0f, 1f);
        return t * t * (3f - 2f * t);
    }
    internal static float Distance(float value, float fallback, float maximum)
        => Math.Clamp(float.IsFinite(value) ? value : fallback, 0f, maximum);
}
