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
        [SerializeField] private float laneWidth = 3.5f;
        [SerializeField] private float sceneryOffset = 10f;
        private readonly List<GameObject> spawned = new();

        public float BuiltLength { get; private set; }

        public void Build()
        {
            Clear();
            if (segmentPrefabs == null || segmentPrefabs.Length == 0) return;
            float z = 0f;
            for (int i = 0; i < segmentCount; i++)
            {
                var prefab = segmentPrefabs[i % segmentPrefabs.Length];
                var segment = Instantiate(prefab, new Vector3(0f, 0f, z), Quaternion.identity, roadRoot);
                spawned.Add(segment.gameObject);
                z += segment.Length > 1f ? segment.Length : segmentLength;
                SpawnScenery(segment, i);
            }
            BuiltLength = z;
        }

        private void SpawnScenery(TrackSegment segment, int index)
        {
            if (!sceneryRoot || !segment.SceneryAnchor) return;
            var left = segment.SceneryAnchor;
            var right = Instantiate(segment.SceneryAnchor.gameObject, sceneryRoot);
            right.transform.SetPositionAndRotation(
                segment.transform.TransformPoint(new Vector3(sceneryOffset, 0f, segment.Length * 0.5f)),
                segment.transform.rotation * Quaternion.Euler(0f, 180f, 0f));
            spawned.Add(right);
        }

        public void Clear()
        {
            foreach (var go in spawned) if (go) DestroyImmediate(go);
            spawned.Clear();
            BuiltLength = 0f;
        }
    }
}
