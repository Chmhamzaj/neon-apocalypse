using UnityEngine;

namespace NeonApocalypse.World
{
    public class SimpleThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 4.5f, -7f);
        public float sensitivity = 130f;
        public float pitch = 8f;
        public float minPitch = -10f;
        public float maxPitch = 45f;
        public float followSharpness = 18f;
        public float collisionRadius = 0.22f;
        public float collisionPadding = 0.15f;
        public LayerMask collisionMask = ~0;

        private float yaw;
        private Vector3 velocity;
        private Vector3 smoothedPivot;

        public void SetTarget(Transform newTarget, Vector3? newOffset = null)
        {
            target = newTarget;
            if (newOffset.HasValue) offset = newOffset.Value;
            if (target) smoothedPivot = target.position + Vector3.up * 1.25f;
        }

        private void Start()
        {
            if (!target) target = GameObject.FindGameObjectWithTag("Player")?.transform;
            yaw = transform.eulerAngles.y;
            velocity = Vector3.zero;
            collisionMask = Physics.DefaultRaycastLayers;
            smoothedPivot = target ? target.position + Vector3.up * 1.25f : Vector3.zero;
        }

        private void LateUpdate()
        {
            if (!target) return;
            yaw += Input.GetAxis("Mouse X") * sensitivity * Time.unscaledDeltaTime;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity * 0.45f * Time.unscaledDeltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 rawPivot = target.position + Vector3.up * 1.25f;
            float pivotBlend = 1f - Mathf.Exp(-24f * Time.unscaledDeltaTime);
            smoothedPivot = Vector3.Lerp(smoothedPivot, rawPivot, pivotBlend);
            Vector3 pivot = smoothedPivot;
            Vector3 desired = pivot + rotation * offset;

            Vector3 ray = desired - pivot;
            float distance = ray.magnitude;
            if (distance > 0.01f && Physics.SphereCast(pivot, collisionRadius, ray.normalized, out RaycastHit hit, distance, collisionMask, QueryTriggerInteraction.Ignore))
                desired = hit.point - ray.normalized * collisionPadding;

            float blend = 1f - Mathf.Exp(-followSharpness * Time.unscaledDeltaTime);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, Mathf.Max(0.01f, 1f / Mathf.Max(1f, followSharpness)), 100f, Time.unscaledDeltaTime);
            Quaternion look = Quaternion.LookRotation(pivot - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, blend);
        }
    }
}
