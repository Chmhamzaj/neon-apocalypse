using UnityEngine;

namespace NitroStreetRush.Racing
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TrafficVehicleAI : MonoBehaviour
    {
        [SerializeField] private float cruiseSpeedKph = 115f;
        [SerializeField] private float speedVariationKph = 22f;
        [SerializeField] private float laneChangeChance = 0.08f;
        [SerializeField] private float laneWidth = 3.5f;
        [SerializeField] private float steeringResponsiveness = 3.2f;
        [SerializeField] private float lateralGrip = 8f;
        private Rigidbody body;
        private float targetSpeed;
        private float targetLane;
        private float laneTimer;
        private RaceSplinePath path;
        private float pathDistance;
        private float laneOffset;

        public void SetPath(RaceSplinePath newPath, float distance, float offset)
        {
            path = newPath;
            pathDistance = Mathf.Max(0f, distance);
            laneOffset = offset;
            targetLane = offset;
        }

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

            if (path && path.TotalLength > 1f)
            {
                pathDistance += speed * Time.fixedDeltaTime;
                if (pathDistance > path.TotalLength) pathDistance = 0f;
                Vector3 roadPoint = path.GetPoint(pathDistance);
                Vector3 tangent = path.GetTangent(pathDistance);
                Vector3 right = path.GetRight(pathDistance);
                Vector3 targetPoint = roadPoint + right * targetLane;
                Vector3 toTarget = targetPoint - transform.position;
                float lateralError = Vector3.Dot(toTarget, right);
                Vector3 desiredForward = (tangent + right * Mathf.Clamp(lateralError * 0.08f, -0.75f, 0.75f)).normalized;
                float signedTurn = Vector3.SignedAngle(transform.forward, desiredForward, Vector3.up);
                float steer = Mathf.Clamp(signedTurn / 30f, -1f, 1f);
                Vector3 lateral = transform.right * Vector3.Dot(body.linearVelocity, transform.right);
                body.AddForce(-lateral * lateralGrip, ForceMode.Acceleration);
                float yaw = steer * steeringResponsiveness * Mathf.Clamp01(speed / 8f) * Time.fixedDeltaTime;
                body.MoveRotation(body.rotation * Quaternion.Euler(0f, yaw * Mathf.Rad2Deg, 0f));
                return;
            }

            laneTimer -= Time.fixedDeltaTime;
            if (laneTimer <= 0f)
            {
                laneTimer = Random.Range(3f, 7f);
                if (Random.value < laneChangeChance)
                    targetLane = Mathf.Round((transform.position.x + Random.Range(-1, 2) * laneWidth) / laneWidth) * laneWidth;
            }
            float error = targetLane - transform.position.x;
            float localSteer = Mathf.Clamp(error * steeringResponsiveness, -1f, 1f);
            Vector3 localLateral = transform.right * Vector3.Dot(body.linearVelocity, transform.right);
            body.AddForce(-localLateral * lateralGrip, ForceMode.Acceleration);
            float localYaw = localSteer * Mathf.Clamp01(speed / 12f) * 1.8f * Time.fixedDeltaTime;
            body.MoveRotation(body.rotation * Quaternion.Euler(0f, localYaw * Mathf.Rad2Deg, 0f));
        }
    }
}
