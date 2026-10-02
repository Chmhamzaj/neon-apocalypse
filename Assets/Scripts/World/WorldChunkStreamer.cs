using System.Collections.Generic;
using UnityEngine;

namespace NeonApocalypse.World
{
    public sealed class WorldChunk : MonoBehaviour
    {
        public Vector2Int coordinate;
        public float activationRadius = 90f;
        public bool alwaysLoaded;
    }

    /// <summary>Lightweight additive-style scene partitioning for procedural/editor-built regions.</summary>
    public sealed class WorldChunkStreamer : MonoBehaviour
    {
        public Transform player;
        public float checkEvery = 0.25f;
        public int activeRadius = 1;
        private float nextCheck;
        private readonly List<WorldChunk> chunks = new List<WorldChunk>(64);
        private Vector2Int currentCell;

        private void Start()
        {
            if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
            WorldChunk[] found = FindObjectsByType<WorldChunk>(FindObjectsSortMode.None);
            chunks.AddRange(found);
            Apply();
        }

        private void Update()
        {
            if (!player || Time.time < nextCheck) return;
            nextCheck = Time.time + checkEvery;
            Vector2Int cell = new Vector2Int(Mathf.FloorToInt(player.position.x / 70f), Mathf.FloorToInt(player.position.z / 70f));
            if (cell != currentCell) { currentCell = cell; Apply(); }
        }

        private void Apply()
        {
            foreach (WorldChunk chunk in chunks)
            {
                if (!chunk) continue;
                bool loaded = chunk.alwaysLoaded || Mathf.Abs(chunk.coordinate.x - currentCell.x) <= activeRadius && Mathf.Abs(chunk.coordinate.y - currentCell.y) <= activeRadius;
                if (chunk.gameObject.activeSelf != loaded) chunk.gameObject.SetActive(loaded);
            }
        }
    }
}
