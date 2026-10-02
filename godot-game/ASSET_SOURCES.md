# Street Sovereign 3D — External Asset Sources

This project downloads selected third-party assets during the Android build rather than storing large binaries in the Git repository.

## Human character / NPC base
Source: UMRAM-Bilkent supine-human-model
URL: https://github.com/UMRAM-Bilkent/supine-human-model
File: assets/human.glb
License/provenance: the repository states that human.glb is derived from a CC0 Quaternius character and includes rigged/skinned walk/idle animation data.

## Vehicle
Source: KhronosGroup glTF Sample Assets
URL: https://github.com/KhronosGroup/glTF-Sample-Assets/tree/main/Models/CarConcept
File: Models/CarConcept/glTF-Binary/CarConcept.glb
The Khronos sample documentation and derivative projects document the source/provenance and attribution information for the CarConcept asset. The build keeps it as an integrated game asset.

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

## Build policy
External downloads happen in GitHub Actions, then Godot imports the assets before the APK is exported. No network downloads are performed by the game at runtime.


## Vehicle fleet

Kenney Car Kit assets are sourced from a public mirror of the Kenney assets used by Godot racing projects:
https://github.com/Hidencod/tge-assets/tree/main/packs/car-kit

Selected GLBs in this build: sedan, sedan-sports, suv, suv-luxury, hatchback-sports, taxi, police, ambulance, firetruck, truck, van, delivery, race. The mirror identifies these models as Kenney Car Kit assets and the associated project documentation identifies the car-kit assets as CC0/public-domain use.

The build also includes KhronosGroup CarConcept as an additional distinct vehicle:
https://github.com/KhronosGroup/glTF-Sample-Assets/tree/main/Models/CarConcept
