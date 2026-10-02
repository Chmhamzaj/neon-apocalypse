using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeonApocalypse.World.Mega
{
    /// <summary>
    /// Streams a deterministic large world without keeping the whole footprint loaded.
    /// The default target is 30x24 cells at 700m = 21km x 16.8km of explorable world.
    /// </summary>
    public sealed class MegaWorldDirector : MonoBehaviour
    {
        public MegaWorldConfig config;
        public Transform player;
        public Material groundMaterial;
        public Material buildingMaterial;
        public Material accentMaterial;
        public Material propMaterial;

        private readonly Dictionary<Vector2Int, MegaWorldChunk> active = new Dictionary<Vector2Int, MegaWorldChunk>();
        private Vector2Int lastCell = new Vector2Int(int.MinValue, int.MinValue);
        private Coroutine rebuildRoutine;

        private void Awake()
        {
            if (!config) config = CreateRuntimeConfig();
            if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        private void Start()
        {
            if (!player) return;
            lastCell = config.WorldToCell(player.position);
            RebuildStreamingSet(lastCell);
        }

        private void Update()
        {
            if (!player || !config) return;
            Vector2Int cell = config.WorldToCell(player.position);
            if (cell == lastCell) return;
            lastCell = cell;
            QueueRebuild(cell);
        }

        private MegaWorldConfig CreateRuntimeConfig()
        {
            MegaWorldConfig runtime = ScriptableObject.CreateInstance<MegaWorldConfig>();
            runtime.hideFlags = HideFlags.DontSave;
            return runtime;
        }

        private void QueueRebuild(Vector2Int center)
        {
            if (rebuildRoutine != null) StopCoroutine(rebuildRoutine);
            rebuildRoutine = StartCoroutine(RebuildRoutine(center));
        }

        private void RebuildStreamingSet(Vector2Int center)
        {
            HashSet<Vector2Int> wanted = BuildWantedSet(center);
            List<Vector2Int> remove = new List<Vector2Int>();
            foreach (var pair in active)
                if (!wanted.Contains(pair.Key)) remove.Add(pair.Key);
            for (int i = 0; i < remove.Count; i++) RemoveChunk(remove[i]);
            foreach (Vector2Int cell in wanted)
                if (!active.ContainsKey(cell)) CreateChunk(cell);
        }

        private IEnumerator RebuildRoutine(Vector2Int center)
        {
            HashSet<Vector2Int> wanted = BuildWantedSet(center);
            List<Vector2Int> remove = new List<Vector2Int>();
            foreach (var pair in active)
                if (!wanted.Contains(pair.Key)) remove.Add(pair.Key);
            for (int i = 0; i < remove.Count; i++)
            {
                RemoveChunk(remove[i]);
                if ((i & 1) == 1) yield return null;
            }

            int built = 0;
            foreach (Vector2Int cell in wanted)
            {
                if (!active.ContainsKey(cell))
                {
                    CreateChunk(cell);
                    built++;
                    if (built >= Mathf.Max(1, config.buildsPerFrame))
                    {
                        built = 0;
                        yield return null;
                    }
                }
            }
            rebuildRoutine = null;
        }

        private HashSet<Vector2Int> BuildWantedSet(Vector2Int center)
        {
            HashSet<Vector2Int> result = new HashSet<Vector2Int>();
            int radius = Mathf.Clamp(config.activeRadius, 1, 4);
            for (int z = -radius; z <= radius; z++)
            for (int x = -radius; x <= radius; x++)
            {
                Vector2Int cell = new Vector2Int(center.x + x, center.y + z);
                if (config.IsValidCell(cell)) result.Add(cell);
            }
            return result;
        }

        private void CreateChunk(Vector2Int cell)
        {
            if (!config.IsValidCell(cell)) return;
            GameObject go = new GameObject($"MegaCell_{cell.x}_{cell.y}");
            go.transform.SetParent(transform, false);
            go.transform.position = config.CellCenter(cell);
            MegaWorldChunk chunk = go.AddComponent<MegaWorldChunk>();
            chunk.coordinate = cell;
            chunk.seed = Hash(cell, config.seed);
            chunk.biome = ChooseBiome(cell, chunk.seed);
            active.Add(cell, chunk);
            MegaWorldCellBuilder.Build(chunk, config, groundMaterial, buildingMaterial, accentMaterial, propMaterial);
        }

        private void RemoveChunk(Vector2Int cell)
        {
            if (!active.TryGetValue(cell, out MegaWorldChunk chunk)) return;
            active.Remove(cell);
            if (chunk) Destroy(chunk.gameObject);
        }

        private MegaBiome ChooseBiome(Vector2Int c, int seed)
        {
            float north = c.y / Mathf.Max(1f, config.cellsZ - 1f);
            float east = c.x / Mathf.Max(1f, config.cellsX - 1f);
            if (north < 0.16f) return MegaBiome.Coast;
            if (east > 0.83f && north > 0.55f) return MegaBiome.Forest;
            if (east < 0.18f && north < 0.55f) return MegaBiome.Badlands;
            if (((seed >> 5) & 7) == 0) return MegaBiome.Ruins;
            if (east > 0.58f) return MegaBiome.Industrial;
            if (north < 0.42f) return MegaBiome.Suburbs;
            return MegaBiome.Metro;
        }

        private static int Hash(Vector2Int c, int seed)
        {
            unchecked
            {
                int h = seed;
                h = h * 486187739 + c.x * 16777619;
                h = h * 486187739 + c.y * 374761393;
                h ^= h >> 13;
                return h;
            }
        }
    }
}
