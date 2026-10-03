using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class VehicleStabilityAssist : MonoBehaviour
    {
        [SerializeField] private Rigidbody body;
        [SerializeField] private float uprightTorque = 3.5f;
        [SerializeField] private float highSpeedDamping = 0.45f;
        [SerializeField] private float activationSpeed = 18f;

        private void Awake() { if (!body) body = GetComponent<Rigidbody>(); }

        private void FixedUpdate()
        {
            if (!body) return;
            Vector3 tiltAxis = Vector3.Cross(transform.up, Vector3.up);
            body.AddTorque(tiltAxis * uprightTorque, ForceMode.Acceleration);
            if (body.linearVelocity.magnitude > activationSpeed)
                body.angularVelocity *= Mathf.Clamp01(1f - highSpeedDamping * Time.fixedDeltaTime);
        }
    }
}