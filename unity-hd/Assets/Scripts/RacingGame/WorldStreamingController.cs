using System.Collections.Generic;
using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class WorldStreamingController : MonoBehaviour
    {
        [System.Serializable]
        public class WorldChunk
        {
            public GameObject root;
            public float centerZ;
            public float loadDistance = 260f;
        }

        [SerializeField] private Transform player;
        [SerializeField] private List<WorldChunk> chunks = new();

        private void Update()
        {
            if (!player) return;
            float z = player.position.z;
            foreach (var chunk in chunks)
            {
                if (!chunk.root) continue;
                float distance = Mathf.Abs(chunk.centerZ - z);
                bool shouldBeActive = distance <= Mathf.Max(80f, chunk.loadDistance);
                if (chunk.root.activeSelf != shouldBeActive) chunk.root.SetActive(shouldBeActive);
            }
        }
    }
}