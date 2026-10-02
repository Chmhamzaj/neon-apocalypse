# NEON APOCALYPSE — Technical Architecture

## Runtime layers
**Core:** session lifecycle, save data, progression, settings.

**Gameplay:** player controller, combat, health, enemies, bosses, missions and loot.

**Presentation:** HUD, camera, VFX/tracers, world health bars, atmosphere.

**Content:** regions, spawn profiles and future addressable/streamed content.

## Scaling strategy
The vertical slice intentionally avoids coupling game rules to specific meshes, scenes or art assets. Production regions can therefore swap placeholder geometry for original environment packs, rigs, animation controllers and VFX without rewriting gameplay logic.

## Large-world direction
For Region 01+ the intended production approach is additive/streamed scenes with content grouped by district. Keep resident gameplay managers tiny and stream environment/content around them. Texture/audio/cinematic payloads should be budgeted per device tier rather than padding install size.

## Mobile performance targets
- 60 FPS target on strong devices; quality tiers for lower-end phones.
- Avoid per-frame global searches in final production; cache references and use event-driven registries.
- Pool frequent projectiles, loot, tracers, damage numbers and enemy waves.
- Use LODs, occlusion culling, GPU instancing and compressed textures/audio in production.
- Profile CPU, GPU, memory and thermal behavior on representative Android hardware before release.


## Build 0.5 additions
- SpawnDirector owns an enemy pool and active set.
- GameManager registers the player transform once; AI consumes that reference.
- Weapon uses a reusable tracer LineRenderer.
- Camera uses LateUpdate + collision-aware smoothing.
- PerformanceOverlay provides development diagnostics.
