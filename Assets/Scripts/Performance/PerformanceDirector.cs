using UnityEngine;
using UnityEngine.Rendering;

namespace NeonApocalypse.Performance
{
    /// <summary>Central mobile performance policy. Prefers stable frame pacing over uncontrolled visual load.</summary>
    public sealed class PerformanceDirector : MonoBehaviour
    {
        public static PerformanceDirector Instance { get; private set; }

        [Header("Frame pacing")]
        [Range(30, 120)] public int targetFps = 60;
        [Range(0.05f, 0.5f)] public float dynamicResolutionMinScale = 0.70f;
        public bool useDynamicResolution = true;

        [Header("Mobile safety limits")]
        public int maxParticleSystems = 80;
        public int maxRealtimeLights = 12;

        private float frameEma;
        private int frameSamples;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFps;
            Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;
            ConfigurePlatformDefaults();
        }

        private void ConfigurePlatformDefaults()
        {
            bool mobile = Application.isMobilePlatform;
            if (mobile)
            {
                QualitySettings.asyncUploadTimeSlice = 2;
                QualitySettings.asyncUploadBufferSize = 16;
                QualitySettings.maximumLODLevel = 0;
                QualitySettings.streamingMipmapsActive = true;
                QualitySettings.streamingMipmapsMemoryBudget = 512;
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.softParticles = false;
                QualitySettings.billboardsFaceCameraPosition = true;
                QualitySettings.shadowDistance = 55f;
                QualitySettings.shadowCascades = 2;
            }

            if (useDynamicResolution && ScalableBufferManager.widthScaleFactor > 0f)
            {
                ScalableBufferManager.ResizeBuffers(1f, 1f);
            }
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            frameEma = frameSamples == 0 ? dt : Mathf.Lerp(frameEma, dt, 0.08f);
            frameSamples++;

            if (!useDynamicResolution || frameSamples < 30 || !Application.isMobilePlatform) return;

            float fps = 1f / Mathf.Max(0.001f, frameEma);
            float scale = ScalableBufferManager.widthScaleFactor;
            if (fps < targetFps - 5f)
                scale = Mathf.Max(dynamicResolutionMinScale, scale - 0.025f);
            else if (fps > targetFps + 8f)
                scale = Mathf.Min(1f, scale + 0.0125f);

            ScalableBufferManager.ResizeBuffers(scale, scale);
        }

        public void ApplyPreset(int qualityLevel, int fps)
        {
            int levels = Mathf.Max(1, QualitySettings.names.Length);
            QualitySettings.SetQualityLevel(Mathf.Clamp(qualityLevel, 0, levels - 1), true);
            targetFps = Mathf.Clamp(fps, 30, 120);
            Application.targetFrameRate = targetFps;
        }
    }
}
