using UnityEngine;

namespace NeonApocalypse.World.Mega
{
    public static class MegaWorldCellBuilder
    {
        private const float RoadWidth = 22f;
        private const int RoadCount = 3;

        public static void Build(MegaWorldChunk chunk, MegaWorldConfig config, Material ground, Material building, Material accent, Material prop)
        {
            int seed = chunk.seed;
            System.Random rng = new System.Random(seed);
            float half = config.cellSize * 0.5f;
            CreateCube(chunk, "Ground", new Vector3(0f, -0.55f, 0f), new Vector3(config.cellSize, 1f, config.cellSize), ground, false);

            BuildRoads(chunk, config, ground, half);
            switch (chunk.biome)
            {
                case MegaBiome.Metro: BuildMetro(chunk, config, building, accent, rng, half); break;
                case MegaBiome.Industrial: BuildIndustrial(chunk, config, building, accent, prop, rng, half); break;
                case MegaBiome.Suburbs: BuildSuburbs(chunk, config, building, prop, rng, half); break;
                case MegaBiome.Coast: BuildCoast(chunk, config, building, prop, rng, half); break;
                case MegaBiome.Badlands: BuildBadlands(chunk, config, building, prop, rng, half); break;
                case MegaBiome.Forest: BuildForest(chunk, config, prop, rng, half); break;
                default: BuildRuins(chunk, config, building, accent, prop, rng, half); break;
            }
        }

        private static void BuildRoads(MegaWorldChunk c, MegaWorldConfig cfg, Material m, float half)
        {
            float[] t = { -cfg.cellSize * 0.28f, 0f, cfg.cellSize * 0.28f };
            for (int i = 0; i < RoadCount; i++)
            {
                CreateCube(c, "Road_EW_" + i, new Vector3(0f, -0.02f, t[i]), new Vector3(cfg.cellSize, 0.08f, RoadWidth), m, false);
                CreateCube(c, "Road_NS_" + i, new Vector3(t[i], -0.01f, 0f), new Vector3(RoadWidth, 0.08f, cfg.cellSize), m, false);
            }
        }

        private static void BuildMetro(MegaWorldChunk c, MegaWorldConfig cfg, Material b, Material a, System.Random rng, float half)
        {
            int count = cfg.buildingsPerUrbanCell;
            for (int i = 0; i < count; i++)
            {
                Vector3 p = SampleLot(rng, half, 55f);
                float h = 25f + (float)rng.NextDouble() * 160f;
                float s = 20f + (float)rng.NextDouble() * 34f;
                GameObject tower = CreateCube(c, "MetroTower", p + Vector3.up * (h * 0.5f), new Vector3(s, h, s * (0.7f + (float)rng.NextDouble() * 0.6f)), b, true);
                if (i % 3 == 0) AddNeonCrown(c, tower.transform, a, h, s);
            }
            AddLandmark(c, a, rng, "SKYBRIDGE");
        }

        private static void BuildIndustrial(MegaWorldChunk c, MegaWorldConfig cfg, Material b, Material a, Material prop, System.Random rng, float half)
        {
            for (int i = 0; i < Mathf.Max(12, cfg.buildingsPerUrbanCell / 2); i++)
            {
                Vector3 p = SampleLot(rng, half, 42f);
                float h = 10f + rng.Next(35);
                float sx = 30f + rng.Next(35), sz = 28f + rng.Next(30);
                CreateCube(c, "Factory", p + Vector3.up * h * 0.5f, new Vector3(sx, h, sz), b, true);
            }
            for (int i = 0; i < 12; i++)
            {
                Vector3 p = new Vector3(-half + 70f + i * 54f, 8f, -half + 50f);
                CreateCube(c, "StorageTank", p, new Vector3(12f, 16f, 12f), prop, false).transform.rotation = Quaternion.Euler(0, i * 19f, 0);
            }
            AddLandmark(c, a, rng, "REACTOR_YARD");
        }

        private static void BuildSuburbs(MegaWorldChunk c, MegaWorldConfig cfg, Material b, Material prop, System.Random rng, float half)
        {
            int count = Mathf.Max(20, cfg.buildingsPerUrbanCell + 10);
            for (int i = 0; i < count; i++)
            {
                Vector3 p = SampleLot(rng, half, 35f);
                float h = 6f + (float)rng.NextDouble() * 8f;
                CreateCube(c, "House", p + Vector3.up * h * 0.5f, new Vector3(18f, h, 14f), b, true);
                if (i % 2 == 0) CreateCube(c, "Garage", p + new Vector3(12f, 2f, 0f), new Vector3(8f, 4f, 10f), prop, true);
            }
        }

        private static void BuildCoast(MegaWorldChunk c, MegaWorldConfig cfg, Material b, Material prop, System.Random rng, float half)
        {
            for (int i = 0; i < 24; i++)
            {
                Vector3 p = SampleLot(rng, half, 30f);
                float h = 4f + (float)rng.NextDouble() * 15f;
                CreateCube(c, "WaterfrontBlock", p + Vector3.up * h * 0.5f, new Vector3(26f, h, 24f), b, true);
            }
            for (int i = 0; i < 16; i++)
                CreateCube(c, "Dock", new Vector3(-half + 80f + i * 42f, 0.1f, half - 70f), new Vector3(32f, 0.25f, 10f), prop, false);
        }

        private static void BuildBadlands(MegaWorldChunk c, MegaWorldConfig cfg, Material b, Material prop, System.Random rng, float half)
        {
            for (int i = 0; i < 45; i++)
            {
                Vector3 p = new Vector3((float)(rng.NextDouble() * 2 - 1) * half, 0.3f, (float)(rng.NextDouble() * 2 - 1) * half);
                float s = 8f + rng.Next(25);
                CreateCube(c, "Rock", p, new Vector3(s, 1.5f + rng.Next(10), s * 0.6f), prop, false).transform.rotation = Quaternion.Euler(rng.Next(30), rng.Next(360), rng.Next(20));
            }
            for (int i = 0; i < 8; i++)
                CreateCube(c, "Outpost", SampleLot(rng, half, 48f) + Vector3.up * 3f, new Vector3(18f, 6f, 14f), b, true);
        }

        private static void BuildForest(MegaWorldChunk c, MegaWorldConfig cfg, Material prop, System.Random rng, float half)
        {
            for (int i = 0; i < 80; i++)
            {
                Vector3 p = new Vector3((float)(rng.NextDouble() * 2 - 1) * half, 2f, (float)(rng.NextDouble() * 2 - 1) * half);
                float h = 9f + rng.Next(18);
                GameObject tree = CreateCube(c, "Tree", p + Vector3.up * h * 0.5f, new Vector3(2f, h, 2f), prop, false);
                tree.transform.rotation = Quaternion.Euler(0, rng.Next(360), 0);
            }
        }

        private static void BuildRuins(MegaWorldChunk c, MegaWorldConfig cfg, Material b, Material a, Material prop, System.Random rng, float half)
        {
            for (int i = 0; i < 34; i++)
            {
                Vector3 p = SampleLot(rng, half, 30f);
                float h = 5f + rng.Next(60);
                CreateCube(c, "RuinedTower", p + Vector3.up * h * 0.5f, new Vector3(18f + rng.Next(10), h, 16f + rng.Next(10)), b, true);
            }
            AddLandmark(c, a, rng, "BLACK_VAULT");
        }

        private static Vector3 SampleLot(System.Random rng, float half, float clearance)
        {
            float x = ((float)rng.NextDouble() * 2f - 1f) * (half - 30f);
            float z = ((float)rng.NextDouble() * 2f - 1f) * (half - 30f);
            if (Mathf.Abs(x) < clearance) x += Mathf.Sign(x == 0 ? 1 : x) * clearance;
            if (Mathf.Abs(z) < clearance) z += Mathf.Sign(z == 0 ? 1 : z) * clearance;
            return new Vector3(x, 0f, z);
        }

        private static void AddNeonCrown(MegaWorldChunk c, Transform tower, Material accent, float h, float size)
        {
            CreateCube(c, "NeonCrown", tower.localPosition + Vector3.up * (h * 0.48f), new Vector3(size * 0.78f, 2.5f, size * 0.78f), accent, false);
        }

        private static void AddLandmark(MegaWorldChunk c, Material accent, System.Random rng, string name)
        {
            Vector3 p = new Vector3((float)(rng.NextDouble() * 2 - 1) * 180f, 14f, (float)(rng.NextDouble() * 2 - 1) * 180f);
            CreateCube(c, name, p, new Vector3(50f, 28f, 50f), accent, true);
        }

        private static GameObject CreateCube(MegaWorldChunk chunk, string name, Vector3 localPosition, Vector3 scale, Material mat, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(chunk.transform, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = scale;
            Renderer r = go.GetComponent<Renderer>();
            if (r) { r.sharedMaterial = mat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; r.receiveShadows = true; }
            if (!collider)
            {
                Collider col = go.GetComponent<Collider>();
                if (col) Object.Destroy(col);
            }
            chunk.Track(go);
            return go;
        }
    }
}
