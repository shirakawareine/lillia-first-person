using System;
using UnityEngine;
using Character = ProjectTarkovH.CharacterController;
using Orientation = ProjectTarkovH.OrientationMethod;

namespace FirstPersonTest;

// Facing and translation are independent. Native SetInputs still calculates
// camera-relative _moveInputVector and all animation/stamina parameters.
internal sealed class BodyFollow
{
    private readonly Action<string> _log;
    private Character? _controller;
    private GameObject? _root;
    private bool _active, _observed;
    private double _nextSearch, _nextReport;
    private Vector3 _lastMoveDirection;
    internal bool CanFollowHead => _controller != null && _controller.isActiveAndEnabled
        && !_controller.DoingSex && _controller._movementEnabled;
    internal BodyFollow(Action<string> log) => _log = log;

    internal void Tick(GameObject? root, bool enabled)
    {
        if (root != _root)
        {
            Restore();
            _controller = null; _root = root; _nextSearch = 0;
        }
        _active = enabled && root != null && root.activeInHierarchy && Time.timeScale > 0;
        if (!_active) { _lastMoveDirection = Vector3.zero; return; }
        if (_controller == null)
        {
            double now = Environment.TickCount64 / 1000.0;
            if (now < _nextSearch) return;
            _nextSearch = now + 5;
            _controller = root!.GetComponentInParent<Character>();
            if (_controller == null) _controller = root.GetComponentInChildren<Character>();
            if (_controller != null)
                _log($"Body follow: bound controller={_controller.name}; unified facing with camera-relative root motion.");
        }
    }

    internal bool Owns(Character controller) => _active && controller == _controller
        && CanFollowHead && !controller.IsBucketMotionActive && Time.timeScale > 0;

    // Scope the orientation setting to SetInputs. UpdateRotation consumes the
    // resulting _lookInputVector, so no persistent mode override is necessary.
    internal Orientation? BeforeInputs(Character controller)
    {
        if (!Owns(controller)) return null;
        Orientation original = controller.OrientationMethod;
        if (!_observed)
        {
            _observed = true;
            _log("Body follow: SetInputs observed; WASD changes translation, not facing.");
        }
        controller.OrientationMethod = Orientation.TowardsCamera;
        return original;
    }

    internal static void AfterInputs(Character controller, Orientation? original)
    {
        if (original.HasValue && controller != null
            && controller.OrientationMethod == Orientation.TowardsCamera)
            controller.OrientationMethod = original.Value;
    }

    internal void BeforeVelocity(Character controller, float deltaTime)
    {
        if (!Owns(controller) || !float.IsFinite(deltaTime) || deltaTime <= 0) return;
        var motor = controller.Motor;
        if (motor == null || !motor.isActiveAndEnabled || !motor.GroundingStatus.IsStableOnGround
            || controller._suppressRootMotionUntilFreshMotion) return;
        Vector3 move = controller._moveInputVector;
        if (RootMotionDirection.HasDirection(move)) _lastMoveDirection = move;
        Vector3 pending = controller._pendingRootMotionLocalDelta;
        Vector3 redirected = RootMotionDirection.Redirect(pending, motor.TransientRotation, _lastMoveDirection);
        // Native UpdateVelocity immediately consumes and clears this field, then
        // handles slopes, contacts and external velocity. Never rotate total velocity.
        controller._pendingRootMotionLocalDelta = redirected;
        double now = Environment.TickCount64 / 1000.0;
        if (now >= _nextReport && pending.sqrMagnitude > .00000001f)
        {
            _nextReport = now + 5;
            _log($"Locomotion: input={move}, rootLocal={pending}, redirected={redirected}, look={controller._lookInputVector}");
        }
    }

    internal void Restore() { _active = false; _lastMoveDirection = Vector3.zero; }
}
