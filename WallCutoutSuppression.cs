using System;
using System.Collections.Generic;
using UnityEngine;

namespace FirstPersonTest;

// Suspend the game's camera-driven wall cutout, not general transparent materials.
internal sealed class WallCutoutSuppression
{
    private readonly Action<string> _log;
    private readonly List<WallCutoutController> _owned = new();
    private Camera? _camera;
    private double _nextSearch;
    internal WallCutoutSuppression(Action<string> log) => _log = log;

    internal void Tick(Camera? camera, bool active)
    {
        if (!active || camera == null || !camera.isActiveAndEnabled)
        { Restore(); return; }
        if (_camera != camera) { Restore(); _camera = camera; }
        for (int i = _owned.Count - 1; i >= 0; --i)
        {
            var c = _owned[i];
            if (c == null) { _owned.RemoveAt(i); continue; }
            if (!c.gameObject.activeInHierarchy || c.ResolveCamera() != camera)
            {
                c.enabled = true;
                _owned.RemoveAt(i);
                continue;
            }
            if (c.enabled) c.enabled = false;
        }
        double now = Environment.TickCount64 / 1000.0;
        if (now < _nextSearch) return;
        _nextSearch = now + 1;
        var controllers = UnityEngine.Object.FindObjectsOfType<WallCutoutController>();
        foreach (var c in controllers)
        {
            if (c == null || !c.isActiveAndEnabled || c.ResolveCamera() != camera || _owned.Contains(c)) continue;
            // Disabling invokes the game's OnDisable cleanup of its shader/culling state.
            // Record ownership first so a later error can still restore the component.
            _owned.Add(c);
            c.enabled = false;
            _log($"Wall cutout: suspended {c.name}, camera={camera.name}, nearFade={c.nearFade}; native OnDisable cleanup requested.");
        }
    }

    internal void Restore()
    {
        foreach (var c in _owned)
        {
            try { if (c != null && !c.enabled) c.enabled = true; }
            catch (Exception e) { _log("Wall cutout: restore failed: " + e.Message); }
        }
        if (_owned.Count > 0) _log("Wall cutout: released suspended components.");
        _owned.Clear(); _camera = null; _nextSearch = 0;
    }
}
