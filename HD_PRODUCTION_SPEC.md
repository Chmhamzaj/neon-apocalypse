# Nitro Street Rush — HD Production Specification

## Toolchain

- Blender 5.2 LTS automation: authoritative source for hero vehicles, props, city modules, collision meshes, LOD generation, and GLB/FBX export.
- Unity 6 + C#: primary production/mobile rendering target.
- Unreal Engine 5.8 + C++: high-fidelity validation target and optional premium/high-end build.
- Godot 4.5/4.6: lightweight Android fallback and current playable prototype.
- Python: asset QA, manifest generation, batch naming, LOD/collision checks, and build tooling.
- C++: Unreal gameplay/vehicle systems and performance-sensitive native code.
- C#: Unity gameplay, UI, input, AI orchestration, save systems.

## Quality target

The game is being built as an original 3D arcade racer with real-world-inspired materials, scale, lighting, camera work, road geometry, traffic, and vehicle proportions. Commercial games such as Forza Horizon have enormous teams, budgets, and content libraries; this repository therefore treats that visual quality as a target, not a claim of one-for-one parity.

## Asset standards

Hero vehicles:
- real-world metre scale
- separate body/glass/carbon/rubber/rim/brake/light/interior materials
- wheel and brake geometry
- multiple LODs
- collision representation
- consistent forward axis and origin

World:
- modular road pieces
- lane markings, curbs, barriers, guard rails
- building facades plus depth modules
- street furniture, trees, signage, lighting
- traffic spawn points and racing spline data

## Android quality tiers

Tier 1: mid-range phones — baked lighting, aggressive LOD, ASTC/ETC2 textures, 30/60 FPS target.

Tier 2: high-end phones — higher texture resolution, dynamic shadows, reflection probes, denser traffic, 60 FPS target.

Tier 3: desktop/console-style validation — highest asset LOD, higher post-processing, larger world streaming budget.

## Current completed work

- A Blender 5.2 HD asset-lab scene was generated with 229 objects.
- The hero supercar contains 46 dedicated parts including wheels, brake discs, calipers, lights, body panels, glass, carbon sections, cockpit hints, and aero.
- A 70 m road test section includes lane markings, guard rails, buildings, trees, and street lights.
- The Blender scene exported a committed GLB and editable BLEND artifact.
- Unity 6 and Unreal 5.8 source scaffolds are committed on the `hd-racing-v2` branch.
