using System;

namespace FirstPersonTest;

internal static class SprintPolicy
{
    // Forward cone includes keyboard W+A/W+D (45 degrees), excludes side/back.
    internal static bool Allows(float forward, float right) =>
        float.IsFinite(forward) && float.IsFinite(right)
        && forward > 0.01f && forward + 0.0001f >= MathF.Abs(right);
}
