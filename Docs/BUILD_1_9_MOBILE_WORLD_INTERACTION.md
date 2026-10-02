# Build 1.9 — Mobile World Interaction & Stability

## Goals
- Reduce visible traffic jitter by moving kinematic Rigidbody motion to FixedUpdate with MovePosition/MoveRotation.
- Avoid interior entry/exit allocation spikes by retaining reusable interior shells.
- Apply one conservative mobile quality tier at boot rather than oscillating quality every frame.
- Provide a low-frequency non-allocating interaction scan for future touch/UI integration.

## Performance contract retained
- No per-frame FindGameObjectWithTag in the new systems.
- No repeated interior Instantiate/Destroy during normal use.
- Traffic uses Rigidbody interpolation and the physics timestep.
- Graphics tier is sticky for the session.
- Final frame-time and thermal certification still requires real Android hardware.
