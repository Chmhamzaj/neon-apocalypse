using UnityEngine;

namespace NeonApocalypse.World.Mega
{
    /// <summary>Applies conservative distance and shadow settings suitable for a large streamed mobile world.</summary>
    public sealed class MegaWorldRuntimeQuality : MonoBehaviour
    {
        public Camera targetCamera;
        public float gameplayFarClip = 900f;
        public float explorationFarClip = 1500f;
        public float highShadowDistance = 70f;
        public float lowShadowDistance = 35f;
        public bool mobileSafe = true;

        private void Awake()
        {
            if (!targetCamera) targetCamera = Camera.main;
            Apply();
        }

        public void Apply()
        {
            if (targetCamera) targetCamera.farClipPlane = mobileSafe ? gameplayFarClip : explorationFarClip;
            QualitySettings.shadowDistance = mobileSafe ? lowShadowDistance : highShadowDistance;
            QualitySettings.pixelLightCount = mobileSafe ? 2 : 4;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        }
    }
}
