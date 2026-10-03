using UnityEngine;

namespace NitroStreetRush.Racing
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TrafficVehicleAI : MonoBehaviour
    {
        [SerializeField] private float cruiseSpeedKph = 115f;
        [SerializeField] private float speedVariationKph = 22f;
        [SerializeField] private float laneChangeChance = 0.08f;
        [SerializeField] private float laneWidth = 3.2f;
        [SerializeField] private float steeringResponsiveness = 2.5f;
        [SerializeField] private float lateralGrip = 8f;

        private Rigidbody body;
        private float targetSpeed;
        private float targetLane;
        private float laneTimer;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.mass = 1350f;
            body.linearDamping = 0.12f;
            body.angularDamping = 4f;
            targetSpeed = Mathf.Max(25f, cruiseSpeedKph + Random.Range(-speedVariationKph, speedVariationKph));
            targetLane = transform.position.x;
            laneTimer = Random.Range(2f, 6f);
        }

        private void FixedUpdate()
        {
            float speed = body.linearVelocity.magnitude;
            float targetMs = targetSpeed / 3.6f;
            float drive = Mathf.Clamp((targetMs - speed) * 3.5f, -10f, 18f);
            body.AddForce(transform.forward * drive, ForceMode.Acceleration);

            laneTimer -= Time.fixedDeltaTime;
            if (laneTimer <= 0f)
            {
                laneTimer = Random.Range(3f, 7f);
                if (Random.value < laneChangeChance)
                    targetLane = Mathf.Round((transform.position.x + Random.Range(-1, 2) * laneWidth) / laneWidth) * laneWidth;
            }

            float lateralError = targetLane - transform.position.x;
            float steer = Mathf.Clamp(lateralError * steeringResponsiveness, -1f, 1f);
            Vector3 velocity = body.linearVelocity;
            Vector3 lateral = transform.right * Vector3.Dot(velocity, transform.right);
            body.AddForce(-lateral * lateralGrip, ForceMode.Acceleration);

            float yaw = steer * Mathf.Clamp01(speed / 12f) * 1.8f * Time.fixedDeltaTime;
            body.MoveRotation(body.rotation * Quaternion.Euler(0f, yaw * Mathf.Rad2Deg, 0f));
        }
    }
}
