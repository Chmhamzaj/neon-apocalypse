# Street Sovereign 3D — External Asset Sources

This project downloads selected third-party assets during the Android build rather than storing large binaries in the Git repository. The game never downloads assets at runtime.

## Human character / NPC base
Source: UMRAM-Bilkent supine-human-model
URL: https://github.com/UMRAM-Bilkent/supine-human-model
File: assets/human.glb
License/provenance: the repository states that human.glb is derived from a CC0 Quaternius character and includes rigged/skinned walk/idle animation data.

## Kenney 3D library
The project's free-resource catalogue is:
https://github.com/teamgravitydev/gamedev-free-resources

Its 3D resources section points to Kenney and other free asset platforms. For the engine-independent 3D integration, the build uses the public GLB mirror:
https://github.com/Hidencod/tge-assets

The mirror contains 1,883 Kenney 3D models across 19 packs, including Nature Kit, Car Kit, City Kit (Commercial), City Kit (Suburban), City Kit (Roads), Food Kit, Furniture Kit, Space Kit, Graveyard Kit, Mini Characters, Blocky Characters, and the other packs published in that library.

All models in that mirror are identified by the mirror as Kenney assets released under CC0 1.0. The full packs library is included in the Android build; the city runtime instantiates the relevant building, vehicle, vegetation, road-furniture, furniture, food and themed 3D assets instead of loading them over the network.

Official Kenney pages:
- Nature Kit: https://kenney.nl/assets/nature-kit
- Car Kit: https://kenney.nl/assets/car-kit
- City Kit (Commercial): https://kenney.nl/assets/city-kit-commercial
- City Kit (Suburban): https://kenney.nl/assets/city-kit-suburban
- City Kit (Roads): https://kenney.nl/assets/city-kit-roads
- Furniture Kit: https://kenney.nl/assets/furniture-kit

## Vehicle
Source: KhronosGroup glTF Sample Assets
URL: https://github.com/KhronosGroup/glTF-Sample-Assets/tree/main/Models/CarConcept
File: Models/CarConcept/glTF-Binary/CarConcept.glb
The build keeps CarConcept as an additional distinct vehicle.

## Road and building materials
Source: Poly Haven
URL: https://polyhaven.com/
Assets:
- Asphalt 07 (2K PBR): diffuse / normal / roughness
- Concrete (2K PBR): diffuse / normal / roughness
Poly Haven states that its HDRIs, textures and 3D models are CC0.

## Street furniture
Source: Poly Haven
Asset: Street Lamp 01 (2K glTF + textures)
URL: https://polyhaven.com/a/street_lamp_01
Poly Haven states that its assets are CC0.
