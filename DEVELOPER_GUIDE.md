# Developer Guide

## Fast start
Open the project in Unity 6.0.x, allow the package resolver to finish, then use **Neon Apocalypse > Build Vertical Slice**.

The menu constructs the arena, player, camera, combat weapon, enemy lineup, boss encounter, mission system, HUD, lighting, dynamic spawner and world time controller.

## Play loop
1. Spawn at the south gate.
2. Move with WASD; sprint with Shift; dash with Space.
3. Hold left mouse to fire; R reloads.
4. Eliminate enemies and collect Neon Shards.
5. Push toward the north boss arena.
6. Defeat NEMESIS through three health phases.
7. XP, level and perk progress persist between sessions.

## Mobile
The HUD already contains a virtual joystick and touch buttons. Production should add touch-look, aim-assist tuning, haptics, accessibility, pause/options screens and device-specific control layouts.

## Production warning
Procedural primitives are intentionally used for the current engineering slice. They are not final commercial art. Replace them with original/licensed assets before publishing to Google Play.
