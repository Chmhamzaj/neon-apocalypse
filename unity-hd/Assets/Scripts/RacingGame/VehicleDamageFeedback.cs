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
            if (damagedVisuals != null)
                foreach (var visual in damagedVisuals) if (visual) visual.SetActive(false);
        }

        private void OnCollisionEnter(Collision collision)
        {
            float impact = collision.relativeVelocity.magnitude;
            if (impact < 7f) return;
            Damage = Mathf.Clamp(Damage + impact * collisionDamageScale, 0f, maxDamage);
            cameraShake?.Impact(Mathf.Clamp01(impact / 45f));
            if (damagedVisuals == null || damagedVisuals.Length == 0) return;
            int stage = Mathf.Clamp(Mathf.FloorToInt(Damage / (maxDamage / damagedVisuals.Length)), 0, damagedVisuals.Length - 1);
            for (int i = 0; i < damagedVisuals.Length; i++) if (damagedVisuals[i]) damagedVisuals[i].SetActive(i == stage);
        }
    }
}
