using UnityEngine;

namespace NeonApocalypse.World
{
    public class WorldDirector : MonoBehaviour
    {
        [Range(0, 24)] public float timeOfDay = 20f;
        public float dayLengthSeconds = 600f;
        public Light sun;
        public float rotationOffset = -90f;

        private void Update()
        {
            timeOfDay = (timeOfDay + 24f * Time.deltaTime / Mathf.Max(30f, dayLengthSeconds)) % 24f;
            if (sun)
            {
                float angle = timeOfDay / 24f * 360f + rotationOffset;
                sun.transform.rotation = Quaternion.Euler(angle, -35f, 0);
                float daylight = Mathf.Clamp01(Mathf.Sin((timeOfDay - 6f) / 12f * Mathf.PI));
                sun.intensity = Mathf.Lerp(0.12f, 1.2f, daylight);
            }
        }
    }
}
