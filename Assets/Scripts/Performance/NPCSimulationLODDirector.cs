using UnityEngine;
using NeonApocalypse.AI;
using NeonApocalypse.AI.Civilians;
using NeonApocalypse.Vehicles;

namespace NeonApocalypse.Performance
{
    /// <summary>
    /// Distance-based simulation budget. Rendering remains the renderer's job;
    /// this component only lowers expensive decision frequency for far agents.
    /// </summary>
    public sealed class NPCSimulationLODDirector : MonoBehaviour
    {
        public float nearDistance = 75f;
        public float farDistance = 150f;
        public float updateEvery = 2.5f;
        private float nextUpdate;
        private Transform player;

        private void Start() => player = GameObject.FindGameObjectWithTag("Player")?.transform;

        private void Update()
        {
            if (Time.time < nextUpdate) return;
            nextUpdate = Time.time + updateEvery;
            if (!player) player = NeonApocalypse.Core.GameManager.Instance?.PlayerTransform;
            if (!player) return;
            var enemies = FindObjectsOfType<EnemyAI>(true);
            for (int i = 0; i < enemies.Length; i++)
            {
                var e = enemies[i]; if (!e) continue;
                float d = Vector3.Distance(player.position, e.transform.position);
                e.enabled = d <= farDistance;
                e.thinkInterval = d <= nearDistance ? Mathf.Clamp(e.thinkInterval, 0.12f, 0.22f) : 0.32f;
            }
            var civs = FindObjectsOfType<CivilianAgent>(true);
            for (int i = 0; i < civs.Length; i++)
            {
                var c = civs[i]; if (!c) continue;
                float d = Vector3.Distance(player.position, c.transform.position);
                c.enabled = d <= farDistance;
            }
            var traffic = FindObjectsOfType<TrafficVehicle>(true);
            for (int i = 0; i < traffic.Length; i++)
            {
                var v = traffic[i]; if (!v) continue;
                float d = Vector3.Distance(player.position, v.transform.position);
                v.enabled = d <= farDistance + 40f;
            }
        }
    }
}
