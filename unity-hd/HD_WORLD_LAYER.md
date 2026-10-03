# HD World Layer

The Unity HD branch now contains the reusable world/race assembly layer.

### Route architecture
- Modular TrackSegment prefabs
- Deterministic CityRoadBuilder generation
- 20-segment default route (~2 km target)
- SceneryAnchor metadata for reusable roadside dressing
- RaceWorldBootstrap orchestration
- RaceMarkers for checkpoint and finish events

### Asset-ready structure
The final 3D vehicle, driver, road, building, vegetation, lighting and traffic assets can be imported into the prefab slots without changing the race-state architecture.

### Visual target
The production target remains physically based, fully 3D, detailed vehicle/city presentation with scalable Android quality tiers. This scaffold is not itself the final visual quality; it is the system that allows those assets to be assembled into the playable route.
