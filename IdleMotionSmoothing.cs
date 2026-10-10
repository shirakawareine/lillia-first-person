using System;
using UnityEngine;

namespace FirstPersonTest;

internal sealed class IdleMotionSmoothing
{
    private bool _ready;
    private int _frame = -1;
    private Quaternion _rotation;
    internal void Reset() { _ready = false; _frame = -1; }
    internal Quaternion Sample(int frame, float delta, Quaternion animated)
    {
        if (_ready && frame == _frame) return _rotation;
        _frame = frame;
        if (!_ready) { _ready = true; return _rotation = animated; }
        float dt = Math.Clamp(float.IsFinite(delta) ? delta : 0, 0, .1f);
        // Smooth only animation motion, never mouse input. One update per frame
        // keeps LateUpdate and both cameras from applying the filter repeatedly.
        return _rotation = Quaternion.Slerp(_rotation, animated, 1f - MathF.Exp(-dt / .12f));
    }
}
