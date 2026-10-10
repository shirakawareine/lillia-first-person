using System;
using UnityEngine;

namespace FirstPersonTest;

// Read-only, one sampled frame per second, at most ten records per frame.
// All native reads stay on the game thread; logging uses the bounded file queue.
internal sealed class PoseDiagnostics
{
    private double _next;
    private int _frame = -1, _count;
    private bool _failed;

    internal void Record(string phase, Camera? camera, Camera? screen, GameObject? player,
        Transform? head, bool enabled, bool eventActive, Action<string> write)
    {
        if (_failed) return;
        try
        {
            int frame = Time.frameCount;
            if (phase == "UpdateBeforeRestore" && Environment.TickCount64 / 1000.0 >= _next)
            {
                _next = Environment.TickCount64 / 1000.0 + 1.0;
                _frame = frame;
                _count = 0;
            }
            if (frame != _frame || _count >= 10 || head == null || player == null) return;
            if (camera != null && camera.name != "MainCharCam" && camera != screen) return;
            ++_count;
            Transform body = player.transform;
            Transform neck = head.parent;
            var controller = player.GetComponentInParent<ProjectTarkovH.CharacterController>();
            Vector3 headAngles = head.eulerAngles;
            string input = controller != null
                ? $"controllerEuler={controller.transform.eulerAngles}, meshEuler={(controller.MeshRoot != null ? controller.MeshRoot.eulerAngles : Vector3.zero)}, move={controller._moveInputVector}, look={controller._lookInputVector}, mode={controller.OrientationMethod}"
                : "controller=missing";
            string view = screen != null
                ? $"screenEuler={screen.transform.eulerAngles}, headScreenAngle={Quaternion.Angle(head.rotation, screen.transform.rotation):F2}"
                : "screen=missing";
            write($"Pose: frame={frame}, seq={_count}, phase={phase}, camera={(camera != null ? camera.name : "none")}, enabled={enabled}, event={eventActive}, bodyEuler={body.eulerAngles}, neckEuler={(neck != null ? neck.eulerAngles : Vector3.zero)}, headEuler={headAngles}, headLocalEuler={head.localEulerAngles}, headBodyYaw={Mathf.DeltaAngle(body.eulerAngles.y, headAngles.y):F2}, {view}, {input}");
        }
        catch (Exception e)
        {
            _failed = true;
            write($"Pose diagnostics disabled (view unchanged): {e.GetType().Name}: {e.Message}");
        }
    }
}
