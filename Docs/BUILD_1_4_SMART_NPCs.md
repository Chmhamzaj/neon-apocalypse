# NEON APOCALYPSE — Build 1.4 Smart NPCs

## Goal
Make the city population feel reactive and tactically intelligent without per-frame heavyweight planners.

## Combat AI
- Allocation-free bounded stimulus ring buffer for gunshots, alarms, impacts and vehicle crashes.
- Perception tick at 0.14s instead of doing full logic every rendered frame.
- Field-of-view + line-of-sight checks.
- Last-known-position memory and investigation/search states.
- Predictive target point based on recent player motion.
- Utility-like health-aware retreat behavior.
- Cover sampling and tactical destination selection.
- Squad roles: suppressor, left flanker, right flanker, rusher, overwatch.
- Shared squad orders reduce the 'all NPCs run directly at the player' behavior.

## Civilian AI
- Hearing-driven panic.
- Evacuation route selection with obstacle and crowd-separation checks.
- Hiding/recovery states.
- Wanted-level awareness.

## Police AI
- Pursuit / intercept / search states.
- Predictive intercept points instead of pure point-chasing.
- Loss-of-sight memory.

## Performance guardrails
- All global stimulus storage is bounded (96 entries).
- NPC decision-making is ticked, not rendered-frame-driven.
- Squad coordinator is bounded to 32 agents.
- Existing pooled enemy/police architecture is preserved.

## Future intelligence upgrades
- Road-aware traffic planning.
- Squad voice/radio events.
- Goal arbitration between combat, rescue and retreat.
- Persistent NPC schedules and relationships.
- NavMesh / world-query integration once production navigation data is available.

## Design principles
NPC intelligence is deliberately game-system intelligence, not a cloud/LLM dependency. Decisions are deterministic, local-first, and bounded so the same behavior can run offline on Android.
