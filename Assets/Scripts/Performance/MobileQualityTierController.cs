using UnityEngine;
using UnityEngine.Rendering;

namespace NeonApocalypse.Performance
{
    /// <summary>
    /// Applies a conservative graphics tier once at boot. The tier is intentionally
    /// sticky during play to avoid visible quality oscillation and frame-time spikes.
    /// Final tuning must still be validated on real target devices.
    /// </summary>
    public sealed class MobileQualityTierController : MonoBehaviour
    {
        public enum Tier { Low, Medium, High, Ultra }
        public Tier selectedTier = Tier.High;
        public bool autoSelect = true;
        public int highMemoryMb = 5500;
        public int mediumMemoryMb = 3500;
        public float highResolutionScale = 1f;
        public float mediumResolutionScale = 0.9f;
        public float lowResolutionScale = 0.78f;

        private void Awake()
        {
            if (!autoSelect) { Apply(selectedTier); return; }

            int memory = Mathf.Max(0, SystemInfo.systemMemorySize);
            int shaderLevel = SystemInfo.graphicsShaderLevel;
            bool capable = shaderLevel >= 45;

            if (!capable || memory < mediumMemoryMb) selectedTier = Tier.Low;
            else if (memory < highMemoryMb) selectedTier = Tier.Medium;
            else selectedTier = Tier.High;

            Apply(selectedTier);
        }

        public void Apply(Tier tier)
        {
            selectedTier = tier;
            QualitySettings.lodBias = tier == Tier.Low ? 0.65f : tier == Tier.Medium ? 0.82f : tier == Tier.High ? 1f : 1.12f;
            QualitySettings.shadowDistance = tier == Tier.Low ? 28f : tier == Tier.Medium ? 42f : tier == Tier.High ? 58f : 72f;
            QualitySettings.pixelLightCount = tier == Tier.Low ? 1 : tier == Tier.Medium ? 2 : tier == Tier.High ? 3 : 4;
            QualitySettings.maximumLODLevel = 0;

            float scale = tier == Tier.Low ? lowResolutionScale : tier == Tier.Medium ? mediumResolutionScale : highResolutionScale;
            ScalableBufferManager.ResizeBuffers(scale, scale);
        }
    }
}
