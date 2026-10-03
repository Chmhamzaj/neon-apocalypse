using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class TrafficCollisionFeedback : MonoBehaviour
    {
        [SerializeField] private CameraShake cameraShake;
        [SerializeField] private float minimumImpact = 8f;
        private void Awake() { if (!cameraShake) cameraShake = FindFirstObjectByType<CameraShake>(); }
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude < minimumImpact) return;
            if (collision.collider.GetComponentInParent<CarController>())
                cameraShake?.Impact(Mathf.Clamp01(collision.relativeVelocity.magnitude / 35f));
        }
    }
}