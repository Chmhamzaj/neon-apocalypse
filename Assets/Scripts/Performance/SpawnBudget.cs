using UnityEngine;
using NeonApocalypse.AI;
using NeonApocalypse.World;

namespace NeonApocalypse.Performance
{
    /// <summary>Cheap global population guard to prevent runaway enemy/effect counts.</summary>
    public sealed class SpawnBudget : MonoBehaviour
    {
        public static SpawnBudget Instance { get; private set; }
        public int maxEnemies = 18;
        public int maxBossProjectiles = 24;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool CanSpawnEnemy()
        {
            if (SpawnDirector.Instance) return SpawnDirector.Instance.GetActiveCount() < maxEnemies;
            return FindObjectsByType<EnemyAI>(FindObjectsSortMode.None).Length < maxEnemies;
        }
    }
}
