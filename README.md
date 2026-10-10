# Lillia First Person

[English](README.md) · [日本語](README.ja.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md)

An unofficial, toggleable first-person plugin for **Novice Succubus Lillia**. Version **0.1.28 (test release)**, tested with game 1.1, Unity 2022.3.62f2, Windows x64, IL2CPP, and BepInEx 6 be.788.

## Behavior in 0.1.28

- The body faces the viewing direction. W/A/S/D moves relative to the camera independently of facing.
- Only forward and diagonally forward movement can sprint. Sideways, backward, and diagonally backward movement stays at walking speed.
- One head pose is finalized after animation/IK and before hair physics, shared by the main view and corner preview.
- Removes the preview-only 65° head limit, preview pose swapping, and per-camera forced skinning updates.
- Preserves idle head-animation following, interruption of idle gestures on input, animation-relative event limits, head-obstruction material handling, F9 hair visibility, and suppression of wall fading in first person.

The animation's stride length is preserved and redirected toward movement input. The native motor still handles slopes, collisions, and external forces. Movement speed, stamina, airborne movement, gravity, and game assets are not replaced.

**Validation:** compiled against the game's generated assemblies; direction conversion, control restoration, pose ordering, idle behavior, and event simulation checks passed. In-game testing on game 1.1 was reported successful, with subjectively smoother performance. No frame-rate benchmark or exhaustive scene/event testing has been performed.

## Download and installation

Download `FirstPersonTest-0.1.28-release.zip` from [Releases](https://github.com/shirakawareine/lillia-first-person/releases/tag/v0.1.28). A standalone DLL and source ZIP are also available.

**Install BepInEx separately: this is not an all-in-one installer.** No game files or loader are included.

1. Install BepInEx 6 IL2CPP x64 and run the game once to generate interop assemblies.
2. Save and exit the game.
3. Put `FirstPersonTest.dll` in `BepInEx/plugins/`, replacing the existing DLL when updating. Keep only one copy.
4. Keep `BepInEx/config/local.lillia.firstpersontest.cfg` and restart.

The startup log should show `Loaded v0.1.28`. Existing per-animation `ClipLimits` do not need to be reset.

## Controls

| Input | Action |
| --- | --- |
| F8 | Toggle first person; disabling restores the game's controls |
| F9 | Hide/show hair in first person for the current session |
| Mouse | Look left/right and up/down |
| W/A/S/D | Move relative to the view without turning toward movement input |
| Shift + W, W+A, or W+D | Request sprinting; the game still decides stamina and sprint eligibility |

Keys respond only while the game is in the foreground. Event free look does not require holding the right mouse button. The plugin does not take over cursor locking or UI events.

## Configuration

Exit the game before editing `BepInEx/config/local.lillia.firstpersontest.cfg`. Restart to apply changes.

| Section / key | Default | Description |
| --- | --- | --- |
| View / EyeHeightAdjustment | -0.01 | Head-local eye-height adjustment, -0.03 to 0.03 |
| View / LookUpDegrees | 75 | Maximum upward angle, 0–89° |
| View / LookDownDegrees | 85 | Maximum downward angle, 0–89° |
| View / PitchSensitivity | 3 | Normal gameplay vertical look sensitivity |
| View / InvertY | false | Invert vertical look |
| Body / FollowCameraYaw | true | Body faces the view independently of movement; keep enabled for unified control |
| Body / FollowHeadPitch | true | Align the head with the view; keep enabled for a unified pose |
| Body / HideHeadInFirstPerson | true | Hide detected obstructing head material slots in the main view, retain them in the preview |
| Body / HideHairOnStart | false | Hide hair on startup; F9 temporarily toggles it |
| Body / ShowPlayerRenderers | true | Show active body renderers in the main view |
| Body / NearClip | 0.02 | Near clipping distance, 0.005–0.1; never increases the original value |
| Body / HeadMaterialSlots | empty | Advanced: exact RendererName:slot entries separated by semicolons |
| EventView / DefaultYaw | 75 | Left/right event-view limit relative to the animated head |
| EventView / Sensitivity | 2 | Event free-look sensitivity |
| EventView / ClipLimits | empty | Per-animation left/right, up, and down limits |

Format: `ClipName|yaw|up|down;OtherClip|yaw|up|down`. Each angle is 0–89°. `0|0|0` follows the animation completely. Per-clip limits override the default yaw limit.

### ClipLimits example

**0.1.28 has no built-in per-animation limits. On a fresh installation, ClipLimits is empty.** Default general limits are 75° left/right, 75° up, and 85° down; your changes to the settings override these defaults. Installing the DLL alone does not add the restrictions below.

1. Run the game once with the plugin to generate its configuration, then exit.
2. Find the existing `[EventView]` section in `BepInEx/config/local.lillia.firstpersontest.cfg`.
3. Replace its `ClipLimits =` line with the example. Do not duplicate the section or key. Keep the entire value on one line, with ASCII `;` and `|` separators.
4. Save and restart the game.

These three entries have been used and verified in game 1.1:

```ini
[EventView]
DefaultYaw = 75
ClipLimits = Fellatio_Female_Loop|3|2|2;Fellatio_Female(bellied man)_Loop|3|2|2;Fellatio_Female_End|3|2|2
```

`3|2|2` allows 3° left/right, 2° up, and 2° down relative to the animated head, closely following its motion. Only these three clips use the narrow limits. Other clips retain the general limits. Use `0|0|0` to follow a clip's animation completely.

Merge existing custom entries with semicolons rather than discarding them. List each clip once. Names are case-sensitive and must match `Event view: clip=...` in the log. Game updates may rename clips; adjust entries to the new names when needed.

Legacy `LookDownClearance`, `LookDownForward`, `LookDownUp`, and `HeadPitchWeight` settings are unused. The eye position has no extra forward offset. Idle animation keeps its view transitions and rotation filtering; mouse look has no added preview-only angle limit or filtering.

## Known limitations

- Compatible, loaded strafe, backward, and turn-in-place animations have not been found. The mod redirects movement but does not create new footstep animations, so foot sliding can occur.
- During events, animation controls the body. NPCs may completely block the view; press F8 to return to third person.
- Extreme angles may still clip through geometry. Adjust limits for the actual animation.
- Camera names, the controller, skeleton, and update order are game-specific; game updates require compatibility checks.
- Hair physics parameters are unchanged. Performance improvements have not been benchmarked.

## Suggested checks

1. Keep the mouse still and try W/A/D/S and diagonals: movement should be correct without turning toward movement input.
2. Look up/down, then start/stop walking: the main view, corner model, and hair should remain consistent.
3. Shift+W and Shift+W+A/D may sprint; Shift+A/D/S should remain walking.
4. Wait for an idle gesture, then move the mouse: check view following and idle interruption.
5. Enter/leave an event: the body should remain animation-controlled, clip limits should apply, and normal movement should recover afterward.
6. Toggle F8 off/on and change scenes to check restoration.

## Logs

Plugin log: `BepInEx/FirstPersonTest.log`. For loading failures, check `BepInEx/LogOutput.log`.

- `Loaded v0.1.28`: version and startup status.
- `Body follow: SetInputs observed`: input callback received.
- `Locomotion:`: input direction, animation displacement, and redirected local displacement; at most once every five seconds.
- `Unified pose:`: pose-finalization phase; `BeforeHairPhysics` when active hair physics is present.
- `Pose:`: samples a frame once per second to inspect body/head/camera order.
- `Idle control:` / `Event view:`: idle interruption and event limits.
- `disabled after control error` / `Retrying after error`: errors and recovery.

Logging uses a bounded background file queue, without synchronous per-frame console output.

## Build from source

Use an SDK supporting .NET 6 and the game's generated BepInEx `core` and `interop` dependencies:

```powershell
dotnet build .\FirstPersonTest.csproj -c Release '-p:BepInExDir=D:\Games\Lillia\BepInEx'
```

Output: `bin/Release/net6.0/FirstPersonTest.dll`. Game assemblies, assets, and BepInEx dependencies are not distributed. See [IMPLEMENTATION.md](IMPLEMENTATION.md) for implementation notes (Chinese).

## Uninstall

Exit the game and remove the plugin DLL. To roll back, replace it with a previously saved DLL and keep your configuration.
