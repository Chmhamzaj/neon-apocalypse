#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using NitroStreetRacing = NitroStreetRush.Racing;

namespace NitroStreetRush.Editor
{
    public static class HDAssetSceneIntegrator
    {
        private const string SourceFolder = "Assets/Art/NitroStreetRush/Source";
        private const string PlayerName = "PlayerCar";
        private const string IntegratedMarkerName = "HDArtVehicle";

        public static bool Integrate()
        {
            if (!Directory.Exists(SourceFolder)) return false;

            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != HDSceneBootstrap.ScenePath)
                scene = EditorSceneManager.OpenScene(HDSceneBootstrap.ScenePath, OpenSceneMode.Single);

            GameObject player = GameObject.Find(PlayerName);
            if (!player) return false;

            if (player.transform.Find(IntegratedMarkerName) != null)
                return false;

            GameObject modelAsset = FindHeroModel();
            if (!modelAsset) return false;

            GameObject art = PrefabUtility.InstantiatePrefab(modelAsset, player.transform) as GameObject;
            if (!art)
                art = UnityEngine.Object.Instantiate(modelAsset, player.transform);

            art.name = IntegratedMarkerName;
            art.transform.localPosition = Vector3.zero;
            art.transform.localRotation = Quaternion.identity;
            NormalizeVehicleScale(art, 4.5f);
            RemovePlaceholderVisuals(player.transform, art.transform);

            var lod = art.GetComponentInChildren<NitroStreetRacing.LODController>(true);
            if (!lod) art.AddComponent<NitroStreetRacing.LODController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"Nitro Street Rush HD art integrated: {AssetDatabase.GetAssetPath(modelAsset)}");
            return true;
        }

        private static GameObject FindHeroModel()
        {
            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { SourceFolder });
            var candidates = new List<string>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith(SourceFolder, StringComparison.OrdinalIgnoreCase)) continue;
                string name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                if (name.Contains("supercar") || name.Contains("hero") || name.Contains("player"))
                    candidates.Add(path);
            }

            if (candidates.Count == 0) return null;
            candidates.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (string path in candidates)
            {
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (obj) return obj;
            }

            return null;
        }

        private static void NormalizeVehicleScale(GameObject root, float targetLengthMeters)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

            float currentLength = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (currentLength <= 0.01f) return;

            float scale = targetLengthMeters / currentLength;
            root.transform.localScale *= Mathf.Clamp(scale, 0.1f, 10f);
        }

        private static void RemovePlaceholderVisuals(Transform player, Transform keep)
        {
            var children = new List<GameObject>();
            for (int i = 0; i < player.childCount; i++)
            {
                Transform child = player.GetChild(i);
                if (child != keep) children.Add(child.gameObject);
            }

            foreach (var child in children)
            {
                if (child) UnityEngine.Object.DestroyImmediate(child);
            }
        }
    }
}
#endif
