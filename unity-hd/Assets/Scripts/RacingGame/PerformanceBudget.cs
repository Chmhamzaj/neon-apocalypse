using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class PerformanceBudget : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private int maxTraffic = 18;
        [SerializeField] private int maxParticles = 1200;
        public int MaxTraffic => maxTraffic;
        public int MaxParticles => maxParticles;
        private void Awake() { Application.targetFrameRate = targetFrameRate; QualitySettings.vSyncCount = 0; }
    }
}
