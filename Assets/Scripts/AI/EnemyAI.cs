using UnityEngine;
using NeonApocalypse.Combat;
using NeonApocalypse.Core;
using NeonApocalypse.Progression;
using NeonApocalypse.World;
using NeonApocalypse.World.OpenWorld;
using NeonApocalypse.AI.Squad;
using NeonApocalypse.AI.Tactics;

namespace NeonApocalypse.AI
{
    [RequireComponent(typeof(Health))]
    public class EnemyAI : MonoBehaviour
    {
        private enum BrainState { Patrol, Investigate, Engage, Search, Retreat }

        public EnemyArchetype archetype = EnemyArchetype.Grunt;
        public float detectionRange = 36f;
        public float fieldOfView = 135f;
        public float attackRange = 2.2f;
        public float moveSpeed = 3.2f;
        public float damage = 12f;
        public float attackCooldown = 1.1f;
        public float xpReward = 35;
        public float thinkInterval = 0.14f;
        public LayerMask visionMask = ~0;

        private Health health;
        private Transform target;
        private float nextAttack;
        private bool dead;
        private Vector3 spawnOrigin;
        private float phaseOffset;
        private BrainState state;
        private Vector3 lastKnownPosition;
        private Vector3 previousTargetPosition;
        private Vector3 rememberedSearchPoints;
        private float suspicion;
        private float nextThink;
        private float nextCoverCheck;
        private Vector3 tacticalDestination;
        private bool targetVisible;
        private SquadRole role = SquadRole.Rusher;
        private Vector3 coverPoint;
        private bool hasCover;

        private void Awake()
        {
            health = GetComponent<Health>();
            if (!AISquadCoordinator.Instance)
            {
                var go = new GameObject("AI_SquadCoordinator_Runtime");
                go.AddComponent<AISquadCoordinator>();
            }
            health.Died += Die;
            phaseOffset = Random.value * 10f;
        }

        private void OnEnable()
        {
            AISquadCoordinator.Instance?.Register(this);
            FactionAIDirector.Instance?.Register(this);
        }

        private void OnDisable()
        {
            AISquadCoordinator.Instance?.Unregister(this);
            FactionAIDirector.Instance?.Unregister(this);
        }

        public void Initialize(EnemyArchetype type, Transform targetTransform, Vector3 spawnPosition)
        {
            archetype = type;
            target = targetTransform;
            spawnOrigin = spawnPosition;
            previousTargetPosition = target ? target.position : spawnPosition;
            lastKnownPosition = spawnPosition;
            rememberedSearchPoints = spawnPosition;
            suspicion = 0f;
            nextAttack = Time.time + 0.35f;
            nextThink = Time.time;
            nextCoverCheck = Time.time;
            dead = false;
            state = BrainState.Patrol;
            hasCover = false;
            ApplyArchetype();
            health.ResetHealth();
            AISquadCoordinator.Instance?.Register(this);
        }

        private void Update()
        {
            if (dead) return;
            if (!target && GameManager.Instance) target = GameManager.Instance.PlayerTransform;
            if (!target) return;
            if (Time.time < nextThink) return;
            nextThink = Time.time + thinkInterval;

            Sense();
            Think();
            Act();
        }

        private void Sense()
        {
            Vector3 targetPos = target.position;
            Vector3 toTarget = targetPos - transform.position;
            float distanceSqr = toTarget.sqrMagnitude;
            bool inRange = distanceSqr <= detectionRange * detectionRange;
            bool inFov = Vector3.Angle(transform.forward, toTarget) <= fieldOfView * 0.5f || distanceSqr < 16f;
            targetVisible = inRange && inFov && HasLineOfSight(targetPos);

            if (targetVisible)
            {
                lastKnownPosition = targetPos;
                suspicion = Mathf.Min(1f, suspicion + 0.45f);
                rememberedSearchPoints = Vector3.Lerp(rememberedSearchPoints, targetPos, 0.65f);
            }
            else
            {
                suspicion = Mathf.Max(0f, suspicion - 0.055f);
            }

            if (!targetVisible && AIStimulusBus.TryGetBest(transform.position, 2.8f, 42f, out AIStimulus stimulus))
            {
                lastKnownPosition = stimulus.position;
                suspicion = Mathf.Max(suspicion, stimulus.danger * 0.9f);
                if (state == BrainState.Patrol || state == BrainState.Search)
                    state = BrainState.Investigate;
            }
        }

        private void Think()
        {
            AISquadCoordinator.TacticalOrder order;
            if (AISquadCoordinator.Instance && AISquadCoordinator.Instance.TryGetOrder(this, out order))
            {
                role = order.role;
                tacticalDestination = order.destination;
            }

            float healthRatio = health && health.maxHealth > 0f ? health.Current / health.maxHealth : 1f;
            float distance = Vector3.Distance(transform.position, target.position);

            if (healthRatio < 0.20f && archetype != EnemyArchetype.Rusher && suspicion > 0.35f)
            {
                state = BrainState.Retreat;
                return;
            }
            if (targetVisible)
            {
                state = BrainState.Engage;
                if (Time.time >= nextCoverCheck && (role == SquadRole.Suppressor || role == SquadRole.Overwatch || archetype == EnemyArchetype.Gunner))
                {
                    nextCoverCheck = Time.time + 0.85f;
                    hasCover = TryFindCover(target.position, out coverPoint);
                }
                return;
            }
            if (suspicion > 0.45f)
            {
                state = BrainState.Search;
                return;
            }
            state = BrainState.Patrol;
        }

        private void Act()
        {
            switch (state)
            {
                case BrainState.Patrol:
                    Patrol();
                    break;
                case BrainState.Investigate:
                    MoveTowards(lastKnownPosition);
                    FacePoint(lastKnownPosition);
                    break;
                case BrainState.Engage:
                    EngageTarget();
                    break;
                case BrainState.Search:
                    SearchLastKnownPosition();
                    break;
                case BrainState.Retreat:
                    Retreat();
                    break;
            }
        }

        private void ApplyArchetype()
        {
            moveSpeed = 3.2f; damage = 12f; attackCooldown = 1.1f; attackRange = 2.2f; xpReward = 35;
            switch (archetype)
            {
                case EnemyArchetype.Rusher:
                    moveSpeed = 5.5f; damage = 20f; attackCooldown = 0.75f; xpReward = 50; break;
                case EnemyArchetype.Gunner:
                    moveSpeed = 2.7f; damage = 10f; attackCooldown = 0.55f; attackRange = 13f; xpReward = 65; break;
                case EnemyArchetype.Elite:
                    moveSpeed = 3.5f; damage = 25f; attackCooldown = 0.8f; xpReward = 140; detectionRange = 44f; break;
            }
        }

        private void Patrol()
        {
            Vector3 offset = new Vector3(Mathf.Sin(Time.time * 0.4f + phaseOffset), 0, Mathf.Cos(Time.time * 0.35f + phaseOffset)) * 4f;
            MoveTowards(spawnOrigin + offset);
        }

        private void EngageTarget()
        {
            FacePoint(target.position);
            float distance = Vector3.Distance(transform.position, target.position);
            if (role == SquadRole.Rusher)
            {
                MoveTowards(tacticalDestination);
                if (distance <= attackRange + 0.4f) AttackTarget();
                return;
            }
            if (hasCover && role != SquadRole.FlankerLeft && role != SquadRole.FlankerRight)
            {
                MoveTowards(coverPoint);
                if (distance <= attackRange + 11f) AttackTarget();
                return;
            }
            MoveTowards(tacticalDestination);
            if (distance <= (archetype == EnemyArchetype.Gunner ? 14f : attackRange + 1f)) AttackTarget();
        }

        private void AttackTarget()
        {
            if (Time.time < nextAttack) return;
            nextAttack = Time.time + attackCooldown;
            Vector3 targetPoint = PredictTargetPoint();
            Health h = target.GetComponentInParent<Health>();
            if (!h) return;
            float rangeFactor = Mathf.Clamp01(1f - Vector3.Distance(transform.position, targetPoint) / Mathf.Max(1f, detectionRange));
            float finalDamage = damage * Mathf.Lerp(0.72f, 1.08f, rangeFactor);
            h.Damage(finalDamage);
            AIStimulusBus.Report(transform.position, 42f, 0.72f, AIStimulusType.Gunshot);
        }

        private Vector3 PredictTargetPoint()
        {
            Vector3 current = target.position;
            Vector3 velocity = (current - previousTargetPosition) / Mathf.Max(thinkInterval, 0.02f);
            previousTargetPosition = current;
            return current + Vector3.ClampMagnitude(velocity, 12f) * Mathf.Clamp(Vector3.Distance(transform.position, current) / 35f, 0.05f, 0.32f);
        }

        private void SearchLastKnownPosition()
        {
            Vector3 search = rememberedSearchPoints;
            if (Vector3.Distance(transform.position, search) < 2.5f)
            {
                Vector3 dir = (search - spawnOrigin).normalized;
                if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
                Vector3 sweep = Quaternion.Euler(0f, Mathf.Sin(Time.time * 0.7f) * 70f, 0f) * dir * (4f + suspicion * 6f);
                search += sweep;
                rememberedSearchPoints = search;
            }
            MoveTowards(search);
            FacePoint(search);
            suspicion = Mathf.Max(0f, suspicion - 0.025f);
        }

        private void Retreat()
        {
            Vector3 away = (transform.position - target.position);
            away.y = 0f;
            if (away.sqrMagnitude < 0.01f) away = -target.forward;
            Vector3 destination = transform.position + away.normalized * 10f;
            if (hasCover) destination = coverPoint;
            MoveTowards(destination);
            if (Vector3.Distance(transform.position, target.position) > detectionRange * 0.9f) suspicion = Mathf.Max(0f, suspicion - 0.08f);
        }

        private bool TryFindCover(Vector3 threat, out Vector3 result)
        {
            result = transform.position;
            float bestScore = float.MinValue;
            Vector3 preferredSide = role == SquadRole.FlankerLeft ? -transform.right : role == SquadRole.FlankerRight ? transform.right : Vector3.zero;
            if (TacticalCoverDirector.Instance && TacticalCoverDirector.Instance.TryFindCover(transform.position, threat, preferredSide, out result))
                return true;
            Vector3 toThreat = (threat - transform.position).WithY(0f);
            Vector3 forward = toThreat.sqrMagnitude > 0.01f ? toThreat.normalized : transform.forward;
            Vector3 side = Vector3.Cross(Vector3.up, forward);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                Vector3 p = transform.position + dir * 4.5f;
                p.y = transform.position.y;
                Vector3 eye = p + Vector3.up * 1.1f;
                if (!Physics.Linecast(eye, threat + Vector3.up * 1.1f, out RaycastHit hit, visionMask, QueryTriggerInteraction.Ignore)) continue;
                float away = Vector3.Dot(dir, -forward);
                float travel = Vector3.Distance(transform.position, p);
                float flankBias = role == SquadRole.FlankerLeft ? Vector3.Dot(dir, side) : role == SquadRole.FlankerRight ? Vector3.Dot(dir, -side) : 0f;
                float score = away * 2.5f + flankBias * 0.7f - travel * 0.12f;
                if (score > bestScore) { bestScore = score; result = p; }
            }
            return bestScore > 0.5f;
        }

        private bool HasLineOfSight(Vector3 point)
        {
            Vector3 origin = transform.position + Vector3.up * 1.1f;
            Vector3 aim = point + Vector3.up * 1.0f;
            Vector3 delta = aim - origin;
            if (delta.sqrMagnitude < 0.01f) return true;
            if (!Physics.Raycast(origin, delta.normalized, out RaycastHit hit, delta.magnitude, visionMask, QueryTriggerInteraction.Ignore)) return true;
            if (hit.transform == target || (target && hit.transform.IsChildOf(target))) return true;
            return false;
        }

        private void FacePoint(Vector3 point)
        {
            Vector3 flat = point - transform.position;
            flat.y = 0f;
            if (flat.sqrMagnitude > 0.01f)
                transform.forward = Vector3.Slerp(transform.forward, flat.normalized, 1f - Mathf.Exp(-9f * Time.deltaTime));
        }

        private void MoveTowards(Vector3 destination)
        {
            Vector3 delta = destination - transform.position;
            delta.y = 0;
            if (delta.sqrMagnitude > 0.16f)
            {
                Vector3 dir = delta.normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;
                FacePoint(destination);
            }
        }

        public void ResetForPool()
        {
            dead = false;
            target = null;
            nextAttack = 0f;
            suspicion = 0f;
            hasCover = false;
            if (health) health.ResetHealth();
        }

        private void Die()
        {
            if (dead) return;
            dead = true;
            GameManager.Instance?.AddXP((int)xpReward);
            MissionSystem.Global?.Advance(ObjectiveType.Kill, archetype.ToString(), 1);
            LootDrop.Spawn(transform.position, archetype);
            AIStimulusBus.Report(transform.position, 24f, 0.9f, AIStimulusType.AllyDown);
            if (SpawnDirector.Instance) SpawnDirector.Instance.Release(this);
            else Destroy(gameObject, archetype == EnemyArchetype.Elite ? 3.5f : 1.5f);
        }
    }
}
