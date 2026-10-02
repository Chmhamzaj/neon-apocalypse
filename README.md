# NEON APOCALYPSE — Build 2.1 Critical Merged HD

A large-scale Unity 6 Android open-world action RPG foundation targeting a San-Andreas-scale map footprint, long campaign, advanced NPC simulation, vehicles, factions, interiors, dynamic encounters, and high-fidelity presentation.

## Critical build status
This is the **single merged source project**. All prior HD texture packs are integrated; the visual library now contains detailed authored/remastered surfaces and decals rather than flat placeholder-like images.

### Important APK note
This environment does not contain Unity Editor, Android Build Support, SDK/NDK, or ADB. Therefore an actual installable APK cannot honestly be produced here. The Unity project includes the real APK build command and will output `Builds/Android/NeonApocalypse_Test.apk` on a machine with Unity 6.0.43f1 + Android Build Support.

## Build steps
1. Open this folder as a Unity 6.0.43f1 project.
2. Install Android Build Support, SDK/NDK and OpenJDK in Unity Hub.
3. Run `Neon Apocalypse > Build Vertical Slice`.
4. Run `Neon Apocalypse > Build Android APK (Test)`.
5. Install the resulting APK on a physical Android device.

## Performance
The project targets stable frame pacing and scalable quality rather than forcing maximum settings on every device. Final FPS/jitter certification still requires real device profiling.

Build 2.1.1 packaging: 2K albedo/ORM and world-art masters are JPEG-encoded at high quality for portable source size; 4K hero textures and all normal maps remain lossless PNG.
