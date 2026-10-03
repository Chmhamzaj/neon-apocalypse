using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class CollisionRecovery : MonoBehaviour
    {
        [SerializeField] private float minSpeedAfterImpact = 8f;
        [SerializeField] private float resetDelay = 0.45f;
        private Rigidbody body;
        private float cooldown;
        private Vector3 safePosition;
        private Quaternion safeRotation;

        private void Awake() => body = GetComponent<Rigidbody>();
        private void Start() { safePosition = transform.position; safeRotation = transform.rotation; }
        private void Update() { cooldown = Mathf.Max(0f, cooldown - Time.deltaTime); }

        private void OnCollisionEnter(Collision collision)
        {
            if (cooldown > 0f || collision.relativeVelocity.magnitude < 12f) return;
            safePosition = transform.position;
            safeRotation = transform.rotation;
            cooldown = resetDelay;
            if (body) body.linearVelocity *= 0.72f;
        }

        public void Recover()
        {
            transform.SetPositionAndRotation(safePosition + Vector3.up * 0.25f, safeRotation);
            if (body) { body.linearVelocity = transform.forward * minSpeedAfterImpact; body.angularVelocity = Vector3.zero; }
        }
    }
}
