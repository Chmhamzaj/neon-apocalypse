using System.Collections.Generic;
using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class TrafficSpawner : MonoBehaviour
    {
        [SerializeField] private TrafficPool pool;
        [SerializeField] private Transform player;
        [SerializeField] private RaceSplinePath racePath;
        [SerializeField] private float aheadDistance = 140f;
        [SerializeField] private float spawnEvery = 0.65f;
        [SerializeField] private float laneWidth = 3.5f;
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
                if (Vector3.Distance(active[i].transform.position, player.position) > 230f && active[i].transform.position.z < player.position.z - 10f)
                { pool.Return(active[i]); active.RemoveAt(i); }
            }
            if (timer > 0f || active.Count >= maxActive) return;
            timer = spawnEvery;
            if (racePath && racePath.TotalLength > 1f)
            {
                float playerDistance = EstimatePlayerDistance();
                float distance = playerDistance + aheadDistance + Random.Range(-20f, 30f);
                if (distance >= racePath.TotalLength - 5f) distance = 20f;
                int lane = Random.Range(-1, 2);
                Vector3 pos = racePath.GetPoint(distance) + racePath.GetRight(distance) * (lane * laneWidth) + Vector3.up * 0.45f;
                GameObject vehicle = pool.Rent(pos, Quaternion.LookRotation(racePath.GetTangent(distance), Vector3.up));
                if (vehicle && vehicle.TryGetComponent<TrafficVehicleAI>(out var ai)) ai.SetPath(racePath, distance, lane * laneWidth);
                if (vehicle) active.Add(vehicle);
                return;
            }
        }
        private float EstimatePlayerDistance()
        {
            float best = 0f, bestSq = float.MaxValue;
            for (int i = 0; i <= 32; i++)
            {
                float d = racePath.TotalLength * i / 32f;
                float sq = (racePath.GetPoint(d) - player.position).sqrMagnitude;
                if (sq < bestSq) { bestSq = sq; best = d; }
            }
            return best;
        }
    }
}
