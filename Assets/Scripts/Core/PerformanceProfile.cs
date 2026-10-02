using UnityEngine;

namespace NeonApocalypse.Core
{
    public class PerformanceProfile : MonoBehaviour
    {
        public int targetFps = 60;
        public void ApplyLow() => Apply(2, 1, 30);
        public void ApplyMedium() => Apply(2, 2, 45);
        public void ApplyHigh() => Apply(3, 2, targetFps);

        public void Apply(int qualityLevel, int shadowResolution, int fps)
        {
            QualitySettings.SetQualityLevel(Mathf.Clamp(qualityLevel, 0, QualitySettings.names.Length - 1), true);
            QualitySettings.shadowResolution = (ShadowResolution)Mathf.Clamp(shadowResolution, 0, 3);
            Application.targetFrameRate = Mathf.Max(30, fps);
        }
    }
}
