# HD Unity Import Guide

## Target location
Place the validated HD Asset Lab GLB under:
Assets/Art/NitroStreetRush/Source/

## Unity 6 import
The HDAssetPostprocessor automatically applies the production model-import defaults and converts legacy Standard materials to URP Lit when necessary.

## Prefab assembly
1. Create the hero car prefab from the imported GLB.
2. Add Rigidbody, CarController, VehiclePrefabAuthoring, and CollisionRecovery.
3. Assign wheel transforms to VehicleWheelVisual.
4. Assign the 36-part driver hierarchy to DriverRigController.
5. Add RaceFXController and connect nitro, tire-smoke, and brake-light particle/light objects.
6. Add RaceCamera as the chase camera target and PlayerVehicleBootstrap to the player root.

## World assembly
- Build road sections from TrackSegment prefabs.
- Use RaceSplinePath for turns/elevation.
- Use CityRoadBuilder and CityDressingSpawner for route dressing.
- Use WorldStreamingController to keep distant city chunks inactive on constrained Android devices.
- Pool traffic with TrafficPool and drive it with spline-aware TrafficVehicleAI.

## Quality targets
Balanced: 30 FPS and aggressive LOD/streaming.
High: 60 FPS target with full road/city presentation.
Ultra: 60 FPS target with higher LOD ranges and richer effects where device thermals permit.

## Asset source
Higgsfield 3D Jutsu project: Nitro Street Rush — HD Asset Lab.
Validated committed scene: revision 4.