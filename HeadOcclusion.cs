using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace FirstPersonTest;

// Camera-scoped material substitution. No bone or original material edits.
internal sealed class HeadOcclusion
{
    private readonly Action<string> _log;
    private readonly HashSet<string> _explicitSlots;
    private readonly List<Plan> _plans = new();
    private readonly List<Saved> _saved = new();
    private readonly HashSet<string> _reported = new();
    private GameObject? _root;
    private Material? _invisible;
    private MaterialPropertyBlock? _blank;
    private double _nextRefresh;
    private bool _failed;
    private bool _hideHead = true, _hideHair;
    internal void SetMode(bool hideHead, bool hideHair)
    {
        if (_hideHead == hideHead && _hideHair == hideHair) return;
        Restore();
        _hideHead = hideHead;
        _hideHair = hideHair;
        _nextRefresh = 0;
    }
    internal static bool IsHairMaterial(string name)
    {
        string words = Regex.Replace(name, "([a-z])([A-Z])", "$1_$2");
        foreach (string token in Regex.Split(words.ToLowerInvariant(), @"[^a-z]+"))
            if (token == "hair") return true;
        return false;
    }
    private sealed class Plan
    {
        internal Renderer Renderer = null!;
        internal Il2CppReferenceArray<Material> Original = null!, Replacement = null!;
        internal readonly List<(int Index, MaterialPropertyBlock Block)> Blocks = new();
    }
    private sealed class Saved { internal Plan Plan = null!; }
    internal HeadOcclusion(Action<string> log, string explicitSlots)
    {
        _log = log;
        _explicitSlots = new HashSet<string>(explicitSlots.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), StringComparer.OrdinalIgnoreCase);
    }
    internal static bool IsHeadMaterial(string name)
    {
        string words = Regex.Replace(name, "([a-z])([A-Z])", "$1_$2");
        string[] tokens = Regex.Split(words.ToLowerInvariant(), @"[^a-z]+");
        foreach (string token in tokens)
            if (token is "body" or "skin" or "hair" or "headband") return false;
        foreach (string token in tokens)
            if (token is "face" or "head" or "eye" or "eyes" or "eyelash" or "eyelashes"
                or "eyebrow" or "eyebrows" or "mouth" or "teeth" or "tooth" or "tongue"
                or "iris" or "pupil" or "sclera" or "eyeline") return true;
        return name.Contains("顔") || name.Contains("口内") || name.Contains("まつげ") || name.Contains("眉毛");
    }
    internal void Refresh(GameObject? root)
    {
        double now = Environment.TickCount64 / 1000.0;
        if (_failed || (root == _root && now < _nextRefresh)) return;
        _nextRefresh = now + 5;
        if (root != _root) _reported.Clear();
        _root = root;
        _plans.Clear();
        if (root == null) return;
        try
        {
            int selected = 0;
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int r = 0; r < renderers.Length; ++r)
            {
                Renderer renderer = renderers[r];
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                var original = renderer.sharedMaterials;
                var indices = new List<int>();
                for (int i = 0; i < original.Length; ++i)
                {
                    Material material = original[i];
                    string name = material != null ? material.name : "<null>";
                    bool hide = (_hideHead && (_explicitSlots.Count > 0 ? _explicitSlots.Contains($"{renderer.name}:{i}") : IsHeadMaterial(name)))
                        || (_hideHair && IsHairMaterial(name));
                    string record = $"Head slot: renderer={renderer.name}, id={renderer.GetInstanceID()}, slot={i}, material={name}, shader={(material != null && material.shader != null ? material.shader.name : "<null>")}, selected={hide}";
                    if (_reported.Add(record)) _log(record);
                    if (hide && material != null) indices.Add(i);
                }
                if (indices.Count == 0) continue;
                PrepareMaterial();
                var replacement = new Il2CppReferenceArray<Material>(original.Length);
                for (int i = 0; i < original.Length; ++i) replacement[i] = original[i];
                var plan = new Plan { Renderer = renderer, Original = original, Replacement = replacement };
                foreach (int index in indices)
                {
                    replacement[index] = _invisible!;
                    plan.Blocks.Add((index, new MaterialPropertyBlock()));
                }
                _plans.Add(plan);
                selected += indices.Count;
            }
            string summary = $"Head mask: selectedSlots={selected}; hideHair={_hideHair}; boneScaleUnchanged=true; mode={(_explicitSlots.Count > 0 ? "explicit" : "material-name")}";
            if (_reported.Add(summary)) _log(summary);
        }
        catch (Exception e) { Fail(e); }
    }
    private void PrepareMaterial()
    {
        if (_invisible != null) return;
        Shader shader = Shader.Find("UI/Default");
        if (shader == null || !shader.isSupported) throw new InvalidOperationException("UI/Default unavailable; original materials retained.");
        _invisible = new Material(shader) { name = "FirstPersonTest_NoColorNoDepth" };
        // Built-in UI/Default: ZWrite Off, ColorMask [_ColorMask].
        if (!_invisible.HasProperty("_ColorMask") || !_invisible.HasProperty("_StencilComp"))
            throw new InvalidOperationException("Invisible-material properties unavailable.");
        _invisible.SetFloat("_ColorMask", 0);
        _invisible.SetFloat("_StencilComp", 1); // CompareFunction.Never
        _invisible.SetFloat("_StencilWriteMask", 0);
        _blank = new MaterialPropertyBlock();
        _blank.SetFloat("_ColorMask", 0);
        _blank.SetFloat("_StencilComp", 1);
        _blank.SetFloat("_StencilWriteMask", 0);
    }
    internal void Apply()
    {
        if (_failed || _saved.Count != 0) return;
        try
        {
            foreach (Plan plan in _plans)
            {
                Renderer renderer = plan.Renderer;
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                // Validate cached identity; wardrobe changes are never overwritten with stale slots.
                var current = renderer.sharedMaterials;
                bool matches = current.Length == plan.Original.Length;
                for (int i = 0; matches && i < current.Length; ++i) matches = current[i] == plan.Original[i];
                if (!matches) { _nextRefresh = 0; continue; }
                foreach (var item in plan.Blocks) renderer.GetPropertyBlock(item.Block, item.Index);
                _saved.Add(new Saved { Plan = plan });
                renderer.sharedMaterials = plan.Replacement;
                foreach (var item in plan.Blocks) renderer.SetPropertyBlock(_blank, item.Index);
            }
        }
        catch (Exception e) { Restore(); Fail(e); }
    }
    private void Fail(Exception e)
    {
        _failed = true;
        _log($"Head mask disabled after error; camera stays active: {e}");
    }
    internal void Restore()
    {
        foreach (Saved state in _saved)
        {
            Plan plan = state.Plan;
            if (plan.Renderer == null) continue;
            try { plan.Renderer.sharedMaterials = plan.Original; }
            catch (Exception e) { _log($"Head materials restore failed: {e.Message}"); }
            foreach (var item in plan.Blocks)
            {
                try { plan.Renderer.SetPropertyBlock(item.Block.isEmpty ? null : item.Block, item.Index); }
                catch (Exception e) { _log($"Head property block restore failed: {e.Message}"); }
            }
        }
        _saved.Clear();
    }
    internal void Dispose()
    {
        Restore();
        if (_invisible != null) UnityEngine.Object.Destroy(_invisible);
        _invisible = null;
        _plans.Clear();
    }
}
