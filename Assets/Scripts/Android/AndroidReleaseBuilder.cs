#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

namespace NeonApocalypse.Android
{
    public static class AndroidReleaseBuilder
    {
        [MenuItem("Neon Apocalypse/Build Android APK (Test)")]
        public static void BuildAPK()
        {
            if (EditorBuildSettings.scenes == null || EditorBuildSettings.scenes.Length == 0)
            {
                Debug.LogError("No scenes are configured. Run Neon Apocalypse > Build Vertical Slice first.");
                return;
            }

            string outputDir = "Builds/Android";
            Directory.CreateDirectory(outputDir);
            string output = Path.Combine(outputDir, "NeonApocalypse_Test.apk");

            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.androidBuildType = AndroidBuildType.Development;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.androidIsGame = true;

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = System.Array.ConvertAll(EditorBuildSettings.scenes, s => s.path),
                locationPathName = output,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.ConnectWithProfiler
            });

            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log("Android test APK built: " + output);
            else
                Debug.LogError("Android APK build failed: " + report.summary.result);
        }

        [MenuItem("Neon Apocalypse/Build Android AAB")]
        public static void BuildAAB()
        {
            if (EditorBuildSettings.scenes == null || EditorBuildSettings.scenes.Length == 0)
            {
                Debug.LogError("No scenes are configured. Run Neon Apocalypse > Build Vertical Slice first.");
                return;
            }

            string outputDir = "Builds/Android";
            Directory.CreateDirectory(outputDir);
            string output = Path.Combine(outputDir, "NeonApocalypse.aab");

            EditorUserBuildSettings.buildAppBundle = true;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.androidBuildType = AndroidBuildType.Release;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = System.Array.ConvertAll(EditorBuildSettings.scenes, s => s.path),
                locationPathName = output,
                target = BuildTarget.Android,
                options = BuildOptions.None
            });

            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log("Android AAB built: " + output);
            else
                Debug.LogError("Android build failed: " + report.summary.result);
        }
    }
}
#endif
