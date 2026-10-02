# World Streaming Contract

NEON APOCALYPSE is partitioned around 70m world cells. WorldChunkStreamer tracks the player cell and enables only nearby chunks.

The current vertical slice intentionally keeps the training district in one root for simple iteration. Production Region 01 should be partitioned into many WorldChunk roots and, later, migrated to addressable/additive scene loading.

Streaming rules:
- Keep the current cell plus a one-cell safety ring active.
- Keep mission-critical cinematics/NPCs explicitly always-loaded.
- Never unload the player, persistent managers, or save state.
- Profile activation/deactivation and asset memory on Android hardware.
