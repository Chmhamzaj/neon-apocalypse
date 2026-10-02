# NEON APOCALYPSE — Build 1.0 Open-World Gameplay

The Mega World now has an integrated open-world gameplay layer on top of its streamed footprint.

## Player vehicle
- Arcade Rigidbody handling with continuous collision detection and interpolation.
- Desktop enter/exit with E.
- Mobile DRIVE button for enter/exit.
- Existing mobile joystick becomes throttle + steering while occupied.
- Vehicle health and impact damage.
- Dedicated vehicle camera target.

## Living world
- Bounded ambient traffic.
- Bounded pedestrian simulation.
- Distance-based recycling.
- Five-level wanted/heat system.
- Lightweight police pursuit agents.

## Interiors
- Prototype runtime-generated safehouse, black market and nightclub interiors.
- Interiors exist only while being used and are removed on exit.

## Performance
- Population counts are capped.
- Traffic/civilians avoid scene-wide searches in their update loops.
- Player vehicle uses Rigidbody interpolation and continuous collision detection.
- Existing Mega World streams cells instead of keeping the entire footprint resident.
- Mobile input is event-driven from the existing joystick.

## Scale
21.0 km x 16.8 km = 352.8 km² target footprint, divided into 720 deterministic cells.

Actual GPU/frame-time certification still requires Unity builds and Android device testing.
