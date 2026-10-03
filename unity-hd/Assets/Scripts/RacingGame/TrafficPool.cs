using System.Collections.Generic;
using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class TrafficPool : MonoBehaviour
    {
        [SerializeField] private GameObject[] prefabs;
        [SerializeField] private int prewarm = 18;
        private readonly Queue<GameObject> pool = new();
        public void Prewarm()
        {
            for (int i = 0; i < prewarm; i++) pool.Enqueue(Create());
        }
        private GameObject Create()
        {
            if (prefabs == null || prefabs.Length == 0) return null;
            var go = Instantiate(prefabs[Random.Range(0, prefabs.Length)], transform);
            if (!go.TryGetComponent<TrafficVehicleAI>(out _)) go.AddComponent<TrafficVehicleAI>();
            go.SetActive(false);
            return go;
        }
        public GameObject Rent(Vector3 position, Quaternion rotation)
        {
            var go = pool.Count > 0 ? pool.Dequeue() : Create();
            if (!go) return null;
            go.transform.SetPositionAndRotation(position, rotation);
            go.SetActive(true);
            return go;
        }
        public void Return(GameObject go)
        {
            if (!go) return;
            go.SetActive(false);
            go.transform.SetParent(transform);
            pool.Enqueue(go);
        }
    }
}
