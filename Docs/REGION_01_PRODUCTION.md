# Region 01 — Fallen City

Build 0.7 introduces the first production-layout pass: four narrative checkpoints, modular district landmarks, distance-based renderer activation, and a region director for story pacing.

The geometry is intentionally replaceable. Final HD art should be authored as optimized LOD groups, baked lighting where appropriate, texture streaming assets, and pooled VFX. Gameplay references landmarks through transforms/components so art replacement does not require rewriting mission logic.

## Performance contract
- No per-frame Instantiate/Destroy in combat loops.
- No scene-wide Find calls in Update.
- Renderer activation is distance-gated for far landmarks.
- Final region should use additive/addressable streaming for major districts.
- Validate on representative Android hardware at 30/45/60 FPS targets before release.
