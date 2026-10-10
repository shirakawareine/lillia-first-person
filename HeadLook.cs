using System;
using UnityEngine;

namespace FirstPersonTest;

internal sealed class HeadLook
{
    private Transform? _saved;
    private Quaternion _original, _applied;

    // This model's head is treated as local +Z forward, +Y up.
    // Absolute world orientation replaces animated pitch instead of adding to it.
    internal void Apply(Transform head, Quaternion viewRotation)
    {
        Restore();
        if (head == null) return;
        _original = head.localRotation;
        _saved = head;
        _applied = _original;
        head.rotation = viewRotation;
        _applied = head.localRotation;
    }

    internal void Restore()
    {
        // Preserve a newer pose authored by Animator/IK/another script.
        if (_saved != null && Quaternion.Angle(_saved.localRotation, _applied) < 0.1f)
            _saved.localRotation = _original;
        _saved = null;
    }
}

