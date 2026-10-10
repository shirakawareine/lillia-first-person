using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine.LowLevel;

namespace FirstPersonTest;

// Game 1.1 script order: FinalIK <= 9999, DynamicBone = 10000.
// Commit after animation/IK but before hair simulation. The end-of-LateUpdate
// callback covers frames with no active DynamicBone; both use one frame gate.
internal sealed class PosePhase : IDisposable
{
    private readonly Harmony _harmony = new("local.lillia.firstpersontest.pose");
    private readonly PlayerLoopSystem.UpdateFunction _callback;
    private readonly Il2CppSystem.Type _marker;
    internal PosePhase()
    {
        _marker = Il2CppType.Of<Driver>();
        _callback = DelegateSupport.ConvertDelegate<PlayerLoopSystem.UpdateFunction>(
            new Action(() => Plugin.Instance?.FinalizePose("AfterLateUpdate")))
            ?? throw new InvalidOperationException("Cannot create the pose update callback.");
        try
        {
            var target = AccessTools.Method(typeof(DynamicBone), "LateUpdate", Type.EmptyTypes)
                ?? throw new MissingMethodException("DynamicBone.LateUpdate");
            _harmony.Patch(target, prefix: new HarmonyMethod(typeof(PosePhase), nameof(BeforePhysics)));
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            if (!Insert(loop)) throw new InvalidOperationException("LateUpdate player-loop phase not found.");
            PlayerLoop.SetPlayerLoop(loop);
        }
        catch { Dispose(); throw; }
    }
    private static void BeforePhysics() => Plugin.Instance?.FinalizePose("BeforeHairPhysics");

    private bool Insert(PlayerLoopSystem node)
    {
        var children = node.subSystemList;
        if (children == null) return false;
        var target = Il2CppType.Of<UnityEngine.PlayerLoop.PreLateUpdate.ScriptRunBehaviourLateUpdate>();
        for (int i = 0; i < children.Length; i++)
        {
            var child = children[i];
            if (child.type == target)
            {
                var copy = new Il2CppReferenceArray<PlayerLoopSystem>(children.Length + 1);
                for (int j = 0; j <= i; j++) copy[j] = children[j];
                copy[i + 1] = new PlayerLoopSystem { type = _marker, updateDelegate = _callback };
                for (int j = i + 1; j < children.Length; j++) copy[j + 1] = children[j];
                node.subSystemList = copy;
                return true;
            }
            if (Insert(child)) { children[i] = child; return true; }
        }
        return false;
    }

    private bool Remove(PlayerLoopSystem node)
    {
        var children = node.subSystemList;
        if (children == null) return false;
        bool changed = false;
        var retained = new List<PlayerLoopSystem>(children.Length);
        foreach (var child in children)
        {
            if (child.type == _marker) { changed = true; continue; }
            changed |= Remove(child);
            retained.Add(child);
        }
        if (changed) node.subSystemList = new Il2CppReferenceArray<PlayerLoopSystem>(retained.ToArray());
        return changed;
    }
    public void Dispose()
    {
        _harmony.UnpatchSelf();
        // Remove only this callback from the current loop; retain other mods.
        var loop = PlayerLoop.GetCurrentPlayerLoop();
        if (Remove(loop)) PlayerLoop.SetPlayerLoop(loop);
    }
}
