# NEON APOCALYPSE — Build 0.8

## Focus
First production-content pass for Region 01 with visual richness and frame-time stability as co-equal requirements.

## Added
- Generated 2048px environment texture set for asphalt, concrete, metal, panels, rubble, and dark glass.
- Procedural production district with roads, 52 high-rise blocks, facade bands, 120 debris pieces, and neon framing.
- Shared-material GPU instancing enabled for generated materials.
- Hero presentation kit: chest armor, shoulders, helmet, visor, backpack, and emissive accent.
- Opening cinematic camera sequence with smooth easing.
- Dedicated performance director using a smoothed frame-time window and conservative dynamic-resolution steps.
- Camera pivot smoothing and explicit default raycast layers to avoid self-collision jitter.
- Android IL2CPP, ARM64, streaming-mipmap, shadow, pixel-light, and frame-queue defaults tuned for mobile.

## Performance rules
- Do not spawn realtime point lights per window or prop.
- Keep dynamic render-scale changes below perceptible frequency.
- Pool combat projectiles, tracers, enemies, and transient VFX.
- Keep distant renderers disabled until the player approaches.
- Prefer shared materials and instancing over unique material instances.
- Target 60 FPS on capable devices, with a 30 FPS fallback strategy.

## Visual roadmap
The current generated geometry is intentionally a production scaffold. The next asset passes can replace it with authored hero environments, skeletal characters, animation sets, high-detail props, decals, particles, audio, and cinematics without changing the campaign systems.
