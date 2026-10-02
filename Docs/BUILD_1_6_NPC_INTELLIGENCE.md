# Build 1.6 — Mission-Grade NPC Intelligence

## New systems
- Police investigation chain: incidents create pooled investigators that travel to the last-known scene, sweep, and escalate into pursuit when they reacquire the player.
- Companion tactical director: nearby companions coordinate Follow / Defend / AttackTarget decisions on a 0.4s cadence.
- Faction conflict drift: territory stability/conflict changes over time with wanted-level pressure and bounded state transitions.
- Traffic schedule director: simulated rush-hour and curfew periods alter traffic speed on a slow cadence.
- NPC simulation LOD: distant enemies/civilians/traffic are disabled from expensive Update loops while near agents retain responsive thinking.

## Stability fixes
- Removed duplicate faction switch case from the life-simulation AI director.
- Removed repeated multiplicative speed decay for Chrome Serpents; faction tuning is now idempotent.
- Companion hostile search now uses OverlapSphereNonAlloc to avoid per-command allocations.
- Traffic scheduling uses a stored baseline speed so multipliers never compound.

## Mobile performance target
Decision systems are bounded by cadence and distance. The renderer/quality system remains responsible for LODs, shadows, dynamic resolution, and GPU budgets.
