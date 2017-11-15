using System;
using System.Reflection;
using UnityEngine;

namespace Framework
{
    public static class GeometryUtils
    {
        private static Action<Plane[], Matrix4x4> m_getFrustumPlanesInternal;

        public static void CalculateFrustumPlanes(Plane[] planes, Camera cam)
        {
            CalculateFrustumPlanes(planes, cam.projectionMatrix * cam.worldToCameraMatrix);
        }

        public static void CalculateFrustumPlanes(Plane[] planes, Matrix4x4 worldToProjectionMatrix)
        {
            if (m_getFrustumPlanesInternal == null)
            {
                MethodInfo method = typeof(GeometryUtility).GetMethod(
                    "Internal_ExtractPlanes",
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(Plane[]), typeof(Matrix4x4) }, null);

                if (method == null)
                {
                    Debug.LogError("Failed to reflect internal method. The Unity version may not contain the presumed named method in GeometryUtility.");
                }

                m_getFrustumPlanesInternal = Delegate.CreateDelegate(typeof(Action<Plane[], Matrix4x4>), method) as Action<Plane[], Matrix4x4>;
            }

            m_getFrustumPlanesInternal(planes, worldToProjectionMatrix);
        }
    }
}
