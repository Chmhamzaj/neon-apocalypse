using System.Collections.Generic;
using UnityEngine;

namespace NeonApocalypse.World.Mega
{
    public enum MegaBiome { Metro, Industrial, Suburbs, Coast, Badlands, Forest, Ruins }

    public sealed class MegaWorldChunk : MonoBehaviour
    {
        public Vector2Int coordinate;
        public MegaBiome biome;
        public bool highDetail;
        public int seed;
        private readonly List<GameObject> generated = new List<GameObject>(128);

        public void ResetChunk()
        {
            for (int i = generated.Count - 1; i >= 0; i--)
            {
                if (generated[i]) Destroy(generated[i]);
            }
            generated.Clear();
        }

        public void Track(GameObject go)
        {
            if (go) generated.Add(go);
        }
    }
}
