using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class CityDressingSpawner : MonoBehaviour
    {
        [SerializeField] private RaceSplinePath path;
        [SerializeField] private GameObject[] buildingPrefabs;
        [SerializeField] private GameObject[] streetLightPrefabs;
        [SerializeField] private GameObject[] vegetationPrefabs;
        [SerializeField] private float interval = 18f;
        [SerializeField] private float sideOffset = 10f;
        [SerializeField] private int seed = 2710;
        [SerializeField] private Transform root;
        public void Build()
        {
            if (!path || path.TotalLength <= 0f || !root) return;
            Random.InitState(seed);
            int count = Mathf.FloorToInt(path.TotalLength / Mathf.Max(4f, interval));
            for (int i = 0; i < count; i++)
            {
                float d = i * interval + 4f;
                Vector3 center = path.GetPoint(d);
                Vector3 right = path.GetRight(d);
                Vector3 tangent = path.GetTangent(d);
                Quaternion facing = Quaternion.LookRotation(tangent, Vector3.up);
                Spawn(buildingPrefabs, center + right * sideOffset, facing, 0.82f + Random.value * 0.55f);
                Spawn(buildingPrefabs, center - right * sideOffset, facing * Quaternion.Euler(0f, 180f, 0f), 0.82f + Random.value * 0.55f);
                if (streetLightPrefabs != null && streetLightPrefabs.Length > 0 && i % 2 == 0) Spawn(streetLightPrefabs, center + right * 6f, facing, 0.95f);
                if (vegetationPrefabs != null && vegetationPrefabs.Length > 0 && i % 3 == 0) Spawn(vegetationPrefabs, center - right * 7f, facing, 0.9f + Random.value * 0.3f);
            }
        }

        private void Spawn(GameObject[] prefabs, Vector3 position, Quaternion rotation, float scale)
        {
            if (prefabs == null || prefabs.Length == 0) return;
            var go = Instantiate(prefabs[Random.Range(0, prefabs.Length)], position, rotation, root);
            go.transform.localScale *= scale;
        }
    }
}
