# Unity HD Build Pipeline

Unity version: 6000.2.0f1.
Target: Android APK.
Build entry point: NitroStreetRush.Editor.BuildAndroid.Build.

GitHub Actions workflow: `.github/workflows/unity-hd-android.yml`.

The workflow is intentionally separate from the Godot fallback workflow. A successful run produces `NitroStreetRush-HD.apk` from the Unity HD project.

Required GitHub repository secrets for a licensed Unity build runner:
UNITY_EMAIL
UNITY_PASSWORD
UNITY_SERIAL

These credentials are not stored in the repository. The source tree now has the actual build route; a Unity-licensed runner must still be able to authenticate before GitHub Actions can produce the APK.
