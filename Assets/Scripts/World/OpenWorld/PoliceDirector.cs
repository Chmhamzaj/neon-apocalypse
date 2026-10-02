using System.Collections.Generic;
using UnityEngine;

namespace NeonApocalypse.World.OpenWorld
{
    public sealed class PoliceDirector : MonoBehaviour
    {
        public int maxPolice = 6;
        public float spawnDistance = 75f;
        public float updateEvery = 0.18f;
        private Transform player;
        private float nextUpdate;
        private readonly List<GameObject> units = new List<GameObject>(8);

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            for (int i = 0; i < Mathf.Clamp(maxPolice, 1, 8); i++)
            {
                var unit = CreateUnit(i);
                unit.SetActive(false);
                units.Add(unit);
            }
        }

        private void Update()
        {
            if (!player || Time.time < nextUpdate) return;
            nextUpdate = Time.time + updateEvery;
            int wanted = WantedSystem.Instance ? WantedSystem.Instance.WantedLevel : 0;
            int desired = Mathf.Min(Mathf.Clamp(maxPolice, 1, 8), wanted == 0 ? 0 : wanted + 1);
            int activeCount = 0;
            for (int i = 0; i < units.Count; i++) if (units[i] && units[i].activeSelf) activeCount++;
            while (activeCount < desired)
            {
                if (ActivateNext(activeCount)) activeCount++; else break;
            }
            while (activeCount > desired)
            {
                if (DeactivateLastActive()) activeCount--; else break;
            }
        }

        private bool ActivateNext(int index)
        {
            for (int i = 0; i < units.Count; i++)
            {
                if (!units[i] || units[i].activeSelf) continue;
                float angle = i * 61f;
                units[i].transform.position = player.position + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * spawnDistance;
                units[i].transform.rotation = Quaternion.LookRotation((player.position - units[i].transform.position).WithY(0f));
                units[i].SetActive(true);
                return true;
            }
            return false;
        }

        private bool DeactivateLastActive()
        {
            for (int i = units.Count - 1; i >= 0; i--)
            {
                if (!units[i] || !units[i].activeSelf) continue;
                units[i].SetActive(false);
                return true;
            }
            return false;
        }

        private static GameObject CreateUnit(int index)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "PoliceInterceptor_" + index;
            go.transform.localScale = new Vector3(1.8f, 1.2f, 3.8f);
            var collider = go.GetComponent<BoxCollider>();
            if (collider) Destroy(collider);
            var agent = go.AddComponent<PolicePursuitAgent>();
            agent.speed = 17f + index * 0.6f;
            return go;
        }
    }

    internal static class Vector3Extensions
    {
        public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
    }
}
