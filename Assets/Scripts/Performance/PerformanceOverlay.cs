using UnityEngine;
using NeonApocalypse.World;

namespace NeonApocalypse.Performance
{
    public sealed class PerformanceOverlay : MonoBehaviour
    {
        public bool startHidden = true;
        private bool visible;
        private float ema;
        private GUIStyle style;

        private void Start()
        {
            visible = !startHidden;
            style = new GUIStyle(GUI.skin.label) { fontSize = 22, richText = true };
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3)) visible = !visible;
            float dt = Time.unscaledDeltaTime;
            ema = ema <= 0f ? dt : Mathf.Lerp(ema, dt, 0.08f);
        }

        private void OnGUI()
        {
            if (!visible) return;
            int fps = Mathf.RoundToInt(1f / Mathf.Max(0.0001f, ema));
            float scale = ScalableBufferManager.widthScaleFactor;
            int enemies = 0;
            if (SpawnDirector.Instance) enemies = SpawnDirector.Instance.GetActiveCount();
            GUI.Label(new Rect(16, 16, 480, 120), $"<b>NEON PERFORMANCE</b>\nFPS: {fps} | Frame: {(ema * 1000f):0.0} ms\nRender Scale: {(scale * 100f):0}% | Enemies: {enemies}", style);
        }
    }
}
