using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class VehicleDamageFeedback : MonoBehaviour
    {
        [SerializeField] private CameraShake cameraShake;
        [SerializeField] private GameObject[] damagedVisuals;
        [SerializeField] private float maxDamage = 100f;
        [SerializeField] private float collisionDamageScale = 0.9f;
        public float Damage { get; private set; }
        public float Health01 => 1f - Mathf.Clamp01(Damage / Mathf.Max(1f, maxDamage));

        private void Awake()
        {
            if (!cameraShake) cameraShake = FindFirstObjectByType<CameraShake>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            float impact = collision.relativeVelocity.magnitude;
            if (impact < 7f) return;
            Damage = Mathf.Clamp(Damage + impact * collisionDamageScale, 0f, maxDamage);
            cameraShake?.Impact(Mathf.Clamp01(impact / 45f));
            int stage = Mathf.Clamp(Mathf.FloorToInt(Damage / (maxDamage / Mathf.Max(1, damagedVisuals.Length))), 0, damagedVisuals.Length - 1);
            if (damagedVisuals != null && damagedVisuals.Length > 0) damagedVisuals[stage]?.SetActive(true);
        }
    }
}