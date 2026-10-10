using System;
using System.Collections.Generic;
using UnityEngine;

namespace FirstPersonTest;

// Read-only snapshots. All Unity reads remain on the game thread; only strings
// are queued to the existing background file logger. No camera is selected here.
internal sealed class CameraDiagnostics
{
    private readonly Action<string> _write;
    private readonly HashSet<int> _rendered = new();
    private double _nextSnapshot;
    private bool _sampling;
    internal CameraDiagnostics(Action<string> write) => _write = write;

    internal void Poll(Transform? head, Camera? target, bool enabled)
    {
        double now = Environment.TickCount64 / 1000.0;
        if (now < _nextSnapshot) return;
        _nextSnapshot = now + 10.0;
        _rendered.Clear();
        _sampling = true;
        try
        {
            var cameras = Camera.allCameras;
            _write($"Snapshot: enabled={enabled}, activeCameras={cameras.Length}, targetId={(target != null ? target.GetInstanceID() : 0)}, headPath={(head != null ? Path(head) : "missing")}, eye={Eye(head)}");
            for (int i = 0; i < Math.Min(cameras.Length, 12); ++i)
                Record(cameras[i], "Update", head, target, enabled);
        }
        catch (Exception e) { _write($"Snapshot unavailable (core unchanged): {e}"); }
    }

    internal void AfterRender(Camera camera, Transform? head, Camera? target, bool enabled, Vector3? eyeOverride = null)
    {
        if (!_sampling || _rendered.Count >= 12) return;
        try
        {
            if (camera == null || !_rendered.Add(camera.GetInstanceID())) return;
            Record(camera, "PostRender", head, target, enabled, eyeOverride);
        }
        catch (Exception e) { _write($"PostRender snapshot unavailable (core unchanged): {e}"); }
    }

    private void Record(Camera camera, string phase, Transform? head, Camera? target, bool enabled, Vector3? eyeOverride = null)
    {
        if (camera == null) return;
        try
        {
            var texture = camera.targetTexture;
            string output = texture == null ? "screen" : $"texture:{texture.name}#{texture.GetInstanceID()}({texture.width}x{texture.height})";
            var matrix = camera.cameraToWorldMatrix;
            var matrixOrigin = new Vector3(matrix.m03, matrix.m13, matrix.m23);
            Vector3 position = camera.transform.position;
            string eye = eyeOverride.HasValue ? V(eyeOverride.Value) : Eye(head);
            _write($"Camera[{phase}]: enabled={enabled}, selected={camera == target}, id={camera.GetInstanceID()}, path={Path(camera.transform)}, scene={camera.gameObject.scene.name}, output={output}, display={camera.targetDisplay}, depth={camera.depth}, rect={camera.rect}, pos={V(position)}, viewOrigin={V(matrixOrigin)}, viewTransformGap={(matrixOrigin - position).magnitude:F4}, eye={eye}");
        }
        catch (Exception e) { _write($"Camera[{phase}] snapshot failed (core unchanged): {e.GetType().Name}: {e.Message}"); }
    }

    private static string Eye(Transform? head) => head != null
        ? V(head.TransformPoint(new Vector3(0f, 0.0455f, 0.0331f))) : "missing";
    private static string V(Vector3 value) => $"({value.x:F4},{value.y:F4},{value.z:F4})";
    private static string Path(Transform transform)
    {
        string result = transform.name;
        for (int i = 0; i < 12; ++i)
        {
            transform = transform.parent;
            if (transform == null) return result;
            result = transform.name + "/" + result;
        }
        return ".../" + result;
    }
}
