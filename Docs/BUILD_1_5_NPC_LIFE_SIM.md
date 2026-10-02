# Build 1.5 — NPC Life Simulation

Build 1.5 extends Smart NPCs into a lightweight living-world simulation.

## Systems
- NPCMemory: persistent threat, fear, trust, loyalty, faction and job state.
- NPCDailyRoutine: deterministic day/work/sleep roaming without per-frame planning.
- NPCRumorNetwork: bounded short-lived world information shared between nearby agents.
- FactionAIDirector: faction-specific perception/tactical tuning on a coarse decision tick.
- CompanionAI: follow/hold/defend/attack command framework for future story companions.

## Mobile performance contract
- No global per-frame NPC planner.
- Rumor store capped at 24 entries.
- Faction tuning runs every 0.8s.
- Routine decisions default to every 4s.
- Combat AI remains on the existing bounded think interval.
- Future LOD tiers should disable full routine simulation beyond the player streaming bubble.
