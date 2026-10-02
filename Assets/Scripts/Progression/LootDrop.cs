using System.Collections.Generic;
using UnityEngine;
using NeonApocalypse.AI;
using NeonApocalypse.Core;

namespace NeonApocalypse.Progression
{
    public class LootDrop : MonoBehaviour
    {
        public int value = 25;
        public string itemId = "NEON_SHARD";
        private static readonly Stack<LootDrop> Pool = new Stack<LootDrop>(32);
        private static Transform poolRoot;

        public static void Spawn(Vector3 position, EnemyArchetype archetype)
        {
            if (Random.value > 0.42f && archetype != EnemyArchetype.Elite) return;

            LootDrop loot = null;
            while (Pool.Count > 0 && !loot)
                loot = Pool.Pop();

            if (!loot)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "LootPoolUnit";
                go.transform.SetParent(GetPoolRoot(), false);
                go.transform.localScale = Vector3.one * 0.35f;
                loot = go.AddComponent<LootDrop>();
                go.AddComponent<LootMagnet>();
            }

            loot.value = archetype == EnemyArchetype.Elite ? 120 : 25;
            loot.itemId = "NEON_SHARD";
            loot.transform.SetPositionAndRotation(position + Vector3.up * 0.5f, Quaternion.identity);
            loot.gameObject.SetActive(true);
        }

        private void Update() => transform.Rotate(0, 120f * Time.deltaTime, 0, Space.World);

        public void Release()
        {
            gameObject.SetActive(false);
            transform.SetParent(GetPoolRoot(), false);
            if (Pool.Count < 32) Pool.Push(this);
            else Destroy(gameObject);
        }

        private static Transform GetPoolRoot()
        {
            if (poolRoot) return poolRoot;
            var go = new GameObject("FX_Pool_Loot");
            DontDestroyOnLoad(go);
            poolRoot = go.transform;
            return poolRoot;
        }
    }

    public class LootMagnet : MonoBehaviour
    {
        public float pickupRange = 1.5f;
        private Transform player;
        private float sqrRange;
        private LootDrop loot;

        private void Awake()
        {
            sqrRange = pickupRange * pickupRange;
            loot = GetComponent<LootDrop>();
        }

        private void OnEnable()
        {
            if (!loot) loot = GetComponent<LootDrop>();
            player = GameManager.Instance ? GameManager.Instance.PlayerTransform : null;
        }

        private void Update()
        {
            if (!player && GameManager.Instance) player = GameManager.Instance.PlayerTransform;
            if (!player) return;
            if ((transform.position - player.position).sqrMagnitude <= sqrRange)
            {
                MissionSystem.Global?.Advance(ObjectiveType.Collect, "NEON_SHARD", 1);
                GameManager.Instance?.AddXP(10);
                loot?.Release();
            }
        }
    }
}
