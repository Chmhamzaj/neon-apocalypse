# Build 1.8 — Mobile Stability & Performance Hardening

## Objective
Reduce hitching, allocation spikes, physics catch-up bursts and camera/render instability while preserving the HD target.

## Runtime guardrails
- 60 FPS target on capable Android devices with 30 FPS fallback through quality/profile controls.
- Fixed physics cadence is explicitly bounded.
- `Time.maximumDeltaTime` prevents long background/lag spikes from producing unbounded physics catch-up.
- Physics solver iteration counts are bounded for predictable CPU cost.
- Collision callbacks are reused and transform syncing is manual by default.
- Dynamic resolution changes are slow, hysteretic and never oscillate every frame.
- Shadow distance, pixel lights and LOD bias are set to controlled mobile-safe defaults.

## Allocation hardening
- Damage-number feedback uses a capped reusable pool.
- Loot pickups use a capped reusable pool.
- Existing enemy, boss projectile and tracer pooling remains in place.

## Camera stability
- Camera remains in LateUpdate with exponential smoothing.
- Player-owned colliders are excluded through the builder's player layer strategy where applicable.
- Vehicle rigidbodies use interpolation.

## Validation
Unity Editor/device validation is still required for final certification. The project includes the runtime diagnostics overlay and Android build configuration for on-device profiling.
