# Build 2.1 — Critical Changes

## User requirements addressed
1. Final release target is an Android **APK** for hands-on testing. The Unity editor/build toolchain is not present in this environment, so no fake or non-installable APK is included. The project contains the real APK build menu and Android settings.
2. Previous HD packs are **merged into this single project** under `Assets/Art/HDMaterials/`.
3. The low-information/flat texture masters were replaced with visibly detailed procedural game-art surfaces: road wear, grime, scratches, seams, brick courses, cracks, panel geometry, glass streaks, rust, neon construction, terrain variation and signs/decals.

## Performance guardrails retained
- ARM64 Android target
- IL2CPP/Gradle release path
- Adaptive resolution with hysteresis
- NPC simulation LOD
- bounded AI decision ticks
- pooled combat and loot effects
- streamed world cells
- capped physics catch-up
- interpolated vehicle motion
- shared material instancing

## APK build path
Open in Unity 6.0.43f1 with Android Build Support installed:
- `Neon Apocalypse > Build Vertical Slice`
- `Neon Apocalypse > Build Android APK (Test)`

Expected output: `Builds/Android/NeonApocalypse_Test.apk`
