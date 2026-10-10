using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FirstPersonTest;

internal sealed class PlayerVisibility
{
    private readonly Action<string> _log;
    private readonly List<Renderer> _renderers = new();
    private readonly List<Saved> _saved = new();
    private GameObject? _root;
    private double _nextRefresh;
    private bool _reported;
    private sealed class Saved
    {
        internal Renderer Renderer = null!;
        internal bool ForceOff;
        internal ShadowCastingMode Shadows;
    }
    internal PlayerVisibility(Action<string> log) => _log = log;
    internal void Refresh(GameObject? root)
    {
        double now = Environment.TickCount64 / 1000.0;
        bool changed = root != _root;
        if (!changed && now < _nextRefresh) return;
        _nextRefresh = now + 5;
        _root = root;
        _renderers.Clear();
        if (changed) _reported = false;
        if (root == null) return;
        var list = root.GetComponentsInChildren<Renderer>(true);
        // The search is restricted to the player hierarchy, never the entire scene.
        for (int i = 0; i < list.Length; ++i)
            if (list[i] != null) _renderers.Add(list[i]);
    }
    internal int Apply(int cameraMask)
    {
        int mask = cameraMask;
        int active = 0, excluded = 0, forced = 0, shadowsOnly = 0;
        foreach (Renderer renderer in _renderers)
        {
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
            ++active;
            int bit = 1 << renderer.gameObject.layer;
            if ((cameraMask & bit) == 0) ++excluded;
            mask |= bit;
            bool off = renderer.forceRenderingOff;
            ShadowCastingMode shadows = renderer.shadowCastingMode;
            if (off || shadows == ShadowCastingMode.ShadowsOnly)
            {
                _saved.Add(new Saved { Renderer = renderer, ForceOff = off, Shadows = shadows });
                if (off) { renderer.forceRenderingOff = false; ++forced; }
                if (shadows == ShadowCastingMode.ShadowsOnly)
                { renderer.shadowCastingMode = ShadowCastingMode.On; ++shadowsOnly; }
            }
            if (!_reported && active <= 32)
                _log($"Player renderer: name={renderer.name}, layer={renderer.gameObject.layer}, excluded={(cameraMask & bit) == 0}, forceOff={off}, shadows={shadows}");
        }
        if (!_reported)
        {
            _log($"Visibility: activeRenderers={active}, layerExcluded={excluded}, forceOff={forced}, shadowsOnly={shadowsOnly}, oldMask={cameraMask}, renderMask={mask}");
            _reported = true;
        }
        return mask;
    }
    internal void Restore()
    {
        foreach (Saved state in _saved)
        {
            try
            {
                if (state.Renderer == null) continue;
                state.Renderer.forceRenderingOff = state.ForceOff;
                state.Renderer.shadowCastingMode = state.Shadows;
            }
            catch (Exception e) { _log($"Renderer restore failed: {e.Message}"); }
        }
        _saved.Clear();
    }
}
