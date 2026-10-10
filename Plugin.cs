using System;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FirstPersonTest;

[BepInPlugin("local.lillia.firstpersontest", "First Person Test", "0.1.28")]
public sealed class Plugin : BasePlugin
{
    internal static Plugin? Instance;
    private static readonly Vector3 EyeOffset = new(0f, 0.0455f, 0.0331f);
    private float _eyeHeightAdjustment;
    private bool _hideHair, _hairHeld;
    private DiagnosticLog? _log;
    private CameraDiagnostics? _diagnostics;
    private readonly PoseDiagnostics _poseDiagnostics = new();
    private Driver? _driver;
    private Camera.CameraCallback? _preCull, _postRender;
    private bool _preInstalled, _postInstalled, _enabled = true, _held, _stopped;
    private GameObject? _player;
    private Animator? _animator;
    private Transform? _head;
    private Camera? _camera, _restoreCamera;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private float _originalNearClip, _nearClip;
    private int _originalMask;
    private PitchController _pitch = new(75f, 85f, 3f, false);
    private PlayerVisibility? _visibility;
    private HeadOcclusion? _headOcclusion;
    private BodyFollow? _bodyFollow;
    private MovementInputHook? _movementHook;
    private EventView? _eventView;
    private WallCutoutSuppression? _wallCutout;
    private bool _wasEvent;
    private PosePhase? _posePhase;
    private int _poseFrame = -1;
    private double _nextPoseReport;
    private readonly HeadLook _headLook = new();
    private readonly IdleViewBlend _idleView = new();
    private readonly IdleMotionSmoothing _idleMotion = new();
    private IdleAnimationControl? _idleAnimation;
    private double _lastMove = double.NegativeInfinity;
    private bool _followHead, _foreground;
    // Exact head/view alignment replaces the old weighted additive mode.
    private bool _followBody;
    private bool _hideHead;
    private Vector3 _renderEye;
    private bool _pitchInputAvailable = true, _showBody;
    private bool _restorePending;
    private int _scene = int.MinValue;
    private double _nextSearch, _nextReport, _nextWarning, _retryAfter;
    private long _ticks, _writes, _restores;
    private string _binding = "waiting";
    private static double Now => Environment.TickCount64 / 1000.0;

    public override void Load()
    {
        Instance = this;
        try
        {
            _log = new DiagnosticLog(System.IO.Path.Combine(Paths.BepInExRootPath, "FirstPersonTest.log"));
            _idleAnimation = new IdleAnimationControl(Info);
            _pitch = new PitchController(
                Config.Bind("View", "LookUpDegrees", 75f, "Maximum upward pitch; 0 to 89 degrees.").Value,
                Config.Bind("View", "LookDownDegrees", 85f, "Maximum downward pitch; 0 to 89 degrees.").Value,
                Config.Bind("View", "PitchSensitivity", 3f, "Degrees per Mouse Y axis unit; restart after editing.").Value,
                Config.Bind("View", "InvertY", false, "Invert vertical mouse movement.").Value);
            _wallCutout = new WallCutoutSuppression(Info);
            _eventView = new EventView(Config, Info); // First-person mouse look no longer requires right click.
            float height = Config.Bind("View", "EyeHeightAdjustment", -0.01f, "Local head-space vertical eye adjustment; -0.03..0.03. Negative lowers the view; 0 restores 0.1.10.").Value;
            _eyeHeightAdjustment = Math.Clamp(float.IsFinite(height) ? height : -0.01f, -0.03f, 0.03f);
            _hideHair = Config.Bind("Body", "HideHairOnStart", false, "Initial hair visibility; F9 toggles for the current session in first person.").Value;
            _followBody = Config.Bind("Body", "FollowCameraYaw", true, "Face the camera while WASD translates independently using native root-motion stride.").Value;
            _bodyFollow = new BodyFollow(Info);
            _movementHook = new MovementInputHook();
            try { _movementHook.InstallIdleHook(); }
            catch (Exception e) { Info($"Idle random hook unavailable: {e}"); }
            Info("Movement hooks installed: camera facing and root-motion direction are paired.");
            _followHead = Config.Bind("Body", "FollowHeadPitch", true, "Align Head world orientation to the camera view; restore before the next animation frame.").Value;
            _showBody = Config.Bind("Body", "ShowPlayerRenderers", true, "Include active enabled player renderer layers and temporarily undo force-off/shadows-only.").Value;
            _hideHead = Config.Bind("Body", "HideHeadInFirstPerson", true, "Hide recognized head material slots only during screen-camera rendering; never change bones.").Value;
            float clip = Config.Bind("Body", "NearClip", 0.02f, "First-person near clip in world units, clamped to 0.005..0.1; never increases original near clip.").Value;
            _nearClip = Math.Clamp(float.IsFinite(clip) ? clip : 0.02f, 0.005f, 0.1f);
            _headOcclusion = new HeadOcclusion(Info, Config.Bind("Body", "HeadMaterialSlots", "", "Optional exact RendererName:index pairs separated by semicolons. Empty uses conservative head material name matching; restart required.").Value);
            _visibility = new PlayerVisibility(Info);
            _diagnostics = new CameraDiagnostics(Info);
            _preCull = DelegateSupport.ConvertDelegate<Camera.CameraCallback>(new Action<Camera>(BeforeRender));
            _postRender = DelegateSupport.ConvertDelegate<Camera.CameraCallback>(new Action<Camera>(AfterRender));
            // Both hooks have actually fired in this game's 0.1.3 logs.
            // Install restoration first; never leave a pre-render write without cleanup.
            Camera.onPostRender = Il2CppSystem.Delegate.Combine(Camera.onPostRender, _postRender).Cast<Camera.CameraCallback>();
            _postInstalled = true;
            Camera.onPreCull = Il2CppSystem.Delegate.Combine(Camera.onPreCull, _preCull).Cast<Camera.CameraCallback>();
            _preInstalled = true;
            _driver = AddComponent<Driver>();
            _posePhase = new PosePhase();
            Info($"Loaded v0.1.28. ON, F8 toggle, F9 hair. EyeHeightAdjustment={_eyeHeightAdjustment}, HideHair={_hideHair}, FollowCameraYaw={_followBody}, FollowHeadPitch={_followHead}. Single pose before hair physics; camera callbacks never write bones.");
        }
        catch
        {
            Stop();
            if (_driver != null) UnityEngine.Object.Destroy(_driver);
            throw;
        }
    }

    internal void Tick()
    {
        if (_stopped) return;
        try
        {
            ++_ticks;
            RecordPose("UpdateBeforeRestore");
            _headLook.Restore();
            // Fallback cleanup if a render was aborted without PostRender.
            RestoreRenderState();
            bool held = (GetAsyncKeyState(0x77) & 0x8000) != 0;
            GetWindowThreadProcessId(GetForegroundWindow(), out uint foreground);
            _foreground = foreground == (uint)Environment.ProcessId;
            if (held && !_held && foreground == (uint)Environment.ProcessId)
            {
                _enabled = !_enabled;
                Info(_enabled ? "F8: First person ON." : "F8: First person OFF.");
                _nextSearch = _nextReport = 0;
                _pitch.Invalidate();
                _idleView.Reset(); _idleMotion.Reset();
            }
            _held = held;
            bool hairHeld = (GetAsyncKeyState(0x78) & 0x8000) != 0;
            if (hairHeld && !_hairHeld && _foreground && _enabled)
            {
                _hideHair = !_hideHair;
                Info(_hideHair ? "F9: Hair hidden in first person." : "F9: Hair visible in first person.");
            }
            _hairHeld = hairHeld;
            CheckScene();
            if (_enabled && Now >= _retryAfter && !HasHead() && Now >= _nextSearch)
            {
                _nextSearch = Now + 1.0;
                if (_player == null || !_player.activeInHierarchy)
                    _player = GameObject.Find("Ingame_heroine_prefab(Clone)");
                _animator = _player != null ? _player.GetComponent<Animator>() : null;
                _head = _animator != null && _animator.isHuman
                    ? _animator.GetBoneTransform(HumanBodyBones.Head) : null;
                _binding = _player == null ? "player missing" : _animator == null ? "Animator missing"
                    : !_animator.isHuman ? "Animator not human" : _head == null ? "Head missing"
                    : !_head.gameObject.activeInHierarchy ? "Head inactive" : "Head bound";
                if (HasHead()) Info($"Bound head={_head!.GetInstanceID()}, name={_head.name}");
            }
            _wallCutout?.Tick(_camera, _enabled && HasHead());
            _eventView?.Tick(_player, _enabled && HasHead(), _foreground);
            bool eventActive = _eventView?.Active == true;
            if (_wasEvent != eventActive) { _pitch.Invalidate(); _headLook.Restore(); _idleView.Reset(); _idleMotion.Reset(); }
            _wasEvent = eventActive;
            try { _bodyFollow?.Tick(_player, _enabled && !eventActive && _eventView?.InputBlocked != true && _followBody && HasHead() && foreground == (uint)Environment.ProcessId); }
            catch (Exception e)
            {
                DisableAfterControlError(e);
            }
            if (_enabled && HasHead())
            {
                if (_showBody)
                {
                    try { _visibility?.Refresh(_player); }
                    catch (Exception e) { _showBody = false; Info($"Body override unavailable; first-person camera remains active: {e}"); }
                }
                _headOcclusion?.SetMode(_hideHead, _hideHair);
                if (_hideHead || _hideHair) _headOcclusion?.Refresh(_player);
                ReadPitchInput(foreground == (uint)Environment.ProcessId);
                _idleAnimation?.Tick(_animator, NormalIdleAllowed(), _idleView.Controlling(Now), Now);
            }
            _diagnostics?.Poll(_head, _camera, _enabled);
            if (Now >= _nextReport)
            {
                _nextReport = Now + 5.0;
                Info($"State: enabled={_enabled}, ticks={_ticks}, binding={_binding}, screenWrites={_writes}, restores={_restores}, pitch={_pitch.Angle:F2}, idleWeight={_idleView.Weight:F2}, inputAvailable={_pitchInputAvailable}, targetId={(_camera != null ? _camera.GetInstanceID() : 0)}, timeScale={Time.timeScale}");
            }
        }
        catch (Exception e) { Fault(e); }
    }

    private void ReadPitchInput(bool foreground)
    {
        if (_eventView?.Active == true || _eventView?.InputBlocked == true || !foreground || !_pitchInputAvailable || !_pitch.Initialized || Time.timeScale <= 0) return;
        try
        {
            // Cursor remains owned by the game; no right-button gate.
            float x = Input.GetAxisRaw("Mouse X"), y = Input.GetAxisRaw("Mouse Y");
            if (MathF.Abs(x) > .001f || MathF.Abs(y) > .001f) _idleView.Activity(Now);
            _pitch.AddMouseDelta(y);
        }
        catch (Exception e)
        {
            _pitchInputAvailable = false;
            Info($"Vertical input unavailable; preserving game rotation until restart: {e}");
        }
    }

    internal void FinalizePose(string phase)
    {
        if (_stopped || _poseFrame == Time.frameCount) return;
        _poseFrame = Time.frameCount;
        try
        {
            if (!_enabled || !HasHead() || Now < _retryAfter) return;
            _eventView?.Refresh();
            bool normal = NormalIdleAllowed();
            _idleView.Step(Now, Time.deltaTime, normal, Now - _lastMove < .2);
            if (!normal) _idleMotion.Reset();
            if (_eventView?.Active == true)
            {
                _bodyFollow?.Restore();
                _headLook.Apply(_head!, _eventView.Rotation(_head!));
            }
            else if (_followHead && _foreground && _pitchInputAvailable && Time.timeScale > 0
                && _camera != null && IsTarget(_camera)
                && (!_followBody || _bodyFollow?.CanFollowHead == true))
            {
                Vector3 view = _camera.transform.eulerAngles;
                if (!_pitch.Initialized) _pitch.Initialize(view.x);
                ApplyNormalHead(Quaternion.Euler(_pitch.Angle, view.y, view.z));
            }
            if (Now >= _nextPoseReport)
            {
                _nextPoseReport = Now + 5;
                Info($"Unified pose: frame={_poseFrame}, phase={phase}, event={_eventView?.Active == true}, idleWeight={_idleView.Weight:F2}");
            }
        }
        catch (Exception e) { DisableAfterControlError(e); }
        finally { RecordPose("FinalPose"); }
    }

    private void RecordPose(string phase, Camera? camera = null) =>
        _poseDiagnostics.Record(phase, camera, _camera, _player, _head,
            _enabled, _eventView?.Active == true, Info);

    private bool NormalIdleAllowed() => !_stopped && _enabled && _followHead && _foreground
        && _pitchInputAvailable && _pitch.Initialized && Time.timeScale > 0 && HasHead()
        && _eventView?.Active != true && _eventView?.InputBlocked != true
        && _bodyFollow?.CanFollowHead == true;

    private Quaternion ApplyNormalHead(Quaternion view)
    {
        _headLook.Restore();
        Quaternion animated = _idleMotion.Sample(Time.frameCount, Time.deltaTime, _head!.rotation);
        Quaternion rotation = Quaternion.Slerp(view, animated, _idleView.Weight);
        _headLook.Apply(_head, rotation);
        return rotation;
    }

    // Evidence from 0.1.3: this screen camera stays ~4 m from the eye while
    // MainCharCam renders CharRederTexture (512x1024) at the eye location.
    private static bool IsTarget(Camera camera) => camera != null && camera.isActiveAndEnabled
        && camera.gameObject.name == "Main Scene Cam"
        && camera.gameObject.scene.name == "MainCanvasScene"
        && camera.targetTexture == null && camera.targetDisplay == 0;

    private void BeforeRender(Camera camera)
    {
        if (camera == null) return;
        RecordPose("PreCullBefore", camera);
        if (_stopped || !_enabled || Now < _retryAfter) return;
        try
        {
            if (!IsTarget(camera)) return;
            if (!HasHead() || _restorePending) return;
            if (_camera != camera)
                Info($"Screen camera bound: id={camera.GetInstanceID()}, name={camera.name}, scene={camera.gameObject.scene.name}, output=screen");
            _camera = camera;
            Vector3 eye = _head!.TransformPoint(EyeOffset + new Vector3(0f, _eyeHeightAdjustment, 0f));
            _originalPosition = camera.transform.position;
            _originalRotation = camera.transform.rotation;
            _originalNearClip = camera.nearClipPlane;
            _originalMask = camera.cullingMask;
            _restoreCamera = camera;
            _restorePending = true;
            // All settings below are restored after this camera renders.
            if (_hideHead || _hideHair) _headOcclusion?.Apply();
            camera.transform.position = eye;
            camera.transform.rotation = _head.rotation;
            _renderEye = eye;
            camera.nearClipPlane = Math.Min(_originalNearClip, _nearClip);
            if (_showBody && _visibility != null)
            {
                try { camera.cullingMask = _visibility.Apply(_originalMask); }
                catch (Exception e)
                {
                    _visibility.Restore();
                    _showBody = false;
                    Info($"Body override unavailable; first-person camera remains active: {e}");
                }
            }
            ++_writes;
        }
        catch (Exception e)
        {
            try { RestoreRenderState(); } catch (Exception restoreError) { Fault(restoreError); }
            Fault(e);
        }
        finally { RecordPose("PreCullAfter", camera); }
    }

    private void AfterRender(Camera camera)
    {
        if (_stopped) return;
        RecordPose("PostRenderBeforeRestore", camera);
        try
        {
            // Capture the position actually used for rendering BEFORE restoring it.
            _diagnostics?.AfterRender(camera, _head, _camera, _enabled,
                _restorePending && camera == _restoreCamera ? _renderEye : (Vector3?)null);
        }
        finally
        {
            try
            {
                if (_restorePending && camera == _restoreCamera) RestoreRenderState();
            }
            catch (Exception e) { Fault(e); }
        }
    }

    private void RestoreRenderState()
    {
        // Restore material slots before camera cleanup.
        try { _headOcclusion?.Restore(); } catch (Exception e) { Fault(e); }
        _visibility?.Restore();
        if (!_restorePending) return;
        Camera? camera = _restoreCamera;
        _restorePending = false;
        _restoreCamera = null;
        if (camera != null)
        {
            try { camera.transform.position = _originalPosition; } catch (Exception e) { Fault(e); }
            try { camera.transform.rotation = _originalRotation; } catch (Exception e) { Fault(e); }
            try { camera.nearClipPlane = _originalNearClip; } catch (Exception e) { Fault(e); }
            try { camera.cullingMask = _originalMask; } catch (Exception e) { Fault(e); }
            ++_restores;
        }
    }

    private bool HasHead() => _player != null && _player.activeInHierarchy
        && _animator != null && _head != null && _head.gameObject.activeInHierarchy;

    private void CheckScene()
    {
        int current = SceneManager.GetActiveScene().handle;
        if (_scene == current) return;
        RestoreRenderState();
        _bodyFollow?.Restore();
        _headLook.Restore();
        _wallCutout?.Restore();
        _eventView?.Reset();
        _idleView.Reset(); _idleMotion.Reset();
        _idleAnimation?.Reset();
        _lastMove = double.NegativeInfinity;
        _wasEvent = false;
        _scene = current;
        _player = null; _animator = null; _head = null; _camera = null;
        _nextSearch = 0;
        _binding = "waiting after scene change";
        _pitch.Invalidate();
    }

    private void Info(string message) => _log?.Write(message);
    private void Fault(Exception error)
    {
        _retryAfter = Now + 1.0;
        if (Now < _nextWarning) return;
        _nextWarning = Now + 10.0;
        Info($"Retrying after error: {error}");
    }

    internal ProjectTarkovH.OrientationMethod? BeforeMovementInputs(ProjectTarkovH.CharacterController controller, ref ProjectTarkovH.PlayerCharacterInputs inputs)
    {
        if (_stopped || !_enabled || !_foreground || controller == null) return null;
        try
        {
            if (_player != null && controller.gameObject == _player
                && (MathF.Abs(inputs.MoveAxisForward) > .001f || MathF.Abs(inputs.MoveAxisRight) > .001f))
            { _lastMove = Now; _idleView.Activity(Now); }
            _eventView?.Refresh();
            if (_eventView?.Active == true || _eventView?.InputBlocked == true)
            { _bodyFollow?.Restore(); return null; }
            if (_player != null && controller.gameObject == _player
                && controller.isActiveAndEnabled && !controller.DoingSex && controller._movementEnabled
                && Time.timeScale > 0 && !SprintPolicy.Allows(inputs.MoveAxisForward, inputs.MoveAxisRight))
                inputs.Run = false;
            return _followBody ? _bodyFollow?.BeforeInputs(controller) : null;
        }
        catch (Exception e) { DisableAfterControlError(e); return null; }
    }

    internal void BeforeMovementVelocity(ProjectTarkovH.CharacterController controller, float deltaTime)
    {
        if (_stopped || !_enabled || !_foreground || !_followBody || controller == null) return;
        try
        {
            _eventView?.Refresh();
            if (_eventView?.Active == true || _eventView?.InputBlocked == true)
            { _bodyFollow?.Restore(); return; }
            _bodyFollow?.BeforeVelocity(controller, deltaTime);
        }
        catch (Exception e) { DisableAfterControlError(e); }
    }

    private void DisableAfterControlError(Exception error)
    {
        _enabled = false;
        try { _bodyFollow?.Restore(); } catch { }
        try { _headLook.Restore(); } catch { }
        _pitch.Invalidate();
        Info($"First person disabled after control error; native third person retained: {error}");
    }

    internal bool AllowRandomIdle(ProjectTarkovH.CharacterController controller)
    {
        try
        {
            return controller == null || _player == null || controller.gameObject != _player
                || !NormalIdleAllowed() || !_idleView.Controlling(Now);
        }
        catch { return true; } // Optional idle hook must never disrupt the game.
    }

    internal void Stop()
    {
        if (_stopped) return;
        _stopped = true;
        _enabled = false;
        try { _posePhase?.Dispose(); } catch (Exception e) { Fault(e); }
        _posePhase = null;
        _wallCutout?.Restore();
        try { _movementHook?.Dispose(); } catch (Exception e) { Fault(e); }
        _movementHook = null;
        try { RestoreRenderState(); } catch (Exception e) { Fault(e); }
        try { _bodyFollow?.Restore(); } catch (Exception e) { Fault(e); }
        try { _headLook.Restore(); } catch (Exception e) { Fault(e); }
        try
        {
            if (_preInstalled && _preCull != null)
                Camera.onPreCull = Il2CppSystem.Delegate.Remove(Camera.onPreCull, _preCull)?.Cast<Camera.CameraCallback>();
        }
        catch (Exception e) { Fault(e); }
        try
        {
            if (_postInstalled && _postRender != null)
                Camera.onPostRender = Il2CppSystem.Delegate.Remove(Camera.onPostRender, _postRender)?.Cast<Camera.CameraCallback>();
        }
        catch (Exception e) { Fault(e); }
        _preInstalled = _postInstalled = false;
        _preCull = _postRender = null;
        _headOcclusion?.Dispose();
        Instance = null;
        Info("Stopped; temporary camera and renderer settings restored.");
        _log?.Close();
    }

    public override bool Unload()
    {
        Stop();
        if (_driver != null) UnityEngine.Object.Destroy(_driver);
        _driver = null;
        return true;
    }

    [DllImport("user32.dll", ExactSpelling = true)] private static extern short GetAsyncKeyState(int key);
    [DllImport("user32.dll", ExactSpelling = true)] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll", ExactSpelling = true)] private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
}

public sealed class Driver : MonoBehaviour
{
    public Driver(IntPtr pointer) : base(pointer) { }
    public void Update() => Plugin.Instance?.Tick();
    public void OnDestroy() => Plugin.Instance?.Stop();
}
