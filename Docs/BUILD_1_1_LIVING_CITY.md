# NEON APOCALYPSE — Build 1.1 Living City & Crime

## New systems
- Four faction identities with persistent reputation in the running session.
- Persistent world credits with controlled spend/reward APIs.
- Dynamic world activities: armored convoys, extractions and gang ambushes.
- Facility terminals for safehouse heat clearing, black-market purchases and vehicle repair.
- Pooled police units replace repeated instantiate/destroy churn.
- World activity population is bounded and expires or unloads outside the local activity radius.

## Performance intent
- Police units are prewarmed once and toggled rather than instantiated every wanted-level change.
- Dynamic activities are capped at three concurrently.
- Facility interactions are proximity-gated and event-driven by a single low-frequency key check per terminal.
- No per-frame scene-wide object searches were introduced.

## Gameplay direction
This is the first layer where the mega map can continuously produce optional player-driven situations, reputation consequences and resource decisions without requiring a fixed linear mission path.
