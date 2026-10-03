from dataclasses import dataclass, asdict
import json

@dataclass
class AssetRecord:
    name: str
    source: str
    format: str
    lods: list[str]
    collision: str
    scale_meters: tuple[float, float, float]

ASSETS = [
    AssetRecord("HeroSupercar", "Blender5.2Automation", "GLB", ["LOD0","LOD1","LOD2"], "convex+wheel_colliders", (2.0,4.8,1.95)),
    AssetRecord("CityBlock_A", "Procedural", "GLB", ["LOD0","LOD1"], "box_colliders", (40.0,70.0,25.0)),
]

if __name__ == "__main__":
    print(json.dumps([asdict(a) for a in ASSETS], indent=2))
