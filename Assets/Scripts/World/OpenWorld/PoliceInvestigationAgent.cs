using UnityEngine;
using NeonApocalypse.AI;

namespace NeonApocalypse.World.OpenWorld
{
    /// <summary>
    /// Lightweight investigative police AI: moves to a last-known incident,
    /// performs a bounded search sweep, and escalates into pursuit when it finds the player.
    /// </summary>
    public sealed class PoliceInvestigationAgent : MonoBehaviour
    {
        private enum State { Travel, Sweep, Pursuit, Return }
        public float speed = 9.5f;
        public float pursuitSpeed = 13f;
        public float searchRadius = 16f;
        public float detectionRange = 25f;
        public float thinkEvery = 0.18f;
        private State state;
        private Transform player;
        private Vector3 incident;
        private Vector3 sweepPoint;
        private float nextThink;
        private float sweepStarted;
        private int seed;
        private float desiredHeading;

        public void Initialize(Transform playerTarget, Vector3 incidentPosition, int randomSeed)
        {
            player = playerTarget;
            SetIncident(incidentPosition, randomSeed);
        }

        public void SetIncident(Vector3 position, int randomSeed)
        {
            incident = position;
            seed = randomSeed == 0 ? 17 : randomSeed;
            state = State.Travel;
            sweepPoint = position;
            sweepStarted = 0f;
            nextThink = 0f;
            desiredHeading = Mathf.Abs(seed % 360);
        }

        private void Update()
        {
            if (!player) player = NeonApocalypse.Core.GameManager.Instance?.PlayerTransform;
            if (!player || Time.time < nextThink) return;
            nextThink = Time.time + thinkEvery;
            Think();
            Move();
        }

        private void Think()
        {
            Vector3 delta = player.position - transform.position;
            bool visible = delta.sqrMagnitude <= detectionRange * detectionRange &&
                           !Physics.Raycast(transform.position + Vector3.up, delta.normalized, delta.magnitude,
                               ~0, QueryTriggerInteraction.Ignore);
            if (visible)
            {
                state = State.Pursuit;
                incident = player.position;
                AIStimulusBus.Report(player.position, 70f, 0.7f, AIStimulusType.Alarm);
                return;
            }

            if (state == State.Travel && Vector3.SqrMagnitude(transform.position - incident) < 7f * 7f)
            {
                state = State.Sweep;
                sweepStarted = Time.time;
                sweepPoint = incident;
            }
            else if (state == State.Sweep)
            {
                if (Time.time - sweepStarted > 18f)
                {
                    state = State.Return;
                    return;
                }
                seed = unchecked(seed * 1103515245 + 12345);
                float a = Mathf.Abs(seed % 6283) / 1000f;
                float r = 5f + Mathf.Abs(seed % Mathf.Max(1, Mathf.RoundToInt(searchRadius * 100f))) / 100f;
                sweepPoint = incident + new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
            }
            else if (state == State.Return && Vector3.SqrMagnitude(transform.position - incident) < 25f)
            {
                state = State.Sweep;
                sweepStarted = Time.time;
            }
        }

        private void Move()
        {
            Vector3 destination = state == State.Pursuit ? PredictIntercept() :
                                  state == State.Sweep ? sweepPoint :
                                  state == State.Return ? incident : incident;
            Vector3 to = destination - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude < 0.4f) return;
            Vector3 dir = to.normalized;
            float currentSpeed = state == State.Pursuit ? pursuitSpeed : speed;
            Quaternion desired = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desired, 1f - Mathf.Exp(-5f * Time.deltaTime));
            transform.position += transform.forward * currentSpeed * Time.deltaTime;
        }

        private Vector3 PredictIntercept()
        {
            Vector3 forward = player ? (player.position - transform.position).WithY(0f) : transform.forward;
            if (forward.sqrMagnitude < 0.01f) forward = transform.forward;
            return player.position + forward.normalized * 5f;
        }
    }
}
