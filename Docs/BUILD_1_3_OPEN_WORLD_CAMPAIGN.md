# Build 1.3 — Open World Campaign Expansion

Build 1.3 connects the mega-world sandbox to a persistent Region 01 story arc.

## New systems
- CampaignMissionDirector with 12 Region 01 missions.
- Reach, kill, and boss objectives driven by the existing combat events.
- Persistent campaign mission index in SaveData v2.
- FactionTerritoryDirector with heat modifiers and territory messaging.
- WorldShopSystem for armory, med-bay, garage, and black-market interactions.
- DynamicEncounterDirector for bounded roaming combat encounters.
- Weapon.RefillMagazine() for economy-linked resupply.
- MissionSystem objective reset/set API and duplicate-completion guard.
- Added missing EnemyArchetype enum source so the current project has an explicit definition.

## Performance contract
- Encounter population is bounded.
- Directors use coarse update cadences instead of every-frame searches where practical.
- Mission markers reuse shared material instances at creation time.
- Dynamic resolution, streaming, pooled police, enemy pooling, and bounded traffic remain in force.

## Validation
This source package can be statically sanity-checked in this environment. Unity compilation, rendering, frame pacing, thermal behavior, and Android device validation still require a Unity editor + Android hardware.
