# NEON APOCALYPSE — Production Roadmap

## 0.2 Vertical Slice (current)
- Playable third-person combat arena
- 4 enemy archetypes
- Boss with 3 phases
- XP/level/perk foundation
- Mission + loot + save foundation
- Mobile-ready UI hooks

## 0.3 Region 01: The Fallen City
- 8 enemy archetypes
- 1 elite boss + 1 end boss
- 10 story missions + 15 side missions
- Inventory/equipment UI
- Skill tree UI
- Crafting
- Cinematic sequence framework
- 3D environment pass

## 0.4 World Streaming
- Additive scene streaming
- 10+ regions
- Day/night + weather
- World events
- Fast travel
- Factions
- Hidden locations

## 0.5 Production Content
- Original characters
- Motion/animation pass
- VFX/audio/music
- Full UI/UX
- Localization
- Accessibility

## 0.6 Live Features (optional)
- Cloud save
- Leaderboards
- Account system
- Events
- Analytics
- Crash reporting
- Optional multiplayer architecture

## 1.0 Release Candidate
- Device profiling across low/mid/high Android devices
- Memory/thermal/battery optimization
- Play Integrity where appropriate
- AAB signing + Play Console setup
- Internal/closed testing
- Store listing and release compliance


## Build 0.5 status
- Combat allocation hotspots reduced with pooled enemies and reusable weapon tracer.
- Player transform registration removes repeated target discovery from AI.
- Camera uses LateUpdate, collision sphere cast and time-based smoothing to reduce jitter.
- Runtime development telemetry is available via F3.
- Boss projectile and loot loops cache player references.

## Build 0.6 direction
- Real region streaming chunks, addressable content boundaries, VFX pooling, occlusion, and high-fidelity asset pipeline.
