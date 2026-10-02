using System.Collections.Generic;
using UnityEngine;
using NeonApocalypse.AI;
using NeonApocalypse.Combat;

namespace NeonApocalypse.World.OpenWorld
{
    public sealed class DynamicEncounterDirector : MonoBehaviour
    {
        private sealed class Encounter { public GameObject root; public float endAt; }
        public int maxActive = 2;
        public float startEverySeconds = 55f;
        public float triggerRadius = 110f;
        private Transform player;
        private float nextSpawn;
        private readonly List<Encounter> active = new List<Encounter>(4);
        private static readonly Vector3[] offsets =
        {
            new Vector3(95,0,45), new Vector3(-85,0,70), new Vector3(90,0,-70), new Vector3(-105,0,-55)
        };

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            nextSpawn = Time.time + 20f;
        }

        private void Update()
        {
            if (!player) return;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var e = active[i];
                if (!e.root || Time.time >= e.endAt || Vector3.SqrMagnitude(player.position - e.root.transform.position) > triggerRadius * triggerRadius)
                {
                    if (e.root) Destroy(e.root);
                    active.RemoveAt(i);
                }
            }
            if (Time.time < nextSpawn || active.Count >= Mathf.Clamp(maxActive, 1, 4)) return;
            nextSpawn = Time.time + Mathf.Max(25f, startEverySeconds);
            SpawnEncounter();
        }

        private void SpawnEncounter()
        {
            int index = Random.Range(0, offsets.Length);
            var root = new GameObject("DynamicEncounter");
            root.transform.position = player.position + offsets[index];
            for (int i = 0; i < 4; i++)
            {
                var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = "EncounterUnit";
                enemy.transform.SetParent(root.transform, false);
                enemy.transform.localPosition = new Vector3((i % 2 == 0 ? -1 : 1) * (2 + i), 1f, (i / 2) * 5f);
                Destroy(enemy.GetComponent<CapsuleCollider>());
                var health = enemy.AddComponent<Health>();
                health.maxHealth = i == 3 ? 280f : 160f;
                var ai = enemy.AddComponent<EnemyAI>();
                ai.Initialize(i == 3 ? EnemyArchetype.Elite : (i % 3 == 0 ? EnemyArchetype.Gunner : EnemyArchetype.Grunt), player, enemy.transform.position);
            }
            active.Add(new Encounter { root = root, endAt = Time.time + 75f });
            WantedSystem.Instance?.AddHeat(0.25f);
            NeonApocalypse.UI.MobileHud.Instance?.ShowMessage("DYNAMIC ENCOUNTER: SIGNAL SPIKE", 4f);
        }
    }
}
