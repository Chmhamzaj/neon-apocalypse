#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace NitroStreetRush.Editor
{
    public static class HDSceneValidator
    {
        [MenuItem("Nitro Street Rush/Validate HD Scene")]
        public static void Validate()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid()) throw new BuildFailedException("Active scene is not valid.");

            string[] requiredTypes =
            {
                "NitroStreetRush.Racing.RaceStateEvents",
                "NitroStreetRush.Racing.SaveGame",
                "NitroStreetRush.Racing.RaceProgress",
                "NitroStreetRush.Racing.RaceFlowController",
                "NitroStreetRush.Racing.RaceTimer",
                "NitroStreetRush.Racing.RaceComboSystem"
            };

            int missing = 0;
            foreach (var typeName in requiredTypes)
            {
                bool found = false;
                foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                {
                    foreach (var component in go.GetComponents<Component>())
                    {
                        if (component && component.GetType().FullName == typeName) { found = true; break; }
                    }
                    if (found) break;
                }
                if (!found) { Debug.LogError($"HD Scene missing required component: {typeName}"); missing++; }
            }

            if (missing > 0) throw new BuildFailedException($"HD Scene validation failed with {missing} missing required components.");
            Debug.Log("Nitro Street Rush HD Scene validation passed.");
        }
    }
}
#endif
