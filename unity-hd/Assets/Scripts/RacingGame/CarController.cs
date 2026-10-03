using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NitroStreetRush.Racing
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CarController : MonoBehaviour
    {
        [Header("Handling")]
        [SerializeField] private float maxSpeedKph = 320f;
        [SerializeField] private float acceleration = 42f;
        [SerializeField] private float brakeForce = 70f;
        [SerializeField] private float steerRate = 2.6f;
        [SerializeField] private float lateralGrip = 7.5f;
        [SerializeField] private float driftGrip = 2.2f;
        [SerializeField] private float downforce = 1.8f;

        [Header("Nitro")]
        [SerializeField] private float nitroCapacity = 100f;
        [SerializeField] private float nitroAcceleration = 95f;
        [SerializeField] private float nitroDrainPerSecond = 26f;
        [SerializeField] private float nitroRechargePerSecond = 7f;

        private Rigidbody body;
        private float steerInput;
        private float throttleInput = 1f;
        private bool brakeInput;
        private bool nitroInput;
        private float nitro;

        public float SpeedKph => body.linearVelocity.magnitude * 3.6f;
        public float Nitro => nitro;
        public float NitroCapacity => nitroCapacity;
        public bool IsBoosting => nitroInput && nitro > 0.5f && throttleInput > 0f;
        public bool IsBraking => brakeInput;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.mass = 1420f;
            body.linearDamping = 0.08f;
            body.angularDamping = 3.5f;
            nitro = nitroCapacity;
        }

        public void ApplyNitroMultiplier(float multiplier)
        {
            multiplier = Mathf.Max(1f, multiplier);
            float normalized = nitroCapacity > 0f ? nitro / nitroCapacity : 1f;
            nitroCapacity = 100f * multiplier;
            nitro = Mathf.Clamp(normalized * nitroCapacity, 0f, nitroCapacity);
        }

        private void FixedUpdate()
        {
            ReadInput();
            Vector3 velocity = body.linearVelocity;
            float forwardSpeed = Vector3.Dot(velocity, transform.forward);
            float maxSpeed = maxSpeedKph / 3.6f;

            float driveForce = acceleration * throttleInput;
            if (IsBoosting)
            {
                driveForce += nitroAcceleration;
                nitro = Mathf.Max(0f, nitro - nitroDrainPerSecond * Time.fixedDeltaTime);
            }
            else nitro = Mathf.Min(nitroCapacity, nitro + nitroRechargePerSecond * Time.fixedDeltaTime);

            if (forwardSpeed < maxSpeed) body.AddForce(transform.forward * driveForce, ForceMode.Acceleration);
            if (brakeInput && velocity.sqrMagnitude > 0.01f) body.AddForce(-velocity.normalized * brakeForce, ForceMode.Acceleration);

            float grip = Mathf.Abs(steerInput) > 0.65f ? driftGrip : lateralGrip;
            Vector3 lateralVelocity = transform.right * Vector3.Dot(velocity, transform.right);
            body.AddForce(-lateralVelocity * grip, ForceMode.Acceleration);

            float steerScale = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / 18f);
            float yaw = steerInput * steerRate * steerScale * Time.fixedDeltaTime;
            body.MoveRotation(body.rotation * Quaternion.Euler(0f, yaw * Mathf.Rad2Deg, 0f));
            body.AddForce(-transform.up * downforce * velocity.sqrMagnitude, ForceMode.Force);
        }

        private void ReadInput()
        {
            steerInput = 0f;
            brakeInput = false;
            nitroInput = false;

#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                steerInput = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
                brakeInput = keyboard.spaceKey.isPressed;
                nitroInput = keyboard.leftShiftKey.isPressed;
            }
#else
            steerInput = Mathf.Clamp(Input.GetAxisRaw("Horizontal"), -1f, 1f);
            brakeInput = Input.GetKey(KeyCode.Space);
            nitroInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.JoystickButton0);
#endif

            if (TouchInput.Instance)
            {
                steerInput = TouchInput.Instance.Steer;
                brakeInput |= TouchInput.Instance.BrakeHeld;
                nitroInput |= TouchInput.Instance.NitroHeld;
            }

            var tilt = FindFirstObjectByType<MobileSteering>();
            if (Mathf.Abs(steerInput) < 0.05f && tilt) steerInput = tilt.Value;
        }
    }
}
