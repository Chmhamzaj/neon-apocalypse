using System.Collections.Generic;
using UnityEngine;
using NeonApocalypse.AI;
using NeonApocalypse.World.OpenWorld;

namespace NeonApocalypse.AI.Civilians
{
    public sealed class CivilianAgent : MonoBehaviour
    {
        private enum CivilianState { Wander, Observe, Panic, Evacuate, Hide, Recover }

        public float walkSpeed = 1.8f;
        public float wanderRadius = 35f;
        public float perceptionRange = 42f;
        public float decisionEvery = 0.18f;

        private static readonly List<CivilianAgent> Registry = new List<CivilianAgent>(64);
        private Transform player;
        private Vector3 destination;
        private Vector3 threatPosition;
        private float nextDecision;
        private float panicUntil;
        private int seed;
        private CivilianState state;
        private NPCMemory memory;
        private NPCDailyRoutine routine;

        private void Awake()
        {
            memory = GetComponent<NPCMemory>() ?? gameObject.AddComponent<NPCMemory>();
            routine = GetComponent<NPCDailyRoutine>() ?? gameObject.AddComponent<NPCDailyRoutine>();
            memory.job = NPCJob.Civilian;
        }

        public void Initialize(Transform playerTarget, int randomSeed)
        {
            player = playerTarget;
            seed = randomSeed;
            state = CivilianState.Wander;
            panicUntil = 0f;
            ChooseDestination(true);
            routine?.Initialize(playerTarget, randomSeed);
        }

        private void OnEnable() { if (!Registry.Contains(this)) Registry.Add(this); }
        private void OnDisable() { Registry.Remove(this); }

        private void Update()
        {
            if (!player || Time.time < nextDecision) return;
            nextDecision = Time.time + decisionEvery;
            SenseAndThink();
            Act();
            if (Vector3.SqrMagnitude(transform.position - player.position) > 190f * 190f) gameObject.SetActive(false);
        }

        private void SenseAndThink()
        {
            if (AIStimulusBus.TryGetBest(transform.position, 2.5f, perceptionRange, out AIStimulus stimulus) && stimulus.danger > 0.45f)
            {
                threatPosition = stimulus.position;
                memory?.RememberThreat(threatPosition, stimulus.danger);
                NPCRumorNetwork.Instance?.Broadcast(threatPosition, stimulus.danger, memory != null ? memory.faction : FactionId.FreeRunners);
                panicUntil = Time.time + 5f + stimulus.danger * 5f;
                state = CivilianState.Panic;
                WantedSystem.Instance?.AddHeat(0.035f);
                return;
            }

            if (state == CivilianState.Panic || state == CivilianState.Evacuate || state == CivilianState.Hide)
            {
                if (Time.time < panicUntil) return;
                state = CivilianState.Recover;
            }
            if (Vector3.Distance(transform.position, player.position) < 22f && WantedSystem.Instance && WantedSystem.Instance.WantedLevel >= 3)
            {
                threatPosition = player.position;
                panicUntil = Time.time + 4f;
                state = CivilianState.Panic;
                return;
            }
            if (state == CivilianState.Recover && Vector3.Distance(transform.position, destination) < 2f)
                state = CivilianState.Wander;
        }

        private void Act()
        {
            switch (state)
            {
                case CivilianState.Wander:
                    if (Vector3.Distance(transform.position, destination) < 2f) ChooseDestination(false);
                    MoveTowards(destination, walkSpeed);
                    break;
                case CivilianState.Panic:
                    ChooseEvacuationRoute();
                    state = CivilianState.Evacuate;
                    break;
                case CivilianState.Evacuate:
                    MoveTowards(destination, walkSpeed * 1.85f);
                    if (Time.time + 0.2f >= panicUntil && Vector3.Distance(transform.position, destination) < 4f)
                        state = CivilianState.Hide;
                    break;
                case CivilianState.Hide:
                    MoveTowards(destination, walkSpeed * 0.45f);
                    break;
                case CivilianState.Recover:
                    MoveTowards(destination, walkSpeed * 1.1f);
                    break;
                case CivilianState.Observe:
                    FacePoint(threatPosition);
                    break;
            }
        }

        private void ChooseDestination(bool immediate)
        {
            seed = unchecked(seed * 1103515245 + 12345);
            float a = Mathf.Abs(seed % 6283) / 1000f;
            seed = unchecked(seed * 1103515245 + 12345);
            float r = 8f + Mathf.Abs(seed % 2800) / 100f;
            Vector3 center = player ? player.position : transform.position;
            destination = center + new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
            nextDecision = Time.time + (immediate ? 0.5f : 2.5f + Mathf.Abs(seed % 200) / 100f);
        }

        private void ChooseEvacuationRoute()
        {
            Vector3 away = (transform.position - threatPosition);
            away.y = 0f;
            if (away.sqrMagnitude < 1f) away = transform.position - player.position;
            if (away.sqrMagnitude < 1f) away = transform.forward;
            away.Normalize();

            Vector3 best = transform.position + away * 24f;
            float bestScore = float.MinValue;
            for (int i = -3; i <= 3; i++)
            {
                Vector3 dir = Quaternion.Euler(0f, i * 28f, 0f) * away;
                Vector3 candidate = transform.position + dir * 22f;
                if (Physics.Raycast(transform.position + Vector3.up, dir, 18f, ~0, QueryTriggerInteraction.Ignore)) continue;
                float separation = 0f;
                for (int j = 0; j < Registry.Count && j < 48; j++)
                {
                    var other = Registry[j];
                    if (!other || other == this || !other.gameObject.activeSelf) continue;
                    float d = Vector3.Distance(candidate, other.transform.position);
                    if (d < 4f) separation -= (4f - d);
                }
                float score = Vector3.Dot(dir, away) * 5f + separation;
                if (score > bestScore) { bestScore = score; best = candidate; }
            }
            destination = best;
        }

        private void MoveTowards(Vector3 point, float speed)
        {
            Vector3 to = point - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude < 0.05f) return;
            Vector3 dir = to.normalized;
            transform.position += dir * speed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 1f - Mathf.Exp(-7f * Time.deltaTime));
        }

        private void FacePoint(Vector3 point)
        {
            Vector3 to = point - transform.position; to.y = 0f;
            if (to.sqrMagnitude > 0.01f) transform.forward = Vector3.Slerp(transform.forward, to.normalized, 0.4f);
        }
    }
}
