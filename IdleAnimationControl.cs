using System;
using System.Text;
using UnityEngine;

namespace FirstPersonTest;

internal sealed class IdleAnimationControl
{
    private readonly Action<string> _log;
    private Animator? _animator;
    private RuntimeAnimatorController? _controller;
    private int _neutral;
    private int _walkLayer = -1;
    private bool _hasRandomPose;
    private double _next, _lastFade;
    private bool _failed, _warned;
    private double _nextTrace;
    private int _traceCount;
    internal IdleAnimationControl(Action<string> log) => _log = log;
    internal void Reset() { _animator = null; _controller = null; _neutral = 0; _walkLayer = -1; _hasRandomPose = false; _next = 0; _lastFade = double.NegativeInfinity; _warned = false; }

    internal void Tick(Animator? animator, bool allowed, bool controlling, double now)
    {
        Trace(animator, allowed, controlling, now);
        if (_failed || !allowed || animator == null || !animator.isActiveAndEnabled) return;
        try
        {
            var controller = animator.runtimeAnimatorController;
            if (_animator != animator || _controller != controller)
            {
                Reset(); _animator = animator; _controller = controller;
                _walkLayer = animator.GetLayerIndex("Walk");
                foreach (var parameter in animator.parameters)
                    if (parameter.name == "ramdomPose" && parameter.type == AnimatorControllerParameterType.Bool)
                        _hasRandomPose = true;
                // Observed in the user's 1.1 runtime log, not derived from a clip name.
                // Still validate against the live controller before using it.
                if (controller != null && controller.name == "MainChar_Animator" && _walkLayer >= 0
                    && animator.HasState(_walkLayer, -1793020935)) _neutral = -1793020935;
                _log($"Idle control: bound Walk layer={_walkLayer}, ramdomPose={_hasRandomPose}, verifiedNeutral={_neutral}.");
            }
            if (controller == null || now < _next) return;
            _next = now + .1;
            if (_walkLayer < 0 || animator.GetLayerWeight(_walkLayer) <= .001f) return;
            if (controlling && _hasRandomPose && animator.GetBool("ramdomPose"))
            {
                animator.SetBool("ramdomPose", false);
                _log("Idle control: cancelled ramdomPose on player input.");
            }
            var clips = animator.GetCurrentAnimatorClipInfo(_walkLayer);
            bool neutral = false, gesture = false;
            foreach (var info in clips)
            {
                if (info.clip == null || info.weight < .5f) continue;
                string name = info.clip.name;
                neutral |= name == "NormalIdle";
                gesture |= IsGesture(name);
            }
            bool transition = animator.IsInTransition(_walkLayer);
            if (neutral && !transition)
            {
                int state = animator.GetCurrentAnimatorStateInfo(_walkLayer).fullPathHash;
                if (_neutral != state) { _neutral = state; _log($"Idle control: learned NormalIdle state={state}."); }
            }
            if (transition)
            {
                bool nextGesture = false;
                foreach (var info in animator.GetNextAnimatorClipInfo(_walkLayer))
                    if (info.clip != null && IsGesture(info.clip.name)) nextGesture = true;
                if (!nextGesture) return; // Already leaving the gesture; don't restart the blend.
                gesture = true;
            }
            if (!controlling || !gesture || now - _lastFade < .5) return;
            if (_neutral == 0 || !animator.HasState(_walkLayer, _neutral))
            {
                if (!_warned) { _warned = true; _log("Idle control: waiting to observe NormalIdle; no guessed state transition."); }
                return;
            }
            animator.CrossFadeInFixedTime(_neutral, .25f, _walkLayer, 0f, 0f);
            _lastFade = now;
            _log("Idle control: observation/movement interrupted an idle gesture with a 0.25s transition.");
        }
        catch (Exception e) { _failed = true; _log($"Idle animation control unavailable; view remains active: {e}"); }
    }

    private static bool IsGesture(string name) => name == "KA_Idle02_LookLeftAndRight"
        || name == "KA_Idle03_LookAtHands" || name == "KA_Idle04_LookAtFeet"
        || name == "KA_Idle12_LeaningForward" || name == "KA_Idle40_CrossLegs";

    // Read-only evidence for the actual runtime graph; do not infer state names
    // or parameter semantics from the clip inventory.
    private void Trace(Animator? animator, bool allowed, bool controlling, double now)
    {
        if (animator == null || now < _nextTrace || _traceCount >= 24) return;
        _nextTrace = now + 5;
        ++_traceCount;
        var text = new StringBuilder($"Idle graph: sample={_traceCount}, frame={Time.frameCount}, allowed={allowed}, controlling={controlling}, failed={_failed}");
        try
        {
            text.Append($", animator={animator.name}, active={animator.isActiveAndEnabled}, layers={animator.layerCount}, neutral={_neutral}");
            for (int layer = 0; layer < Math.Min(animator.layerCount, 16); ++layer)
            {
                try
                {
                    var state = animator.GetCurrentAnimatorStateInfo(layer);
                    text.Append($"\n layer={layer}:{animator.GetLayerName(layer)}, weight={animator.GetLayerWeight(layer):F3}, state={state.fullPathHash}, short={state.shortNameHash}, time={state.normalizedTime:F3}, transition={animator.IsInTransition(layer)}; current=");
                    foreach (var clip in animator.GetCurrentAnimatorClipInfo(layer))
                        if (clip.clip != null) text.Append($"[{clip.clip.name}|{clip.weight:F3}]");
                    if (animator.IsInTransition(layer))
                    {
                        var next = animator.GetNextAnimatorStateInfo(layer);
                        text.Append($"; nextState={next.fullPathHash}; next=");
                        foreach (var clip in animator.GetNextAnimatorClipInfo(layer))
                            if (clip.clip != null) text.Append($"[{clip.clip.name}|{clip.weight:F3}]");
                    }
                }
                catch (Exception e) { text.Append($" layer {layer} read failed: {e.Message}"); }
            }
            try
            {
                var parameters = animator.parameters;
                text.Append("\n parameters=");
                for (int i = 0; i < Math.Min(parameters.Length, 64); ++i)
                {
                    var p = parameters[i];
                    string value = p.type == AnimatorControllerParameterType.Bool ? animator.GetBool(p.nameHash).ToString()
                        : p.type == AnimatorControllerParameterType.Int ? animator.GetInteger(p.nameHash).ToString()
                        : p.type == AnimatorControllerParameterType.Float ? animator.GetFloat(p.nameHash).ToString("F3") : "trigger-unread";
                    text.Append($"[{p.name}:{p.type}={value}]");
                }
            }
            catch (Exception e) { text.Append(" parameters read failed: " + e.Message); }
        }
        catch (Exception e) { text.Append(" graph read failed: " + e.Message); }
        _log(text.ToString());
    }
}
