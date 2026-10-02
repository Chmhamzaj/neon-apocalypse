using System.Collections.Generic;
using UnityEngine;

namespace NeonApocalypse.World
{
    public class ProductionWorldBuilder : MonoBehaviour
    {
        public Transform player;
        public float activeDistance = 150f;
        public int buildingCount = 44;
        public int debrisCount = 90;
        public Material floorMaterial; public Material wallMaterial; public Material accentMaterial; public Material rubbleMaterial;
        private readonly List<Renderer> farRenderers = new List<Renderer>(256);
        private float nextCull;

        public void BuildRuntimeDistrict()
        {
            if (transform.childCount > 0) return;
            var root = new GameObject("FallenCity_RuntimeDistrict").transform;
            root.SetParent(transform, false);
            BuildRoadGrid(root);
            BuildBuildings(root);
            BuildDebris(root);
            BuildNeonFrames(root);
        }

        private void Update()
        {
            if (!player || Time.time < nextCull) return;
            nextCull = Time.time + 0.5f;
            Vector3 p = player.position;
            float maxDistanceSqr = activeDistance * activeDistance;
            for (int i = 0; i < farRenderers.Count; i++)
            {
                var r = farRenderers[i];
                if (!r) continue;
                r.enabled = (r.bounds.center - p).sqrMagnitude <= maxDistanceSqr;
            }
        }

        private void BuildRoadGrid(Transform root)
        {
            for (int i = -2; i <= 2; i++)
            {
                var roadX = CreatePrimitive("Road_X_" + i, PrimitiveType.Cube, root,
                    new Vector3(i * 14f, -0.4f, 0), new Vector3(7.5f, 0.2f, 72f));
                roadX.GetComponent<Renderer>().sharedMaterial = floorMaterial;
                var roadZ = CreatePrimitive("Road_Z_" + i, PrimitiveType.Cube, root,
                    new Vector3(0, -0.39f, i * 14f), new Vector3(72f, 0.2f, 7.5f));
                roadZ.GetComponent<Renderer>().sharedMaterial = floorMaterial;
            }
        }

        private void BuildBuildings(Transform root)
        {
            for (int i = 0; i < buildingCount; i++)
            {
                float a = i * 137.50776f * Mathf.Deg2Rad;
                float radius = 22f + (i % 7) * 3.3f;
                float x = Mathf.Cos(a) * radius;
                float z = Mathf.Sin(a) * radius;
                float h = 7f + (i % 9) * 2.5f;
                float w = 5f + (i % 4) * 2f;
                float d = 5f + ((i + 2) % 4) * 2f;
                var b = CreatePrimitive("Tower_" + i, PrimitiveType.Cube, root,
                    new Vector3(x, h * 0.5f, z), new Vector3(w, h, d));
                b.transform.rotation = Quaternion.Euler(0, (i * 29) % 180, 0);
                var r = b.GetComponent<Renderer>();
                if (r) { r.sharedMaterial = wallMaterial; farRenderers.Add(r); }
                if (h > 12f) AddFacadeBands(b.transform, h, w, d);
                AddBuildingLights(b.transform, h, w, d);
            }
        }

        private void AddFacadeBands(Transform building, float h, float w, float d)
        {
            int floors = Mathf.Clamp(Mathf.FloorToInt(h / 2.5f), 3, 12);
            for (int f = 1; f < floors; f++)
            {
                float y = f * (h / floors);
                var band = CreatePrimitive("FacadeBand", PrimitiveType.Cube, building,
                    new Vector3(0, -h * 0.5f + y, 0), new Vector3(w, 0.12f, d));
                var r = band.GetComponent<Renderer>();
                if (r) r.sharedMaterial = wallMaterial;
            }
        }

        private void AddBuildingLights(Transform building, float h, float w, float d)
        {
            int strips = Mathf.Clamp(Mathf.FloorToInt(h / 3f), 2, 6);
            for (int i = 0; i < strips; i++)
            {
                var strip = CreatePrimitive("WindowStrip", PrimitiveType.Cube, building,
                    new Vector3(0, -h * 0.42f + i * 2.7f, d * 0.505f), new Vector3(w * 0.72f, 0.34f, 0.04f));
                var renderer = strip.GetComponent<Renderer>();
                if (renderer) renderer.sharedMaterial = accentMaterial;
            }
        }

        private void BuildDebris(Transform root)
        {
            for (int i = 0; i < debrisCount; i++)
            {
                float a = i * 41.3f * Mathf.Deg2Rad;
                float radius = 6f + (i % 12) * 2.3f;
                Vector3 p = new Vector3(Mathf.Cos(a) * radius, 0.2f + (i % 3) * 0.15f, Mathf.Sin(a) * radius);
                var piece = CreatePrimitive("Debris_" + i, PrimitiveType.Cube, root, p,
                    new Vector3(0.3f + (i % 4) * 0.2f, 0.2f + (i % 3) * 0.25f, 0.25f + ((i + 1) % 4) * 0.2f));
                piece.transform.rotation = Quaternion.Euler(i * 13f, i * 31f, i * 7f);
                var col = piece.GetComponent<Collider>(); if (col) Destroy(col);
                var r = piece.GetComponent<Renderer>(); if (r) { r.sharedMaterial = rubbleMaterial; farRenderers.Add(r); }
            }
        }

        private void BuildNeonFrames(Transform root)
        {
            for (int i = 0; i < 18; i++)
            {
                float a = i * 20f * Mathf.Deg2Rad;
                Vector3 p = new Vector3(Mathf.Cos(a) * 38f, 2.5f, Mathf.Sin(a) * 38f);
                var frame = CreatePrimitive("NeonFrame_" + i, PrimitiveType.Cube, root, p, new Vector3(0.18f, 5f, 0.18f));
                var renderer = frame.GetComponent<Renderer>(); if (renderer) renderer.sharedMaterial = accentMaterial;
                var c = frame.GetComponent<Collider>(); if (c) Destroy(c);
            }
        }

        private static GameObject CreatePrimitive(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            return go;
        }

    }
}
