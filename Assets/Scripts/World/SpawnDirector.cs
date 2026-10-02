using System.Collections.Generic;
using UnityEngine;
using NeonApocalypse.AI;
using NeonApocalypse.Combat;
using NeonApocalypse.Performance;

namespace NeonApocalypse.World
{
    public class SpawnDirector : MonoBehaviour
    {
        [System.Serializable]
        public class SpawnProfile { public EnemyArchetype archetype; public int weight = 1; }

        public static SpawnDirector Instance { get; private set; }
        public SpawnProfile[] profiles;
        public Transform[] spawnPoints;
        public int maxAlive = 12;
        public float spawnEvery = 8f;
        public int burst = 2;
        public int prewarm = 16;

        private float nextSpawn;
        private readonly List<EnemyAI> pool = new List<EnemyAI>(32);
        private readonly HashSet<EnemyAI> active = new HashSet<EnemyAI>();
        private Transform player;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            Prewarm();
        }

        private void Update()
        {
            if (Time.time < nextSpawn) return;
            nextSpawn = Time.time + spawnEvery;
            if (active.Count >= maxAlive || (SpawnBudget.Instance != null && !SpawnBudget.Instance.CanSpawnEnemy())) return;

            int room = maxAlive - active.Count;
            int count = Mathf.Min(burst, room);
            for (int i = 0; i < count; i++) SpawnOne();
        }

        private void Prewarm()
        {
            for (int i = 0; i < Mathf.Max(prewarm, maxAlive); i++)
            {
                EnemyAI enemy = CreatePooledEnemy();
                enemy.gameObject.SetActive(false);
                pool.Add(enemy);
            }
        }

        private EnemyAI CreatePooledEnemy()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "EnemyPoolUnit";
            Object.Destroy(go.GetComponent<CapsuleCollider>());
            go.AddComponent<SphereCollider>();
            go.GetComponent<Renderer>().sharedMaterial = BuildFallbackMaterial();
            Health h = go.AddComponent<Health>();
            h.SetMaxHealth(140f);
            EnemyAI ai = go.AddComponent<EnemyAI>();
            return ai;
        }

        private Material BuildFallbackMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.color = new Color(0.36f, 0.055f, 0.07f);
            mat.SetFloat("_Metallic", 0.25f);
            mat.SetFloat("_Smoothness", 0.25f);
            return mat;
        }

        private void SpawnOne()
        {
            if (spawnPoints == null || spawnPoints.Length == 0) return;
            EnemyAI ai = GetFreeEnemy();
            if (!ai) return;

            SpawnProfile profile = PickProfile();
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            ai.transform.SetPositionAndRotation(point.position, Quaternion.identity);
            ai.transform.localScale = profile.archetype == EnemyArchetype.Elite ? Vector3.one * 1.3f : Vector3.one;
            ai.gameObject.SetActive(true);
            ai.Initialize(profile.archetype, player, point.position);
            active.Add(ai);
        }

        private EnemyAI GetFreeEnemy()
        {
            for (int i = 0; i < pool.Count; i++)
                if (pool[i] && !pool[i].gameObject.activeSelf) return pool[i];
            EnemyAI extra = CreatePooledEnemy();
            pool.Add(extra);
            return extra;
        }

        public int GetActiveCount() => active.Count;

        public void Release(EnemyAI ai)
        {
            if (!ai) return;
            active.Remove(ai);
            ai.ResetForPool();
            ai.gameObject.SetActive(false);
        }

        private SpawnProfile PickProfile()
        {
            if (profiles == null || profiles.Length == 0)
                return new SpawnProfile { archetype = EnemyArchetype.Grunt, weight = 1 };
            int total = 0;
            foreach (SpawnProfile p in profiles) total += Mathf.Max(0, p.weight);
            int roll = Random.Range(0, Mathf.Max(1, total));
            foreach (SpawnProfile p in profiles)
            {
                roll -= Mathf.Max(0, p.weight);
                if (roll < 0) return p;
            }
            return profiles[profiles.Length - 1];
        }
    }
}
