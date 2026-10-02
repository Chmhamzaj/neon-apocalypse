using UnityEngine;
using NeonApocalypse.AI;

namespace NeonApocalypse.World.OpenWorld
{
    public sealed class PolicePursuitAgent : MonoBehaviour
    {
        private enum PursuitState { Pursue, Intercept, Search }
        public float speed = 15f;
        public float turnSharpness = 3.2f;
        public float brakingDistance = 18f;
        public float thinkEvery = 0.12f;

        private Transform target;
        private PursuitState state;
        private Vector3 lastSeen;
        private Vector3 previousTarget;
        private float nextThink;
        private float lostSince;

        public void Initialize(Transform player)
        {
            target = player;
            previousTarget = player ? player.position : transform.position;
            lastSeen = previousTarget;
            state = PursuitState.Pursue;
            lostSince = 0f;
        }

        private void Update()
        {
            if (!target || !gameObject.activeSelf) return;
            if (Time.time < nextThink) return;
            nextThink = Time.time + thinkEvery;
            Think();
            Move();
        }

        private void Think()
        {
            Vector3 current = target.position;
            Vector3 velocity = (current - previousTarget) / Mathf.Max(thinkEvery, 0.02f);
            previousTarget = current;
            Vector3 predicted = current + Vector3.ClampMagnitude(velocity, 16f) * 0.35f;
            Vector3 rayOrigin = transform.position + Vector3.up;
            Vector3 rayDelta = current + Vector3.up - rayOrigin;
            bool visible = !Physics.Raycast(rayOrigin, rayDelta.normalized, out RaycastHit hit, rayDelta.magnitude, ~0, QueryTriggerInteraction.Ignore) ||
                           hit.transform == target || (target && hit.transform.IsChildOf(target));

            if (visible)
            {
                lastSeen = current;
                lostSince = 0f;
                state = Vector3.Distance(transform.position, predicted) > 45f ? PursuitState.Intercept : PursuitState.Pursue;
            }
            else
            {
                if (lostSince <= 0f) lostSince = Time.time;
                state = Time.time - lostSince < 2.2f ? PursuitState.Intercept : PursuitState.Search;
            }
            if (visible) AIStimulusBus.Report(current, 70f, 0.5f, AIStimulusType.Alarm);
        }

        private void Move()
        {
            Vector3 destination = lastSeen;
            if (state == PursuitState.Intercept)
            {
                Vector3 toTarget = target.position - transform.position;
                Vector3 forward = toTarget.sqrMagnitude > 0.1f ? toTarget.normalized : transform.forward;
                destination = target.position + forward * Mathf.Clamp(Vector3.Distance(transform.position, target.position) * 0.18f, 3f, 16f);
            }
            else if (state == PursuitState.Pursue)
            {
                destination = target.position;
            }

            Vector3 to = destination - transform.position;
            to.y = 0f;
            float distance = to.magnitude;
            if (distance <= 1f) return;
            Vector3 dir = to / Mathf.Max(0.001f, distance);
            Quaternion desired = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desired, 1f - Mathf.Exp(-turnSharpness * Time.deltaTime));
            float desiredSpeed = state == PursuitState.Search ? speed * 0.72f : speed;
            float applied = distance < brakingDistance ? Mathf.Lerp(2f, desiredSpeed, distance / brakingDistance) : desiredSpeed;
            transform.position += transform.forward * applied * Time.deltaTime;
        }
    }
}
