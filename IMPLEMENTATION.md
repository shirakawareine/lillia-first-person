# Implementation notes for 0.1.28

[English](IMPLEMENTATION.md) · [日本語](IMPLEMENTATION.ja.md) · [简体中文](IMPLEMENTATION.zh-CN.md) · [繁體中文](IMPLEMENTATION.zh-TW.md)

These findings come from metadata, native-method disassembly, and script execution order in the inspected game 1.1 build, rather than assumptions based on API names. The RVAs below are for local verification only. The plugin uses IL2CPP interop and Harmony; it does not hard-code these addresses.

## Movement

- `CharacterController.SetInputs`, RVA `0x744450`: converts input axes into camera-relative `_moveInputVector`. `TowardsCamera` makes `_lookInputVector` use the camera's planar facing direction; `TowardsMovement` uses the movement direction. This method also handles animation parameters and sprint state.
- `OnAnimatorMove`, RVA `0x742930`: transforms `Animator.deltaPosition` by the inverse character rotation and accumulates it in `_pendingRootMotionLocalDelta`. The normal movement branch does not accumulate local Y.
- `UpdateVelocity`, RVA `0x747340`: takes and clears the accumulated displacement at entry. When stably grounded, it transforms the displacement back to world space with `Motor.TransientRotation`, projects it onto the plane perpendicular to the character's up direction, and applies stride divided by time along the ground tangent. It adds `_internalVelocityAdd` separately at the end. Airborne movement has separate input-acceleration, gravity, and drag branches.

Enabling TowardsCamera alone would keep forward animation displacement aligned with the view, preventing strafing. Version 0.1.28 temporarily uses that mode within SetInputs and restores the original setting on return or exception. Before UpdateVelocity consumes the accumulated displacement, only its planar direction is changed. Native input vectors, velocity, gravity, and final motor velocity are not replaced.

The final stride after releasing input uses the last valid movement direction, preventing a sudden forward step after releasing a strafe key. This direction is cleared on disable, character replacement, and handoff to event animation. Root motion is not modified during special actions, movement locks, pauses, or airborne movement. Hooks are installed as a pair; if either fails, installation is rolled back to avoid taking over body facing alone.

## Pose

MonoScript execution order in the inspected `globalgamemanagers.assets`: FinalIK's IKExecutionOrder is 9996, individual IK scripts are 9997–9999, FullBodyBipedIK is 9999, and DynamicBone is 10000.

The native DynamicBone.LateUpdate path calls Prepare, UpdateParticles, and ApplyParticlesToTransforms in that order. A prefix on this LateUpdate therefore finalizes the head pose on its first invocation each frame, after existing animation/IK and before hair physics. Subsequent DynamicBone callbacks skip pose finalization through a frame-number gate.

A PlayerLoop callback after ScriptRunBehaviourLateUpdate invokes the same entry point for frames without active DynamicBone components. It shares the same frame gate, preventing repeated computation or repeated application of look offsets. Unloading removes this plugin's node from the current loop instead of restoring an old snapshot that could overwrite nodes added later by other plugins.

This is the sole point that finalizes the head pose. Before the next frame's update, the plugin restores the local rotation only if it still owns that pose, allowing Animator to produce a fresh base pose. If another system has already written a new pose, that value is preserved. Camera callbacks only read bones and adjust camera properties and visible materials; they no longer write to the head bone. Per-camera forced skinning updates are therefore unnecessary.

Normal look, idle following, and animation-relative event look share this finalization path. Idle handling retains the validated cancellation of ramdomPose on the Walk layer and the transition to NormalIdle. Event handling retains ClipLimits, while the body remains controlled by animation.

## Validation scope

- Built against the actual BepInEx and game interop assemblies with warnings treated as errors.
- Simulation checks cover eight movement directions and five view headings, displacement while the body is still turning, stopping strides, tilted motor coordinates, preservation of external forces, bypasses for airborne/paused/event states, disabling and character replacement, and pose restoration.
- Simulated update-loop checks cover finalizing once with multiple physics components, fallback without physics components, preservation of other plugins' nodes, and rollback after installation failure.
- Existing idle and event-angle regression checks passed.
- Subsequent in-game testing on game 1.1 was reported successful and subjectively smoother. This does not establish coverage of all scenes, collisions, or events. No frame-rate benchmark was performed.
