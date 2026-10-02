using System.Collections.Generic;
using UnityEngine;

namespace NeonApocalypse.World.OpenWorld
{
    /// <summary>Reusable interior shells. Prevents repeated Instantiate/Destroy spikes when entering buildings.</summary>
    public sealed class InteriorPoolService : MonoBehaviour
    {
        public static InteriorPoolService Instance { get; private set; }
        private readonly Dictionary<string, GameObject> roots = new Dictionary<string, GameObject>(8);

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public Transform Acquire(string interiorName, Vector3 worldPosition, Material wallMaterial = null)
        {
            string key = string.IsNullOrEmpty(interiorName) ? "DEFAULT" : interiorName;
            if (!roots.TryGetValue(key, out var root) || !root)
            {
                root = Build(key, wallMaterial);
                roots[key] = root;
            }
            root.transform.position = worldPosition;
            root.SetActive(true);
            return root.transform;
        }

        public void Release(string interiorName)
        {
            string key = string.IsNullOrEmpty(interiorName) ? "DEFAULT" : interiorName;
            if (roots.TryGetValue(key, out var root) && root) root.SetActive(false);
        }

        private static GameObject Build(string key, Material wallMaterial)
        {
            var root = new GameObject("InteriorPool_" + key);
            var floor = Create("Floor", root.transform, new Vector3(0f, 0f, 0f), new Vector3(26f, 0.4f, 22f));
            var back = Create("BackWall", root.transform, new Vector3(0, 3, 10), new Vector3(26, 6, 0.4f));
            var left = Create("LeftWall", root.transform, new Vector3(-13, 3, 0), new Vector3(0.4f, 6, 22f));
            var right = Create("RightWall", root.transform, new Vector3(13, 3, 0), new Vector3(0.4f, 6, 22f));
            var roof = Create("Roof", root.transform, new Vector3(0, 6, 0), new Vector3(26f, 0.4f, 22f));
            if (wallMaterial)
            {
                floor.GetComponent<Renderer>().sharedMaterial = wallMaterial;
                back.GetComponent<Renderer>().sharedMaterial = wallMaterial;
                left.GetComponent<Renderer>().sharedMaterial = wallMaterial;
                right.GetComponent<Renderer>().sharedMaterial = wallMaterial;
                roof.GetComponent<Renderer>().sharedMaterial = wallMaterial;
            }
            root.SetActive(false);
            return root;
        }

        private static GameObject Create(string name, Transform parent, Vector3 local, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            go.transform.localScale = scale;
            return go;
        }
    }
}
