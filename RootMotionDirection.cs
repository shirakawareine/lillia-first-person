using System;
using UnityEngine;

namespace FirstPersonTest;

internal static class RootMotionDirection
{
    internal static bool HasDirection(Vector3 value) => Finite(value) && value.sqrMagnitude > .00000001f;
    private static bool Finite(Vector3 v) => float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);

    internal static Vector3 Redirect(Vector3 localDelta, Quaternion motorRotation, Vector3 worldDirection)
    {
        if (!Finite(localDelta) || !HasDirection(worldDirection)) return localDelta;
        // Motor local Y is its up axis. Keep the original stride length and
        // vertical component; the native motor still projects onto the ground.
        Vector3 localDirection = Quaternion.Inverse(motorRotation) * worldDirection;
        localDirection.y = 0;
        float directionLength = localDirection.magnitude;
        float stride = MathF.Sqrt(localDelta.x * localDelta.x + localDelta.z * localDelta.z);
        if (!float.IsFinite(directionLength) || directionLength < .00001f || !float.IsFinite(stride)) return localDelta;
        return new Vector3(localDirection.x / directionLength * stride,
            localDelta.y, localDirection.z / directionLength * stride);
    }
}
