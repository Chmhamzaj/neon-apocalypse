# HD Production Integration Checklist

## Gameplay backbone
- [x] Rigidbody vehicle controller with 320 km/h target envelope
- [x] Nitro drain/recharge and boost camera FOV
- [x] Touch steering, brake and nitro input
- [x] Traffic AI with spline-aware movement
- [x] Race progress and curved/elevated finish flow
- [x] Checkpoints, score, combo and race objectives
- [x] Pause, restart and results/replay flow
- [x] Route guidance HUD and race-event banner
- [x] Player vehicle bootstrap wiring camera/progress/input
- [x] Driver steering-visual hook
- [x] Nitro, tire-smoke and brake-light FX hooks

## Procedural production slice
- [x] Curved/elevated spline route
- [x] Road lane markings
- [x] Traffic with varied paint
- [x] Buildings and window bands
- [x] Trees, streetlights and guardrails
- [x] Stunt ramps and checkpoint gates
- [x] Finish gate
- [x] Mobile-responsive HUD/control layout

## HD asset integration target
- Hero vehicle: Blender/Higgsfield HD asset, 46-part architecture
- Driver: 36-part interior character rig target
- Traffic: 4 reusable variants sharing mesh geometry
- Environment: modular road, buildings, windows, trees, lights, guard rails, lane markings

## Automated HD handoff
- [x] Unity Editor asset integrator added
- [x] Build pipeline attempts HD-art discovery before Android build
- [ ] Place validated revision-7 GLB under `Assets/Art/NitroStreetRush/Source/`
- [ ] Confirm imported hero model is discovered and replaces procedural hero visuals
- [ ] Add remaining HD environment/traffic models to the source folder

## Final verification
- [ ] Successful Unity Android build
- [ ] APK produced from Unity HD scene
- [ ] Physical Android device verification
- [ ] Performance/thermal pass
- [ ] Final visual QA after HD asset injection

### Build environment note
The GitHub Unity runner currently reaches the Unity Builder but stops at Unity license activation because no `UNITY_LICENSE` or `UNITY_SERIAL` is configured. This is an environment/configuration blocker; it is not evidence of a C# compile failure.
