# NEON APOCALYPSE 1.6 — Mission-Grade NPC Intelligence

This build extends the smart NPC foundation into a persistent living-world layer.

### AI
- Police investigate last-known incidents before escalating to pursuit.
- Companions coordinate commands as a small tactical team.
- Faction conflict drifts over time and can change territory ownership.
- Traffic changes speed by simulated time-of-day.
- Distance LOD suppresses expensive AI loops for distant agents.

### Engineering fixes
- Removed duplicate faction switch case that could fail Unity compilation.
- Removed compounding Chrome Serpents speed decay.
- Companion hostile search uses non-allocating physics queries.
- Traffic scheduling is based on immutable baseline cruise speed.
