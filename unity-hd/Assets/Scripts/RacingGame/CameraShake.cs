using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class CameraShake : MonoBehaviour
    {
        [SerializeField] private float impactAmplitude = 0.08f;
        [SerializeField] private float impactDuration = 0.18f;
        [SerializeField] private float nitroAmplitude = 0.025f;
        private float trauma;
        private Vector3 baseLocalPosition;
        public void Impact(float strength = 1f) => trauma = Mathf.Max(trauma, impactDuration * Mathf.Clamp01(strength));
        public void NitroPulse() => trauma = Mathf.Max(trauma, nitroAmplitude);
        private void Awake() => baseLocalPosition = transform.localPosition;
        private void LateUpdate()
        {
            float normalized = impactDuration <= 0f ? 0f : Mathf.Clamp01(trauma / impactDuration);
            float amplitude = impactAmplitude * normalized + nitroAmplitude * (normalized > 0f ? 0.25f : 0f);
            transform.localPosition = baseLocalPosition + Random.insideUnitSphere * amplitude;
            trauma = Mathf.Max(0f, trauma - Time.deltaTime);
        }
    }
}
