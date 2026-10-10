using System;

namespace FirstPersonTest;

// Update once per game frame, never once per camera. No accumulated bone offsets.
internal sealed class IdleViewBlend
{
    private double _lastActivity;
    private bool _ready;
    internal float Weight { get; private set; }
    internal bool Controlling(double now) => !_ready || now - _lastActivity < 1.25;
    internal void Activity(double now) { _lastActivity = now; }
    internal void Reset() { _ready = false; Weight = 0; }
    internal void Step(double now, float delta, bool allowed, bool moving)
    {
        if (!allowed) { Reset(); return; }
        if (!_ready) { _lastActivity = now; _ready = true; }
        if (moving) Activity(now);
        float target = Controlling(now) ? 0 : 1;
        float amount = Math.Clamp(float.IsFinite(delta) ? delta : 0, 0, .1f) / (target > Weight ? .8f : .25f);
        Weight = target > Weight ? Math.Min(target, Weight + amount) : Math.Max(target, Weight - amount);
    }
}
