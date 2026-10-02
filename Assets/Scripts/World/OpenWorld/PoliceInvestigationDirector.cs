using System.Collections.Generic;
using UnityEngine;

namespace NeonApocalypse.World.OpenWorld
{
    public sealed class PoliceInvestigationDirector : MonoBehaviour
    {
        public int maxInvestigators = 3;
        public float spawnRadius = 55f;
        public float retestEvery = 0.35f;
        private Transform player;
        private float nextRetest;
        private readonly List<GameObject> pool = new List<GameObject>(4);
        private Vector3 lastIncident;
        private int incidentSerial;

        private void Awake()
        {
            WantedSystem.Instance.WantedLevelChanged += OnWantedChanged;
        }

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            EnsurePool();
        }

        private void OnDestroy()
        {
            if (WantedSystem.Instance) WantedSystem.Instance.WantedLevelChanged -= OnWantedChanged;
        }

        private void Update()
        {
            if (!player || Time.time < nextRetest) return;
            nextRetest = Time.time + retestEvery;
            int wanted = WantedSystem.Instance ? WantedSystem.Instance.WantedLevel : 0;
            if (wanted <= 1)
            {
                for (int i = 0; i < pool.Count; i++) if (pool[i]) pool[i].SetActive(false);
            }
        }

        private void OnWantedChanged(int level)
        {
            if (!player || level < 2) return;
            lastIncident = player.position;
            incidentSerial++;
            EnsurePool();
            int desired = Mathf.Clamp(level - 1, 1, maxInvestigators);
            int active = 0;
            for (int i = 0; i < pool.Count; i++)
                if (pool[i] && pool[i].activeSelf) active++;
            for (int i = 0; i < pool.Count && active < desired; i++)
            {
                if (!pool[i] || pool[i].activeSelf) continue;
                ActivateInvestigator(pool[i], i);
                active++;
            }
        }

        private void EnsurePool()
        {
            while (pool.Count < Mathf.Clamp(maxInvestigators, 1, 5))
            {
                int index = pool.Count;
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = "PoliceInvestigator_" + index;
                go.transform.localScale = new Vector3(0.65f, 1.1f, 0.65f);
                var col = go.GetComponent<CapsuleCollider>();
                if (col) col.isTrigger = true;
                go.AddComponent<PoliceInvestigationAgent>();
                go.SetActive(false);
                pool.Add(go);
            }
        }

        private void ActivateInvestigator(GameObject go, int index)
        {
            float angle = (index * 117f + incidentSerial * 23f) * Mathf.Deg2Rad;
            Vector3 pos = lastIncident + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * spawnRadius;
            pos.y = 1f;
            go.transform.position = pos;
            var agent = go.GetComponent<PoliceInvestigationAgent>();
            agent.Initialize(player, lastIncident, 1009 + index * 73 + incidentSerial * 19);
            go.SetActive(true);
        }
    }
}
