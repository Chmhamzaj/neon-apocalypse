# NEON APOCALYPSE — Build 1.7 Advanced Gameplay Systems

## Major additions
- Bounded tactical cover service for squad AI.
- Crime evidence trail with decay, strength scoring, and police-facing queries.
- Persistent companion relationships: trust, loyalty, bond, morale, mission history.
- Vehicle archetype catalog covering road, off-road, marine and airborne classes.
- VehicleSpecBinder for data-driven handling profiles.

## Performance intent
AI services are event/tick driven and bounded. Evidence is capped at 64 records by default. Cover queries are capped to 18 candidates. Companion saves occur on meaningful events or lifecycle pause/quit.

## Visual production target
These systems are deliberately separated from visual assets so high-resolution production assets can be introduced without changing mission/AI logic.

## Validation status
Static source and package checks only. Unity compilation, GPU profiling, thermal behavior and Android device testing still require a Unity + Android hardware environment.
