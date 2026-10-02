using System.Collections.Generic;
using UnityEngine;
using NeonApocalypse.World.OpenWorld;

namespace NeonApocalypse.AI.Squad
{
    /// <summary>Coordinates a small companion team without running a planner every frame.</summary>
    public sealed class CompanionTacticalDirector : MonoBehaviour
    {
        public float thinkEvery = 0.4f;
        public float threatRadius = 30f;
        private Transform player;
        private float nextThink;
        private readonly Collider[] hostileBuffer = new Collider[48];

        private void Start() => player = GameObject.FindGameObjectWithTag("Player")?.transform;

        private void Update()
        {
            if (Time.time < nextThink) return;
            nextThink = Time.time + thinkEvery;
            if (!player) player = NeonApocalypse.Core.GameManager.Instance?.PlayerTransform;
            if (!player) return;
            Coordinate();
        }

        private void Coordinate()
        {
            IReadOnlyList<CompanionAI> companions = CompanionAI.ActiveCompanions;
            if (companions == null || companions.Count == 0) return;
            Transform threat = FindNearestEnemy();
            int active = 0;
            for (int i = 0; i < companions.Count; i++) if (companions[i] && companions[i].isActiveAndEnabled) active++;
            if (active == 0) return;

            int slot = 0;
            for (int i = 0; i < companions.Count; i++)
            {
                var c = companions[i];
                if (!c || !c.isActiveAndEnabled) continue;
                if (threat)
                {
                    if (slot == 0) c.SetCommand(CompanionAI.Command.AttackTarget, threat);
                    else if (slot % 3 == 1) c.SetCommand(CompanionAI.Command.Defend);
                    else c.SetCommand(CompanionAI.Command.Follow);
                }
                else if (WantedSystem.Instance && WantedSystem.Instance.WantedLevel >= 3)
                {
                    c.SetCommand(slot == 0 ? CompanionAI.Command.Defend : CompanionAI.Command.Follow);
                }
                else
                {
                    c.SetCommand(CompanionAI.Command.Follow);
                }
                slot++;
            }
        }

        private Transform FindNearestEnemy()
        {
            int count = Physics.OverlapSphereNonAlloc(player.position, threatRadius, hostileBuffer, ~0, QueryTriggerInteraction.Ignore);
            Transform best = null; float bestD = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                Collider hit = hostileBuffer[i];
                if (!hit) continue;
                Transform root = hit.transform.root;
                if (!root || root.CompareTag("Player")) continue;
                var enemy = root.GetComponentInChildren<EnemyAI>();
                if (!enemy) continue;
                float d = (root.position - player.position).sqrMagnitude;
                if (d < bestD) { bestD = d; best = root; }
            }
            return best;
        }
    }
}
