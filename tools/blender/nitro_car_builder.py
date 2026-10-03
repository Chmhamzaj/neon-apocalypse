"""Blender 5.2 procedural HD asset builder.
Run inside Blender 5.2 with --background --python.
Outputs an editable scene; export GLB/FBX from Blender as the engine interchange format.
"""
import bpy, math

def clear():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)

def material(name, rgba, metallic=0.0, roughness=0.5):
    m=bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes=True
    bs=m.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=rgba
    bs.inputs["Metallic"].default_value=metallic
    bs.inputs["Roughness"].default_value=roughness
    return m

def box(name, dims, loc, mat, bevel=0.0):
    bpy.ops.mesh.primitive_cube_add(location=loc)
    o=bpy.context.object
    o.name=name
    o.dimensions=dims
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if bevel:
        mod=o.modifiers.new("Bevel","BEVEL")
        mod.width=bevel
        mod.segments=4
    o.data.materials.append(mat)
    return o

def build():
    clear()
    paint=material("CarPaint",(0.05,0.16,0.48,1),0.75,0.2)
    dark=material("Carbon",(0.01,0.012,0.018,1),0.7,0.3)
    rubber=material("Rubber",(0.008,0.008,0.01,1),0,0.82)
    chrome=material("Rim",(0.42,0.45,0.50,1),0.92,0.15)

    box("Body",(2.0,4.8,0.58),(0,0,0.65),paint,0.18)
    box("UpperBody",(1.85,3.5,0.5),(0,-0.1,0.98),paint,0.22)
    box("FrontSplitter",(1.9,0.45,0.16),(0,2.2,0.45),dark,0.05)
    for x in (-1.03,1.03):
        for y in (-1.62,1.62):
            bpy.ops.mesh.primitive_torus_add(major_radius=0.43, minor_radius=0.17, major_segments=64, minor_segments=20,
                                              location=(x,y,0.52), rotation=(math.pi/2,0,0))
            t=bpy.context.object; t.name=f"Tire_{x}_{y}"; t.data.materials.append(rubber)
            bpy.ops.mesh.primitive_cylinder_add(vertices=48, radius=0.31, depth=0.16, location=(x,y,0.52), rotation=(math.pi/2,0,0))
            bpy.context.object.data.materials.append(chrome)

    for x in (-0.62,0.62):
        box("Headlamp",(0.40,0.07,0.18),(x,2.42,0.84),chrome,0.02)
        box("TailLamp",(0.44,0.06,0.16),(x,-2.42,0.84),chrome,0.02)

    bpy.ops.wm.save_as_mainfile(filepath="NitroStreetRush_HeroCar.blend")

if __name__ == "__main__":
    build()
