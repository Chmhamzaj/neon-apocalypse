# NEON APOCALYPSE — Mega World Plan (0.9)

## Target footprint
The campaign world is now designed around **30 x 24 streamed cells**, each 700m wide: **21.0 km x 16.8 km** of explorable space, with deterministic generation so the same seed produces the same world.

The world is intentionally larger than a compact open-world map. The goal is to support the feel of a large driving/exploration game while remaining mobile-conscious by loading only nearby cells.

## District structure
The 720 cells are mapped into biome/district families:
- Metro megacity
- Industrial belts
- Suburbs
- Coast and docks
- Badlands
- Forest
- Ruined zones

These are gameplay placeholders today and are designed to accept final AAA-quality assets later without rewriting mission logic.

## Performance rules
- Never load the entire world into memory.
- Stream only a small square of cells around the player.
- Generate cells incrementally across frames.
- Keep distant geometry cheap and reserve high-detail assets for near cells.
- Keep dynamic AI, traffic, civilians and VFX budgets separate from world geometry.
- Avoid per-frame allocations on movement/combat hot paths.

## Final visual target
The eventual art pass should replace primitive geometry with:
- PBR environment kits
- high-detail hero character meshes
- high/medium/low LOD chains
- baked or mixed lighting where practical
- selective realtime lights
- volumetrics only on supported quality tiers
- optimized skinned meshes and animation clips
- streamed texture/audio/cinematic content

## Important
The source project now contains the world-scale architecture. A true production build still requires Unity Editor compilation, imported final art, device profiling and QA on representative Android hardware before any performance guarantee can honestly be made.
