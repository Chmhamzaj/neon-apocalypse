using UnityEngine;
using NeonApocalypse.Performance;

namespace NeonApocalypse.World.Mega
{
    public sealed class WorldScaleTelemetry : MonoBehaviour
    {
        public MegaWorldConfig config;
        private PerformanceOverlay overlay;

        private void Start() => overlay = FindFirstObjectByType<PerformanceOverlay>();

        private void OnGUI()
        {
            if (!Debug.isDebugBuild || !config) return;
            int loaded = FindObjectsByType<MegaWorldChunk>(FindObjectsSortMode.None).Length;
            GUI.Label(new Rect(18, 18, 620, 24), $"WORLD {config.WidthMeters / 1000f:0.0} km x {config.DepthMeters / 1000f:0.0} km  |  {config.cellsX * config.cellsZ} cells  |  Loaded {loaded}");
        }
    }
}
