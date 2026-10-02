using System;
using System.Collections.Generic;
using UnityEngine;
using NeonApocalypse.World.OpenWorld;

namespace NeonApocalypse.AI
{
    public enum NPCJob { Civilian, Fixer, Medic, Engineer, Guard, Driver, Smuggler, Officer, Scout }
    public enum NPCDisposition { Calm, Alert, Hostile, Coward, Loyal }

    public sealed class NPCMemory : MonoBehaviour
    {
        [Range(-100,100)] public int trust;
        [Range(-100,100)] public int fear;
        [Range(-100,100)] public int loyalty;
        public Vector3 lastThreatPosition;
        public float lastThreatTime = -999f;
        public FactionId faction = FactionId.FreeRunners;
        public NPCJob job = NPCJob.Civilian;
        public NPCDisposition disposition = NPCDisposition.Calm;

        public void RememberThreat(Vector3 position, float danger)
        {
            lastThreatPosition = position;
            lastThreatTime = Time.time;
            fear = Mathf.Clamp(fear + Mathf.RoundToInt(danger * 18f), 0, 100);
        }

        public float MemoryStrength(float halfLife = 45f)
        {
            if (lastThreatTime < 0f) return 0f;
            return Mathf.Pow(0.5f, Mathf.Max(0f, Time.time - lastThreatTime) / Mathf.Max(1f, halfLife));
        }

        public void DailyUpdate(float stress)
        {
            fear = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(fear, 0, 0.02f + stress * 0.02f)), 0, 100);
            loyalty = Mathf.Clamp(loyalty + (disposition == NPCDisposition.Loyal ? 1 : 0), -100, 100);
        }
    }

    public sealed class NPCDailyRoutine : MonoBehaviour
    {
        public float decisionEvery = 4f;
        public float workStart = 8f;
        public float workEnd = 17f;
        public float sleepStart = 23f;
        public float sleepEnd = 6f;
        public float radius = 22f;

        private NPCMemory memory;
        private Transform player;
        private Vector3 home;
        private float nextDecision;
        private int routineSeed;

        private void Awake()
        {
            memory = GetComponent<NPCMemory>() ?? gameObject.AddComponent<NPCMemory>();
            home = transform.position;
            routineSeed = Mathf.Abs(GetInstanceID() * 31);
        }

        public void Initialize(Transform playerTarget, int seed)
        {
            player = playerTarget;
            routineSeed = seed == 0 ? routineSeed : seed;
            home = transform.position;
        }

        private void Update()
        {
            if (Time.time < nextDecision || !gameObject.activeInHierarchy) return;
            nextDecision = Time.time + decisionEvery;
            if (!player && NeonApocalypse.Core.GameManager.Instance) player = NeonApocalypse.Core.GameManager.Instance.PlayerTransform;
            SimulateSchedule();
        }

        private void SimulateSchedule()
        {
            float hour = (Time.time / 60f) % 24f;
            bool sleeping = hour >= sleepStart || hour < sleepEnd;
            bool working = hour >= workStart && hour < workEnd;
            float danger = WantedSystem.Instance ? WantedSystem.Instance.WantedLevel / 5f : 0f;

            memory.DailyUpdate(danger);

            Vector3 destination = home;
            if (sleeping)
            {
                destination = home;
            }
            else if (working)
            {
                float angle = Mathf.Abs((routineSeed % 360) + Mathf.FloorToInt(hour) * 13) * Mathf.Deg2Rad;
                destination = home + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            }
            else
            {
                float angle = Mathf.Abs((routineSeed % 360) + Mathf.FloorToInt(hour) * 29) * Mathf.Deg2Rad;
                destination = home + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (radius * 0.45f);
            }

            float playerDistance = player ? Vector3.Distance(transform.position, player.position) : 999f;
            float memoryStrength = memory.MemoryStrength();
            if (playerDistance < 18f && (memory.fear > 40 || danger > 0.6f))
                destination += (transform.position - player.position).normalized * 14f;
            else if (memoryStrength > 0.35f)
                destination = Vector3.Lerp(destination, memory.lastThreatPosition, memoryStrength * 0.12f);

            Vector3 to = destination - transform.position; to.y = 0f;
            if (to.sqrMagnitude > 9f)
            {
                Vector3 dir = to.normalized;
                transform.position += dir * Time.deltaTime * (sleeping ? 1.0f : 1.45f);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 0.12f);
            }
        }
    }

    public sealed class NPCRumorNetwork : MonoBehaviour
    {
        public static NPCRumorNetwork Instance { get; private set; }
        public struct Rumor
        {
            public Vector3 position;
            public float danger;
            public FactionId faction;
            public float createdAt;
        }

        private readonly List<Rumor> rumors = new List<Rumor>(32);
        public int maxRumors = 24;
        public float rumorLifetime = 90f;

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Broadcast(Vector3 position, float danger, FactionId faction)
        {
            rumors.Add(new Rumor { position = position, danger = danger, faction = faction, createdAt = Time.time });
            if (rumors.Count > maxRumors) rumors.RemoveAt(0);
        }

        public bool TryGetNearby(Vector3 position, float radius, out Rumor best)
        {
            best = default(Rumor);
            float bestScore = 0f;
            for (int i = rumors.Count - 1; i >= 0; i--)
            {
                Rumor r = rumors[i];
                if (Time.time - r.createdAt > rumorLifetime) { rumors.RemoveAt(i); continue; }
                float d = Vector3.Distance(position, r.position);
                if (d > radius) continue;
                float strength = r.danger * Mathf.Pow(0.5f, (Time.time - r.createdAt) / 35f) * (1f - d / radius);
                if (strength > bestScore) { bestScore = strength; best = r; }
            }
            return bestScore > 0.12f;
        }
    }

    public sealed class FactionAIDirector : MonoBehaviour
    {
        public static FactionAIDirector Instance { get; private set; }
        private Transform player;
        private float nextTick;
        private readonly List<EnemyAI> enemies = new List<EnemyAI>(64);

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() => player = GameObject.FindGameObjectWithTag("Player")?.transform;

        public void Register(EnemyAI ai)
        {
            if (ai && !enemies.Contains(ai)) enemies.Add(ai);
            EnsureProfile(ai);
        }

        public void Unregister(EnemyAI ai) => enemies.Remove(ai);

        private void Update()
        {
            if (Time.time < nextTick) return;
            nextTick = Time.time + 0.8f;
            if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                EnemyAI ai = enemies[i];
                if (!ai) { enemies.RemoveAt(i); continue; }
                NPCMemory mem = EnsureProfile(ai);
                if (FactionTerritoryDirector.Instance?.CurrentTerritory != null)
                    mem.faction = FactionTerritoryDirector.Instance.CurrentTerritory.faction;
                ApplyAdaptiveTactics(ai, mem);
            }
        }

        private NPCMemory EnsureProfile(EnemyAI ai)
        {
            if (!ai) return null;
            NPCMemory mem = ai.GetComponent<NPCMemory>() ?? ai.gameObject.AddComponent<NPCMemory>();
            return mem;
        }

        private static void ApplyAdaptiveTactics(EnemyAI ai, NPCMemory mem)
        {
            if (!ai || mem == null) return;
            switch (mem.faction)
            {
                case FactionId.HelixAuthority:
                    ai.detectionRange = 44f; ai.thinkInterval = 0.16f; ai.attackCooldown = Mathf.Max(0.45f, ai.attackCooldown * 0.98f); break;
                case FactionId.ChromeSerpents:
                    ai.detectionRange = 39f; ai.thinkInterval = 0.18f; ai.moveSpeed *= 0.985f; break;
                case FactionId.NullCult:
                    ai.detectionRange = 34f + mem.fear * 0.04f; ai.thinkInterval = 0.20f; break;
                case FactionId.FreeRunners:
                    ai.detectionRange = 37f; ai.thinkInterval = 0.15f; break;
            }
            ai.thinkInterval = Mathf.Clamp(ai.thinkInterval, 0.12f, 0.28f);
        }
    }

    public sealed class CompanionAI : MonoBehaviour
    {
        public enum Command { Follow, Hold, AttackTarget, Defend }
        private static readonly List<CompanionAI> Registry = new List<CompanionAI>(16);
        public static IReadOnlyList<CompanionAI> ActiveCompanions => Registry;
        public Command command = Command.Follow;
        public float followDistance = 4.5f;
        public float thinkEvery = 0.16f;
        public float attackRange = 18f;
        public float moveSpeed = 4.2f;

        private Transform player;
        private Transform target;
        private float nextThink;
        private NPCMemory memory;

        private void Awake()
        {
            memory = GetComponent<NPCMemory>() ?? gameObject.AddComponent<NPCMemory>();
        }

        private void OnEnable() { if (!Registry.Contains(this)) Registry.Add(this); }
        private void OnDisable() { Registry.Remove(this); }

        public Command CurrentCommand => command;

        public void Initialize(Transform playerTarget) { player = playerTarget; }
        public void SetCommand(Command next, Transform targetOverride = null) { command = next; target = targetOverride; }

        private void Update()
        {
            if (Time.time < nextThink) return;
            nextThink = Time.time + thinkEvery;
            if (!player) player = NeonApocalypse.Core.GameManager.Instance?.PlayerTransform;
            if (!player) return;

            if (command == Command.AttackTarget && !target)
                target = FindNearestHostile();

            Vector3 destination = command == Command.Hold ? transform.position : player.position;
            if (command == Command.Follow)
                destination = player.position - player.forward * followDistance;
            else if (command == Command.Defend)
                destination = player.position + (transform.position - player.position).normalized * 7f;
            else if (command == Command.AttackTarget && target)
                destination = target.position;

            Vector3 to = destination - transform.position; to.y = 0f;
            if (to.sqrMagnitude > 4f)
            {
                Vector3 dir = to.normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 0.18f);
            }
            if (target && Vector3.Distance(transform.position, target.position) <= attackRange)
            {
                Health h = target.GetComponentInParent<Health>();
                if (h) h.Damage(16f * Time.deltaTime * 5f);
            }
            if (memory != null && target) memory.RememberThreat(target.position, 0.5f);
        }

        private readonly Collider[] hostileBuffer = new Collider[32];

        private Transform FindNearestHostile()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, attackRange, hostileBuffer, ~0, QueryTriggerInteraction.Ignore);
            Transform best = null; float bestD = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                Collider hit = hostileBuffer[i];
                if (!hit) continue;
                Transform t = hit.transform.root;
                if (!t || t == transform.root || t.CompareTag("Player")) continue;
                EnemyAI e = t.GetComponentInChildren<EnemyAI>();
                if (!e) continue;
                float d = (t.position - transform.position).sqrMagnitude;
                if (d < bestD) { bestD = d; best = t; }
            }
            return best;
        }
    }
}
