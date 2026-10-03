#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NitroStreetRush.Editor
{
    public static class BuildAndroid
    {
        [MenuItem("Nitro Street Rush/Build Android APK") ]
        public static void Build()
        {
            HDSceneBootstrap.EnsureScene();
            AssetDatabase.Refresh();
            HDAssetSceneIntegrator.Integrate();
            HDSceneValidator.Validate();
            const string output = "Builds/NitroStreetRush-HD.apk";
            Directory.CreateDirectory("Builds");
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            var scenes = GetEnabledScenes();
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = output,
                target = BuildTarget.Android,
                options = BuildOptions.CompressWithLz4HC
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"Android build failed: {report.summary.result}");
            Debug.Log($"Nitro Street Rush HD APK built: {Path.GetFullPath(output)}");
        }

        private static string[] GetEnabledScenes()
        {
            var scenes = new System.Collections.Generic.List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
                if (scene.enabled) scenes.Add(scene.path);
            if (scenes.Count == 0) throw new BuildFailedException("No enabled scenes are configured.");
            return scenes.ToArray();
        }
    }
}
#endif