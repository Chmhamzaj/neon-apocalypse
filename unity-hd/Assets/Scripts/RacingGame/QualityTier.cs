using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class QualityTier : MonoBehaviour
    {
        public enum Tier { Mid, High, Ultra }

        [SerializeField] private Tier forcedTier = Tier.High;
        [SerializeField] private bool autoDetect = true;

        private void Start()
        {
            Tier tier = autoDetect ? DetectTier() : forcedTier;
            Apply(tier);
        }

        private static Tier DetectTier()
        {
            int ram = SystemInfo.systemMemorySize;
            int gpu = SystemInfo.graphicsMemorySize;
            if (ram >= 8000 && gpu >= 4000) return Tier.Ultra;
            if (ram >= 4000 && gpu >= 2000) return Tier.High;
            return Tier.Mid;
        }

        private static void Apply(Tier tier)
        {
            QualitySettings.SetQualityLevel((int)tier, true);
            Application.targetFrameRate = tier == Tier.Mid ? 30 : 60;
        }
    }
}
