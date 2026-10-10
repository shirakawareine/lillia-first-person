using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;
using Character = ProjectTarkovH.CharacterController;
using Director = ProjectTarkovH.PlayerDirector;

namespace FirstPersonTest;

// Read game state; never writes character root, motor, animation playback or game inputs.
internal sealed class EventView
{
    private readonly Action<string> _log;
    private readonly EventLookAngles _angles = new();
    private readonly Dictionary<string, (float yaw, float up, float down)> _limits;
    private readonly float _sensitivity;
    private readonly bool _invert;
    private Character? _controller;
    private Director? _director;
    private GameObject? _root;
    private double _nextSearch;
    private string _clip = "", _profile = "";
    private readonly (float yaw, float up, float down) _defaults;
    internal bool Active { get; private set; }
    internal bool InputBlocked => _director != null &&
        (_director.IsSystemMenuOpen || _director.menuOverrideActive || _director.DoingConversation || _director.isFadeInProgress);

    internal EventView(ConfigFile config, Action<string> log)
    {
        _log = log;
        _defaults = (
            Limit(config.Bind("EventView", "DefaultYaw", 75f, "Default left/right offset in degrees; 0..89.").Value, 75f, 89f),
            Limit(config.Bind("View", "LookUpDegrees", 75f).Value, 75f, 89f),
            Limit(config.Bind("View", "LookDownDegrees", 85f).Value, 85f, 89f));
        _limits = EventLookAngles.Parse(config.Bind("EventView", "ClipLimits", "",
            "Exact animation clip limits: ClipName|yaw|up|down;OtherClip|yaw|up|down. Degrees: yaw 0..89, pitch 0..89. 0|0|0 locks to animation. Restart after editing.").Value, log);
        float s = config.Bind("EventView", "Sensitivity", 2f, "Free-look mouse sensitivity; 0.1..10. No right button required.").Value;
        _sensitivity = Math.Clamp(float.IsFinite(s) ? s : 2f, .1f, 10f);
        _invert = config.Bind("View", "InvertY", false).Value;
    }

    internal void Tick(GameObject? root, bool enabled, bool foreground)
    {
        if (root != _root) { Reset(); _root = root; _controller = null; _director = null; _nextSearch = 0; }
        if (!enabled || root == null || !root.activeInHierarchy) { Reset(); return; }
        double now = Environment.TickCount64 / 1000.0;
        if ((_controller == null || _director == null) && now >= _nextSearch)
        {
            _nextSearch = now + 2;
            _controller = root.GetComponentInParent<Character>();
            if (_controller == null) _controller = root.GetComponentInChildren<Character>();
            var director = UnityEngine.Object.FindObjectOfType<Director>();
            if (director != null && director.Character == _controller) _director = director;
        }
        Refresh();
        if (Active && foreground && !InputBlocked && Time.timeScale > 0)
            _angles.Add(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"), _sensitivity, _invert);
    }

    internal void Refresh()
    {
        bool active = _controller != null && _controller.isActiveAndEnabled &&
            (_controller.DoingSex || (_director != null && _director.IsSexSessionActive));
        if (!active) { if (Active) _log("Event view: ended; returning to normal camera."); Reset(); return; }
        string clip = "unknown";
        var motion = _controller!.sexMotionPlayer;
        if (motion != null && motion.CurrentMotionClip != null) clip = motion.CurrentMotionClip.name;
        if (!Active || clip != _clip)
        {
            _angles.Reset(); _profile = "";
            _log("Event view: clip=" + clip + "; animated-head relative view; body untouched.");
        }
        Active = true; _clip = clip;
    }

    // Called after HeadLook.Restore, so limits and basis use the unmodified animated pose.
    internal Quaternion Rotation(Transform head)
    {
        var limits = _defaults;
        string profile = "default";
        if (_limits.TryGetValue(_clip, out var custom)) { limits = custom; profile = "clip override"; }
        // Do not allow accumulated input beyond a bound to delay reversing direction.
        _angles.Clamp(limits.yaw, limits.up, limits.down);
        if (profile != _profile)
        {
            _profile = profile;
            _log($"Event view: clip={_clip}, profile={profile}, yaw={limits.yaw}, up={limits.up}, down={limits.down}");
        }
        return head.rotation * Quaternion.Euler(_angles.Pitch, _angles.Yaw, 0);
    }

    private static float Limit(float value, float fallback, float max) =>
        Math.Clamp(float.IsFinite(value) ? value : fallback, 0f, max);

    internal void Reset() { Active = false; _angles.Reset(); _clip = _profile = ""; }
}
