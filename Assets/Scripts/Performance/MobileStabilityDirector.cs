using UnityEngine;
using UnityEngine.Rendering;

namespace NeonApocalypse.Performance
{
    /// <summary>
    /// Conservative Android runtime guardrails. It prevents long catch-up loops,
    /// keeps physics cost bounded, and applies stable rendering defaults without
    /// forcing aggressive quality changes every frame.
    /// </summary>
    public sealed class MobileStabilityDirector : MonoBehaviour
    {
        public int targetFps = 60;
        [Range(30, 120)] public int physicsHz = 60;
        [Range(0.04f, 0.2f)] public float maximumDeltaTime = 0.08f;
        public int defaultSolverIterations = 6;
        public int defaultSolverVelocityIterations = 2;
        public float shadowDistance = 65f;
        public int pixelLightCount = 2;
        public float lodBias = 1.0f;
        public bool useMobileOptimizations = true;

        private void Awake()
        {
            Application.targetFrameRate = Mathf.Max(30, targetFps);
            QualitySettings.vSyncCount = 0;
            QualitySettings.maxQueuedFrames = 1;
            Time.fixedDeltaTime = 1f / Mathf.Max(30, physicsHz);
            Time.maximumDeltaTime = Mathf.Clamp(maximumDeltaTime, 0.04f, 0.2f);

            Physics.defaultSolverIterations = Mathf.Clamp(defaultSolverIterations, 4, 12);
            Physics.defaultSolverVelocityIterations = Mathf.Clamp(defaultSolverVelocityIterations, 1, 6);
            Physics.defaultMaxDepenetrationVelocity = 10f;
            Physics.reuseCollisionCallbacks = true;
            Physics.autoSyncTransforms = false;

            if (!useMobileOptimizations) return;

            QualitySettings.shadowDistance = Mathf.Clamp(shadowDistance, 25f, 120f);
            QualitySettings.pixelLightCount = Mathf.Clamp(pixelLightCount, 0, 4);
            QualitySettings.lodBias = Mathf.Clamp(lodBias, 0.5f, 2f);
            QualitySettings.maximumLODLevel = 0;

            // Dynamic resolution should remain gradual and is controlled by
            // PerformanceDirectorV2; do not resize buffers from this component.
            ScalableBufferManager.ResizeBuffers(1f, 1f);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) return;
            // Clear a potentially huge catch-up step after long background sleeps.
            Time.captureDeltaTime = 0f;
        }
    }
}
