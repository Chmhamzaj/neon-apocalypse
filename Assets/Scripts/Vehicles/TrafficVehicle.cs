using UnityEngine;

namespace NeonApocalypse.Vehicles
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TrafficVehicle : MonoBehaviour
    {
        public float cruiseSpeed = 12f;
        public float steeringSharpness = 5f;
        public bool loopX = true;
        private Vector3 target;
        private bool initialized;
        private int direction;
        private float despawnDistance = 260f;
        private float baseCruiseSpeed = 12f;
        private float scheduleMultiplier = 1f;
        private Transform player;
        private Rigidbody body;

        public void Initialize(Transform playerTarget, int seed)
        {
            player = playerTarget;
            body = GetComponent<Rigidbody>();
            direction = (seed & 1) == 0 ? 1 : -1;
            baseCruiseSpeed = 10f + Mathf.Abs(seed % 100) * 0.09f;
            cruiseSpeed = baseCruiseSpeed;
            loopX = (seed & 2) == 0;
            initialized = true;
            target = transform.position + (loopX ? Vector3.right : Vector3.forward) * direction * 100f;
        }

        public void SetScheduleMultiplier(float multiplier)
        {
            scheduleMultiplier = Mathf.Clamp(multiplier, 0.45f, 1.3f);
            cruiseSpeed = baseCruiseSpeed * scheduleMultiplier;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            if (body)
            {
                body.isKinematic = true;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }
        }

        private void OnEnable()
        {
            if (initialized)
            {
                cruiseSpeed = baseCruiseSpeed * scheduleMultiplier;
                target = transform.position + (loopX ? Vector3.right : Vector3.forward) * direction * 100f;
            }
        }

        private void FixedUpdate()
        {
            if (!initialized || !body) return;
            Vector3 dir = target - body.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.1f)
            {
                Quaternion desired = Quaternion.LookRotation(dir.normalized, Vector3.up);
                Quaternion nextRot = Quaternion.Slerp(body.rotation, desired, 1f - Mathf.Exp(-steeringSharpness * Time.fixedDeltaTime));
                body.MoveRotation(nextRot);
                body.MovePosition(body.position + nextRot * Vector3.forward * cruiseSpeed * Time.fixedDeltaTime);
            }

            if (Vector3.SqrMagnitude(body.position - target) < 18f * 18f)
                target += (loopX ? Vector3.right : Vector3.forward) * direction * 140f;

            if (player && Vector3.SqrMagnitude(body.position - player.position) > despawnDistance * despawnDistance)
                gameObject.SetActive(false);
        }
    }
}
