using UnityEngine;
using NeonApocalypse.Combat;
using NeonApocalypse.Core;
using NeonApocalypse.World.OpenWorld;
using NeonApocalypse.AI.Civilians;

namespace NeonApocalypse.Vehicles
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Health))]
    public sealed class VehicleController : MonoBehaviour
    {
        [Header("Arcade Physics")]
        public float engineForce = 10500f;
        public float reverseForce = 5200f;
        public float maxSpeed = 33f;
        public float steeringTorque = 5.2f;
        public float lateralGrip = 7f;
        public float downforce = 38f;
        public float brakeDrag = 1.8f;
        public Transform driverSeat;
        public Transform exitPoint;
        public Transform cameraTarget;
        public bool playerControlled;

        private Rigidbody body;
        private Health health;
        private float throttle;
        private float steer;
        private bool brake;
        private VehicleEnterExit interaction;
        private const float InputDeadzone = 0.04f;

        public float SpeedKph => body ? body.velocity.magnitude * 3.6f : 0f;
        public bool IsOccupied { get; set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            health = GetComponent<Health>();
            interaction = gameObject.GetComponent<VehicleEnterExit>();
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.centerOfMass = new Vector3(0f, -0.45f, 0f);
            if (health) health.maxHealth = Mathf.Max(health.maxHealth, 900f);
        }

        public void SetPlayerControlled(bool value)
        {
            playerControlled = value;
            if (!value) { throttle = 0f; steer = 0f; brake = false; }
        }

        public void SetInputs(float throttleInput, float steerInput, bool brakeInput)
        {
            throttle = Mathf.Clamp(throttleInput, -1f, 1f);
            steer = Mathf.Clamp(steerInput, -1f, 1f);
            brake = brakeInput;
        }

        private void FixedUpdate()
        {
            if (!body) return;
            if (playerControlled)
            {
                float rawThrottle = Input.GetAxisRaw("Vertical");
                float rawSteer = Input.GetAxisRaw("Horizontal");
                bool rawBrake = Input.GetKey(KeyCode.Space);
                if (Mathf.Abs(rawThrottle) > InputDeadzone || Mathf.Abs(rawSteer) > InputDeadzone || rawBrake)
                    SetInputs(rawThrottle, rawSteer, rawBrake);
            }

            ApplyPhysics();
        }

        private void OnCollisionEnter(Collision collision)
        {
            float impact = collision.relativeVelocity.magnitude;
            if (impact > 8f && health) health.Damage(Mathf.Clamp((impact - 8f) * 8f, 0f, 140f));
            if (playerControlled && collision.collider && collision.collider.GetComponentInParent<CivilianAgent>())
            {
                WantedSystem.Instance?.AddHeat(0.65f);
                CrimeEvidenceSystem.Instance?.Report(EvidenceType.VehicleImpact, collision.GetContact(0).point, 0.65f);
            }
        }

        private void ApplyPhysics()
        {
            Vector3 localVelocity = transform.InverseTransformDirection(body.velocity);
            float forwardSpeed = localVelocity.z;
            float speedRatio = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / maxSpeed);

            if (Mathf.Abs(throttle) > InputDeadzone)
            {
                float force = throttle >= 0f ? engineForce : reverseForce;
                if (Mathf.Sign(throttle) != Mathf.Sign(forwardSpeed) && Mathf.Abs(forwardSpeed) > 2f)
                    force *= 0.35f;
                body.AddForce(transform.forward * (throttle * force), ForceMode.Force);
            }

            float steerAuthority = Mathf.Lerp(1f, 0.35f, speedRatio);
            float yaw = steer * steeringTorque * Mathf.Clamp01(body.velocity.magnitude / 2f) * steerAuthority;
            body.AddTorque(Vector3.up * yaw, ForceMode.Acceleration);

            Vector3 lateral = transform.right * Vector3.Dot(body.velocity, transform.right);
            body.AddForce(-lateral * lateralGrip, ForceMode.Acceleration);
            body.AddForce(Vector3.down * (body.velocity.magnitude * downforce), ForceMode.Force);

            if (brake) body.velocity *= 1f / (1f + brakeDrag * Time.fixedDeltaTime);

            float max = maxSpeed;
            if (body.velocity.sqrMagnitude > max * max)
                body.velocity = body.velocity.normalized * max;
        }
    }
}
