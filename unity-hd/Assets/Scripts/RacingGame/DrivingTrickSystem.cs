using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class DrivingTrickSystem : MonoBehaviour
    {
        [SerializeField] private Rigidbody body;
        [SerializeField] private CarController car;
        [SerializeField] private CameraShake cameraShake;
        [SerializeField] private float jumpHeightThreshold = 0.35f;
        [SerializeField] private float minimumJumpSpeedKph = 55f;
        [SerializeField] private float driftSpeedKph = 65f;
        [SerializeField] private float driftAngle = 12f;

        public bool Airborne { get; private set; }
        public bool Drifting { get; private set; }
        private bool wasAirborne;
        private int groundedFrames;

        private void Awake()
        {
            if (!body) body = GetComponent<Rigidbody>();
            if (!car) car = GetComponent<CarController>();
            if (!cameraShake) cameraShake = FindFirstObjectByType<CameraShake>();
        }

        private void FixedUpdate()
        {
            if (!body || !car) return;
            bool ground = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, jumpHeightThreshold);
            if (ground) groundedFrames = Mathf.Min(groundedFrames + 1, 5); else groundedFrames = 0;
            Airborne = groundedFrames == 0 && car.SpeedKph >= minimumJumpSpeedKph;
            float forward = Vector3.Dot(body.linearVelocity.normalized, transform.forward);
            float lateral = Vector3.Dot(body.linearVelocity.normalized, transform.right);
            float angle = Mathf.Atan2(Mathf.Abs(lateral), Mathf.Max(0.01f, forward)) * Mathf.Rad2Deg;
            Drifting = !Airborne && car.SpeedKph >= driftSpeedKph && angle >= driftAngle;
            if (Airborne && !wasAirborne) cameraShake?.Impact(0.25f);
            wasAirborne = Airborne;
        }
    }
}
