using System;
using HarmonyLib;
using Character = ProjectTarkovH.CharacterController;
using Inputs = ProjectTarkovH.PlayerCharacterInputs;
using Orientation = ProjectTarkovH.OrientationMethod;

namespace FirstPersonTest;

internal sealed class MovementInputHook : IDisposable
{
    private readonly Harmony _harmony = new("local.lillia.firstpersontest.movement");
    internal MovementInputHook()
    {
        try
        {
            var input = AccessTools.Method(typeof(Character), "SetInputs", new[] { typeof(Inputs).MakeByRefType() })
                ?? throw new MissingMethodException("CharacterController.SetInputs(ref PlayerCharacterInputs)");
            var velocity = AccessTools.Method(typeof(Character), "UpdateVelocity", new[] { typeof(UnityEngine.Vector3).MakeByRefType(), typeof(float) })
                ?? throw new MissingMethodException("CharacterController.UpdateVelocity(ref Vector3, float)");
            _harmony.Patch(velocity, prefix: new HarmonyMethod(typeof(MovementInputHook), nameof(VelocityPrefix)));
            _harmony.Patch(input, prefix: new HarmonyMethod(typeof(MovementInputHook), nameof(Prefix)),
                postfix: new HarmonyMethod(typeof(MovementInputHook), nameof(Postfix)),
                finalizer: new HarmonyMethod(typeof(MovementInputHook), nameof(Finalizer)));
        }
        catch { _harmony.UnpatchSelf(); throw; } // Never install facing without translation.
    }
    private static void Prefix(Character __instance, ref Inputs __0, out Orientation? __state)
        => __state = Plugin.Instance?.BeforeMovementInputs(__instance, ref __0);
    private static void Postfix(Character __instance, Orientation? __state)
        => BodyFollow.AfterInputs(__instance, __state);
    private static void Finalizer(Character __instance, Orientation? __state)
        => BodyFollow.AfterInputs(__instance, __state);
    private static void VelocityPrefix(Character __instance, float __1)
        => Plugin.Instance?.BeforeMovementVelocity(__instance, __1);
    internal void InstallIdleHook()
    {
        var target = AccessTools.Method(typeof(Character), "SetRandomBool", Type.EmptyTypes)
            ?? throw new MissingMethodException("CharacterController.SetRandomBool");
        _harmony.Patch(target, prefix: new HarmonyMethod(typeof(MovementInputHook), nameof(IdlePrefix)));
    }
    private static bool IdlePrefix(Character __instance) => Plugin.Instance?.AllowRandomIdle(__instance) ?? true;
    public void Dispose() => _harmony.UnpatchSelf();
}
