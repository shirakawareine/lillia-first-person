# Lillia First Person

[English](README.md) · [日本語](README.ja.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md)

An unofficial, toggleable first-person plugin for **Novice Succubus Lillia**. Version **0.1.28 (test release)**, tested with game 1.1, Unity 2022.3.62f2, Windows x64, IL2CPP, and BepInEx 6 be.788.

## Installation and troubleshooting

### Choose by installed components

Use game **version 1.1**, Windows x64. The bundle contains BepInEx **6 be.788 for IL2CPP x64**; Mono, x86 and BepInEx 5 packages are not substitutes. It does not include the game.

- **BepInEx not installed:** download `FirstPersonTest-0.1.28-bundle-win-x64.zip` from [Releases](https://github.com/shirakawareine/lillia-first-person/releases/tag/v0.1.28).
- **Compatible BepInEx already installed:** download `FirstPersonTest-0.1.28-release.zip` or `FirstPersonTest.dll`, and keep the existing loader and settings.
- **Source code:** `FirstPersonTest-0.1.28-source.zip` is for building the plugin, not installing it. GitHub's automatic source archives are not installation packages either.

Check the game folder for `BepInEx` and `winhttp.dll`. Their presence indicates an existing loader or modification that needs checking; do not blindly overwrite it. Confirm the BepInEx version and IL2CPP runtime in `BepInEx/LogOutput.log` when available.

### Bundle installation (BepInEx not installed)

1. Save and exit the game. Back up saves and any existing configuration before changing the installation.
2. Use a short game path such as `C:\Games\Lillia`. Move the **whole game folder**, not just the EXE, if needed. A deeply nested test copy failed to load character content even without BepInEx; the same files worked at a shorter path. The exact path-length threshold is unknown.
3. Extract **all contents** of the bundle beside `NoviceSuccubusLillia.exe`. Do not run the game inside an archive, extract into the Data folder, or leave an extra bundle folder between the EXE and the loader files.
4. Compare the layout below. The EXE and Data folder are supplied by your game installation; the bundle adds the loader, runtime, plugin and configuration.

```text
C:\Games\Lillia\
  NoviceSuccubusLillia.exe
  NoviceSuccubusLillia_Data\
  winhttp.dll
  doorstop_config.ini
  dotnet\
  BepInEx\
    core\
    plugins\FirstPersonTest.dll
    config\BepInEx.cfg
    config\local.lillia.firstpersontest.cfg
```

5. Launch the usual game EXE. First launch needs Internet access to download Unity base libraries and time to generate `BepInEx/interop`; several minutes may be needed. Do not launch multiple copies while waiting. No log console window is expected.
6. Enter normal gameplay. First person starts enabled. With the game focused, test **F8** to switch views and **F9** to hide/show hair. The plugin log should contain `Loaded v0.1.28`. Successful loading alone does not verify every gameplay feature.

### Plugin installation or update (BepInEx installed)

1. Save and exit. Back up the old DLL and `BepInEx/config/local.lillia.firstpersontest.cfg` outside the `plugins` folder.
2. Replace `BepInEx/plugins/FirstPersonTest.dll`. Keep only one copy, including in subfolders; do not leave backup DLLs there. Keep your existing loader and configuration.
3. On first plugin-only installation, launch once to generate the configuration, then exit. A newly generated `ClipLimits` is empty; the bundle already contains four presets.
4. Updating the DLL does **not** merge new presets into existing settings. In `[EventView]`, merge the four-entry example below into the existing `ClipLimits` line, keeping custom entries. Use one entry per exact clip name and keep the whole value on one line. The added Gsun entry is `Fellatio_Female(Gsun)_Loop|3|2|2`.
5. Save and restart. Configuration is read at startup. The bundle revision changes presets and documentation, not the 0.1.28 DLL.

### Troubleshooting

| Symptom | Check first |
| --- | --- |
| No first-person view or no F8 response | Enter gameplay, focus the game window, check the folder layout and duplicate DLLs. Then check loader and plugin logs for a successful load or an error. |
| First launch appears stuck | Check whether `BepInEx/LogOutput.log` is still progressing through downloads or interop generation. If it reports a download failure, check connectivity and the specific error before retrying; do not repeatedly start extra copies. |
| `Character content could not be loaded` | Look for `A local absolute bundle path is required` in the Unity player log. If present, exit and move the entire game to a shorter path. The message alone does not prove a path issue; preserve logs if it persists. |
| Event view still has a wide range | Check the exact `Event view: clip=...` name, the matching ClipLimits entry and whether you restarted. Names are case-sensitive; unknown clips use general limits. |
| Hair or an NPC blocks the view | F9 toggles hair; F8 returns to third person during obstructed scenes. |

Loader log: `BepInEx/LogOutput.log`. Plugin log: `BepInEx/FirstPersonTest.log`. Unity log: `%USERPROFILE%\AppData\LocalLow\PaconeSoft\Novice Succubus Lillia\Player.log`. Preserve relevant logs before another launch; do not clear saves or caches as a routine installation step.

To temporarily disable first person, press F8. To uninstall the plugin, exit and remove only `BepInEx/plugins/FirstPersonTest.dll`; keep shared loader files needed by other mods. To roll back, restore the backed-up DLL and configuration. `SHA256SUMS.txt` provides download checksums.

## Screenshots

### First person — hair hidden

![First person — hair hidden](docs/screenshots/first-person-hair-hidden.png)

An unobstructed first-person street view. The game’s character preview remains visible on the left.

### First person — hair visible

![First person — hair visible](docs/screenshots/first-person-hair-visible.png)

Hair can naturally obstruct the view. Press **F9** to hide or show hair in first person.

### Third person

![Third person](docs/screenshots/third-person.png)

Press **F8** to switch between first- and third-person views.

These screenshots show viewing modes, not a controlled performance comparison.

## Behavior in 0.1.28

- The body faces the viewing direction. W/A/S/D moves relative to the camera independently of facing.
- Only forward and diagonally forward movement can sprint. Sideways, backward, and diagonally backward movement stays at walking speed.
- One head pose is finalized after animation/IK and before hair physics, shared by the main view and corner preview.
- Removes the preview-only 65° head limit, preview pose swapping, and per-camera forced skinning updates.
- Preserves idle head-animation following, interruption of idle gestures on input, animation-relative event limits, head-obstruction material handling, F9 hair visibility, and suppression of wall fading in first person.

The animation's stride length is preserved and redirected toward movement input. The native motor still handles slopes, collisions, and external forces. Movement speed, stamina, airborne movement, gravity, and game assets are not replaced.

**Validation:** compiled against the game's generated assemblies; direction conversion, control restoration, pose ordering, idle behavior, and event simulation checks passed. In-game testing on game 1.1 was reported successful, with subjectively smoother performance. No frame-rate benchmark or exhaustive scene/event testing has been performed.

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

**The plugin DLL has no built-in per-animation limits. With the plugin-only package, a newly generated ClipLimits is empty. The fresh-install bundle already includes the four entries below.** Default general limits are 75° left/right, 75° up, and 85° down; your changes to the settings override these defaults. Installing the DLL alone does not add the restrictions below.

1. Run the game once with the plugin to generate its configuration, then exit.
2. Find the existing `[EventView]` section in `BepInEx/config/local.lillia.firstpersontest.cfg`.
3. Replace its `ClipLimits =` line with the example. Do not duplicate the section or key. Keep the entire value on one line, with ASCII `;` and `|` separators.
4. Save and restart the game.

These four entries cover this animation family listed in the game 1.1 catalog:

```ini
[EventView]
DefaultYaw = 75
ClipLimits = Fellatio_Female_Loop|3|2|2;Fellatio_Female(bellied man)_Loop|3|2|2;Fellatio_Female_End|3|2|2;Fellatio_Female(Gsun)_Loop|3|2|2
```

The Gsun clip restriction (3° left/right, 2° up/down) has passed in-game testing, as confirmed by the tester. This does not imply exhaustive testing of all clips.

`3|2|2` allows 3° left/right, 2° up, and 2° down relative to the animated head, closely following its motion. Only these four clips use the narrow limits. Other clips retain the general limits. Use `0|0|0` to follow a clip's animation completely.

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

Output: `bin/Release/net6.0/FirstPersonTest.dll`. The source ZIP does not include game assemblies, assets, or BepInEx dependencies. See [IMPLEMENTATION.md](IMPLEMENTATION.md) for implementation notes.

## Uninstall

Exit the game and remove the plugin DLL. To roll back, replace it with a previously saved DLL and keep your configuration.
