using System.Collections.Generic;
using UnityEngine;
using NeonApocalypse.Vehicles;
using NeonApocalypse.AI.Civilians;

namespace NeonApocalypse.World.OpenWorld
{
    public sealed class TrafficAndPedestrianDirector : MonoBehaviour
    {
        public int trafficCount = 28;
        public int pedestrianCount = 36;
        public float simulationRadius = 170f;
        public float respawnEvery = 0.2f;
        private Transform player;
        private float nextRespawn;
        private readonly List<GameObject> traffic = new List<GameObject>(40);
        private readonly List<GameObject> pedestrians = new List<GameObject>(48);

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player) { RefillTraffic(); RefillPedestrians(); }
        }

        private void Update()
        {
            if (!player || Time.time < nextRespawn) return;
            nextRespawn = Time.time + respawnEvery;
            RecycleFar(traffic);
            RecycleFar(pedestrians);
            RefillTraffic();
            RefillPedestrians();
        }

        private void RefillTraffic()
        {
            while (traffic.Count < trafficCount) SpawnTraffic(traffic.Count);
        }

        private void RefillPedestrians()
        {
            while (pedestrians.Count < pedestrianCount) SpawnPedestrian(pedestrians.Count);
        }

        private void SpawnTraffic(int index)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "TrafficCar_" + index;
            go.transform.position = player.position + new Vector3((index % 7 - 3) * 18f, 0.8f, (index % 5 - 2) * 24f);
            go.transform.localScale = new Vector3(1.65f, 1.2f, 3.8f);
            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            var trafficAgent = go.AddComponent<TrafficVehicle>();
            trafficAgent.Initialize(player, 17003 + index * 977);
            traffic.Add(go);
        }

        private void SpawnPedestrian(int index)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Civilian_" + index;
            go.transform.position = player.position + new Vector3((index % 9 - 4) * 9f, 1f, (index % 7 - 3) * 10f);
            go.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            var col = go.GetComponent<CapsuleCollider>();
            if (col) col.isTrigger = true;
            go.AddComponent<CivilianAgent>().Initialize(player, 3301 + index * 173);
            pedestrians.Add(go);
        }

        private void RecycleFar(List<GameObject> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var go = list[i];
                if (!go) { list.RemoveAt(i); continue; }
                if (!go.activeSelf) { go.SetActive(true); PlaceNearPlayer(go, i); }
                else if (Vector3.SqrMagnitude(go.transform.position - player.position) > simulationRadius * simulationRadius)
                {
                    go.SetActive(false);
                }
            }
        }

        private void PlaceNearPlayer(GameObject go, int index)
        {
            float a = (index * 47.3f) * Mathf.Deg2Rad;
            float r = 55f + (index % 5) * 15f;
            go.transform.position = player.position + new Vector3(Mathf.Cos(a) * r, go.name.StartsWith("Civilian") ? 1f : 0.8f, Mathf.Sin(a) * r);
        }
    }
}
