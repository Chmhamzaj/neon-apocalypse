using UnityEngine;
using NeonApocalypse.Combat;
using NeonApocalypse.Core;
using NeonApocalypse.Progression;
using NeonApocalypse.UI;
using System.Collections.Generic;

namespace NeonApocalypse.AI
{
    public class BossAI : MonoBehaviour
    {
        [Header("Phases")]
        public float phase2Health = 0.66f;
        public float phase3Health = 0.33f;
        public float maxHealth = 1800f;
        public float attackRange = 16f;
        public float moveSpeed = 2.7f;
        public float attackCooldown = 1.4f;
        public float specialCooldown = 4.5f;

        public int Phase { get; private set; } = 1;

        private Health health;
        private Transform target;
        private float nextAttack;
        private float nextSpecial;
        private bool dead;
        private Vector3 arenaCenter;
        private readonly List<BossProjectile> projectilePool = new List<BossProjectile>(24);

        private void Awake()
        {
            health = GetComponent<Health>();
            health.SetMaxHealth(maxHealth);
            health.Damaged += OnDamaged;
            health.Died += OnDied;
            arenaCenter = transform.position;
            nextSpecial = Time.time + 2.5f;
            PrewarmProjectiles(24);
        }

        private void Update()
        {
            if (dead) return;
            if (!target && GameManager.Instance) target = GameManager.Instance.PlayerTransform;
            if (!target) return;

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0;
            float dist = toTarget.magnitude;
            if (dist > 6f)
                transform.position += toTarget.normalized * moveSpeed * Time.deltaTime;
            if (toTarget.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toTarget.normalized), 5f * Time.deltaTime);

            if (Time.time >= nextAttack && dist <= attackRange)
            {
                nextAttack = Time.time + Mathf.Max(0.45f, attackCooldown - (Phase - 1) * 0.25f);
                AttackPlayer();
            }

            if (Time.time >= nextSpecial)
            {
                nextSpecial = Time.time + Mathf.Max(1.8f, specialCooldown - (Phase - 1) * 0.8f);
                SpecialAttack();
            }
        }

        private void AttackPlayer()
        {
            var h = target.GetComponentInParent<Health>();
            if (h) h.Damage(22f + Phase * 8f);
        }

        private void SpecialAttack()
        {
            MobileHud.Instance?.ShowMessage(Phase == 3 ? "NEMESIS: OVERDRIVE" : "NEMESIS: CHARGING", 1.3f);
            int count = Phase == 1 ? 3 : Phase == 2 ? 5 : 8;
            for (int i = 0; i < count; i++)
            {
                BossProjectile p = GetProjectile();
                if (!p) continue;
                p.transform.position = transform.position + Vector3.up * 1.5f + Random.insideUnitSphere * 0.5f;
                p.gameObject.SetActive(true);
                Vector3 targetPoint = target.position + Random.insideUnitSphere * (1.5f + Phase);
                p.Initialize(targetPoint, Phase * 7f);
            }
        }


        private void PrewarmProjectiles(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                orb.name = "Nemesis_Plasma_Pooled";
                orb.transform.SetParent(transform.parent, true);
                orb.transform.localScale = Vector3.one * 0.28f;
                Object.Destroy(orb.GetComponent<SphereCollider>());
                BossProjectile p = orb.AddComponent<BossProjectile>();
                p.SetPoolRelease(ReleaseProjectile);
                orb.SetActive(false);
                projectilePool.Add(p);
            }
        }

        private BossProjectile GetProjectile()
        {
            for (int i = 0; i < projectilePool.Count; i++)
                if (projectilePool[i] && !projectilePool[i].gameObject.activeSelf) return projectilePool[i];
            return null;
        }

        private void ReleaseProjectile(BossProjectile p)
        {
            if (!p) return;
            p.ResetForPool();
            p.gameObject.SetActive(false);
        }
        private void OnDamaged(float current, float max)
        {
            float pct = current / max;
            if (Phase == 1 && pct <= phase2Health) EnterPhase(2);
            else if (Phase == 2 && pct <= phase3Health) EnterPhase(3);
        }

        private void EnterPhase(int phase)
        {
            Phase = phase;
            moveSpeed += 0.8f;
            attackCooldown = Mathf.Max(0.5f, attackCooldown - 0.15f);
            specialCooldown = Mathf.Max(1.8f, specialCooldown - 0.5f);
            MobileHud.Instance?.ShowMessage("BOSS PHASE " + Phase, 2f);
        }

        private void SpawnAdd()
        {
            Vector3 p = arenaCenter + Random.insideUnitSphere * (7f + Phase * 3f);
            p.y = transform.position.y;
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Boss_Spawned_Elite";
            go.transform.position = p;
            var h = go.AddComponent<Health>();
            h.SetMaxHealth(220f);
            var ai = go.AddComponent<EnemyAI>();
            ai.archetype = EnemyArchetype.Elite;
        }

        private void OnDied()
        {
            dead = true;
            for (int i = 0; i < 5; i++)
                SpawnRewardOrb(i);
            GameManager.Instance?.AddXP(1500);
            MissionSystem.Global?.Advance(ObjectiveType.Kill, "BOSS", 1);
            MobileHud.Instance?.ShowMessage("TARGET ELIMINATED", 4f);
            Destroy(gameObject, 6f);
        }

        private void SpawnRewardOrb(int index)
        {
            var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "BossReward_" + index;
            orb.transform.position = transform.position + Vector3.up * 2f + Random.insideUnitSphere * 2f;
            orb.transform.localScale = Vector3.one * 0.3f;
            Object.Destroy(orb.GetComponent<Collider>());
            var loot = orb.AddComponent<LootRewardOrb>();
            loot.value = 50 + Phase * 25;
        }
    }

    public class LootRewardOrb : MonoBehaviour
    {
        public int value = 75;
        private void Update()
        {
            transform.Rotate(0, 200f * Time.deltaTime, 0);
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p && Vector3.Distance(transform.position, p.transform.position) < 2f)
            {
                GameManager.Instance?.AddXP(value);
                MobileHud.Instance?.ShowMessage($"CORE FRAGMENT +{value} XP", 1.5f);
                Destroy(gameObject);
            }
        }
    }
}
