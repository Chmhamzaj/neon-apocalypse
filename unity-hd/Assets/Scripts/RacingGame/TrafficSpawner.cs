using System.Collections.Generic;
using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class TrafficSpawner : MonoBehaviour
    {
        [SerializeField] private TrafficPool pool;
        [SerializeField] private Transform player;
        [SerializeField] private float aheadDistance = 140f;
        [SerializeField] private float spawnEvery = 0.65f;
        [SerializeField] private float[] laneX = { -3.5f, 0f, 3.5f };
        [SerializeField] private int maxActive = 18;
        private readonly List<GameObject> active = new();
        private float timer;

        private void Awake() { if (!pool) pool = GetComponent<TrafficPool>(); if (pool) pool.Prewarm(); }
        private void Update()
        {
            if (!player || !pool) return;
            timer -= Time.deltaTime;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                if (!active[i]) { active.RemoveAt(i); continue; }
                if (active[i].transform.position.z < player.position.z - 45f) { pool.Return(active[i]); active.RemoveAt(i); }
            }
            if (timer > 0f || active.Count >= maxActive) return;
            timer = spawnEvery;
            int lane = Random.Range(0, laneX.Length);
            Vector3 pos = new(laneX[lane], 0.45f, player.position.z + aheadDistance + Random.Range(-15f, 30f));
            GameObject vehicle = pool.Rent(pos, Quaternion.identity);
            if (vehicle) active.Add(vehicle);
        }
    }
}
