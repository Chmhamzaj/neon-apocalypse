using System.Collections.Generic;
using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class CityRoadBuilder : MonoBehaviour
    {
        [SerializeField] private TrackSegment[] segmentPrefabs;
        [SerializeField] private Transform roadRoot;
        [SerializeField] private Transform sceneryRoot;
        [SerializeField] private int segmentCount = 20;
        [SerializeField] private float segmentLength = 100f;
        [SerializeField] private float sceneryOffset = 10f;
        [SerializeField] private int seed = 2709;
        private readonly List<GameObject> spawned = new();
        public float BuiltLength { get; private set; }

        public void Build()
        {
            Clear();
            if (segmentPrefabs == null || segmentPrefabs.Length == 0) return;
            Random.InitState(seed);
            float z = 0f;
            for (int i = 0; i < segmentCount; i++)
            {
                var prefab = segmentPrefabs[i % segmentPrefabs.Length];
                var segment = Instantiate(prefab, new Vector3(0f, 0f, z), Quaternion.identity, roadRoot);
                spawned.Add(segment.gameObject);
                float length = segment.Length > 1f ? segment.Length : segmentLength;
                SpawnScenery(segment, i, length);
                z += length;
            }
            BuiltLength = z;
        }

        private void SpawnScenery(TrackSegment segment, int index, float length)
        {
            if (!sceneryRoot || !segment.SceneryAnchor) return;
            var anchor = segment.SceneryAnchor;
            Vector3 baseLocal = new Vector3(sceneryOffset, 0f, length * 0.5f);
            var right = Instantiate(anchor.gameObject, sceneryRoot);
            right.transform.SetPositionAndRotation(segment.transform.TransformPoint(baseLocal), segment.transform.rotation * Quaternion.Euler(0f, 180f, 0f));
            right.name = $"Scenery_{index:00}_R";
            spawned.Add(right);
        }

        public void Clear()
        {
            for (int i = spawned.Count - 1; i >= 0; i--)
            {
                if (!spawned[i]) continue;
                if (Application.isPlaying) Destroy(spawned[i]);
                else DestroyImmediate(spawned[i]);
            }
            spawned.Clear(); BuiltLength = 0f;
        }
    }
}
