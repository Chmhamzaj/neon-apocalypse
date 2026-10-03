# HD Production Integration Checklist

## Gameplay backbone
- [x] Rigidbody vehicle controller with 320 km/h target envelope
- [x] Nitro drain/recharge and boost camera FOV
- [x] Touch steering, brake and nitro input
- [x] Traffic AI attachment in spawner
- [x] Race progress and 2 km finish flow
- [x] Explicit race-track layout scaffold
- [x] Player vehicle bootstrap wiring camera/progress/input
- [x] Driver steering-visual hook
- [x] Nitro, tire-smoke and brake-light FX hooks

## Asset integration target
- Hero vehicle: Blender/Higgsfield HD asset, 46-part architecture
- Driver: 36-part interior character rig target
- Traffic: 4 reusable variants sharing mesh geometry
- Environment: modular road, buildings, windows, trees, lights, guard rails, lane markings

## Next production block
1. Build the modular road/city segment authoring layer.
2. Add pooled traffic lifecycle and collision recovery.
3. Add vehicle FX/audio event hooks and race-state events.
4. Add quality-tier asset/LOD configuration for Android.
5. Import the validated GLB into the Unity asset pipeline when a Unity build environment is available.
6. Perform an actual Unity Android build and device verification; the existing Godot APK remains only a fallback smoke build.
