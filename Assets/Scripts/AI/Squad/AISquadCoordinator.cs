using System.Collections.Generic;
using UnityEngine;

namespace NeonApocalypse.AI.Squad
{
    public enum SquadRole { Suppressor, FlankerLeft, FlankerRight, Rusher, Overwatch }

    public struct TacticalOrder
    {
        public SquadRole role;
        public Vector3 destination;
        public Vector3 predictedTarget;
        public bool hasDestination;
        public float confidence;
    }

    /// <summary>
    /// Lightweight tactical coordinator. It assigns complementary roles so NPCs
    /// don't all run directly at the player at once.
    /// </summary>
    public sealed class AISquadCoordinator : MonoBehaviour
    {
        public static AISquadCoordinator Instance { get; private set; }
        public float thinkEvery = 0.32f;
        public int maxAgents = 32;

        private readonly List<NeonApocalypse.AI.EnemyAI> agents = new List<NeonApocalypse.AI.EnemyAI>(32);
        private readonly Dictionary<NeonApocalypse.AI.EnemyAI, TacticalOrder> orders = new Dictionary<NeonApocalypse.AI.EnemyAI, TacticalOrder>(32);
        private Transform player;
        private float nextThink;

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        public void Register(NeonApocalypse.AI.EnemyAI agent)
        {
            if (!agent || agents.Contains(agent) || agents.Count >= maxAgents) return;
            agents.Add(agent);
        }

        public void Unregister(NeonApocalypse.AI.EnemyAI agent)
        {
            if (!agent) return;
            agents.Remove(agent);
            orders.Remove(agent);
        }

        public bool TryGetOrder(NeonApocalypse.AI.EnemyAI agent, out TacticalOrder order)
        {
            return orders.TryGetValue(agent, out order);
        }

        private void Update()
        {
            if (!player || Time.time < nextThink) return;
            nextThink = Time.time + thinkEvery;
            int count = 0;
            for (int i = agents.Count - 1; i >= 0; i--)
            {
                var agent = agents[i];
                if (!agent || !agent.isActiveAndEnabled) { agents.RemoveAt(i); continue; }
                count++;
            }
            if (count == 0) return;

            Vector3 predicted = player.position;
            Vector3 velocity = agentVelocity(player);
            predicted += velocity * 0.22f;

            int slot = 0;
            for (int i = 0; i < agents.Count; i++)
            {
                var agent = agents[i];
                if (!agent || !agent.isActiveAndEnabled) continue;
                SquadRole role;
                if (agent.archetype == NeonApocalypse.AI.EnemyArchetype.Rusher)
                    role = SquadRole.Rusher;
                else if (agent.archetype == NeonApocalypse.AI.EnemyArchetype.Gunner)
                    role = (slot % 3 == 0) ? SquadRole.Overwatch : SquadRole.Suppressor;
                else if (agent.archetype == NeonApocalypse.AI.EnemyArchetype.Elite)
                    role = (slot % 2 == 0) ? SquadRole.FlankerLeft : SquadRole.FlankerRight;
                else
                {
                    switch (slot % 5)
                    {
                        case 0: role = SquadRole.Suppressor; break;
                        case 1: role = SquadRole.FlankerLeft; break;
                        case 2: role = SquadRole.FlankerRight; break;
                        case 3: role = SquadRole.Rusher; break;
                        default: role = SquadRole.Overwatch; break;
                    }
                }

                Vector3 toPlayer = (predicted - agent.transform.position).WithY(0f);
                Vector3 forward = toPlayer.sqrMagnitude > 0.001f ? toPlayer.normalized : Vector3.forward;
                Vector3 side = Vector3.Cross(Vector3.up, forward);
                float radius = role == SquadRole.Rusher ? 2.0f : (role == SquadRole.Overwatch ? 10f : 7f);
                Vector3 destination = predicted;
                if (role == SquadRole.FlankerLeft) destination = predicted - side * radius;
                else if (role == SquadRole.FlankerRight) destination = predicted + side * radius;
                else if (role == SquadRole.Overwatch) destination = agent.transform.position + (-forward * 3f) + side * 2f;
                else if (role == SquadRole.Suppressor) destination = predicted - forward * 5f;

                orders[agent] = new TacticalOrder { role = role, destination = destination, predictedTarget = predicted, hasDestination = true, confidence = Mathf.Clamp01(count / 5f) };
                slot++;
            }
        }

        private static Vector3 agentVelocity(Transform t)
        {
            var rb = t.GetComponent<Rigidbody>();
            return rb ? rb.linearVelocity : Vector3.zero;
        }
    }

    internal static class SquadVectorExtensions
    {
        public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
    }
}
