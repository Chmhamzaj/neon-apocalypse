# NEON APOCALYPSE — HD Production Targets

This is a target specification, not a claim that placeholder assets already meet it.

## Visual target
- High-fidelity physically based materials
- 4K source textures for hero assets, streamed/downscaled for mobile tiers
- Nanite-like geometric discipline adapted to mobile budgets
- Dense but aggressively culled environments
- Baked + mixed lighting for stable frame times
- Volumetric-looking fog using mobile-friendly techniques
- Screen-space reflections selectively enabled on high-tier devices
- High-quality temporal anti-aliasing where supported
- Hero characters with detailed facial rigs and layered animation
- Cinematic camera language for major story beats

## Android performance tiers
**Low:** stable 30 FPS target, reduced shadow distance, compressed textures, simplified VFX.
**Medium:** 30–45 FPS target, medium texture/lighting budget.
**High:** 45–60 FPS target where device thermals permit.
**Ultra:** highest visual preset for flagship devices; not assumed on every phone.

## Storage target
Large install size must come from meaningful content: environments, animations, audio, cinematics, voice packs, and optional high-resolution asset packs. Do not add junk data solely to reach 1 GB.
