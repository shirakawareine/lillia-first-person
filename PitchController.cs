using System;

namespace FirstPersonTest;

internal sealed class PitchController
{
    private readonly float _up, _down, _sensitivity, _direction;
    internal bool Initialized { get; private set; }
    internal float Angle { get; private set; }
    internal PitchController(float up, float down, float sensitivity, bool invert)
    {
        _up = Math.Clamp(float.IsFinite(up) ? up : 75f, 0f, 89f);
        _down = Math.Clamp(float.IsFinite(down) ? down : 85f, 0f, 89f);
        _sensitivity = Math.Clamp(float.IsFinite(sensitivity) ? sensitivity : 3f, 0.01f, 30f);
        _direction = invert ? 1f : -1f;
    }
    internal void Invalidate() => Initialized = false;
    internal void Initialize(float eulerX)
    {
        if (!float.IsFinite(eulerX)) return;
        float signed = (eulerX % 360f + 540f) % 360f - 180f;
        Angle = Math.Clamp(signed, -_up, _down);
        Initialized = true;
    }
    internal void AddMouseDelta(float delta)
    {
        if (!Initialized || !float.IsFinite(delta)) return;
        // Mouse axis is already a per-frame delta; multiplying by deltaTime is incorrect.
        Angle = Math.Clamp(Angle + delta * _sensitivity * _direction, -_up, _down);
    }
}
