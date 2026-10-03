extends Node3D
## STREET SOVEREIGN 3D — mobile-first visual rebuild.
## Original procedural assets; optimized for Android using reusable meshes and batched scenery.

const WORLD := 190.0
const PLAYER_START := Vector3(-58, 0.1, 54)
# One shared human specification is used by the hero and every civilian.
# This removes per-NPC scale drift and keeps physics/visual proportions identical.
const HUMAN_HEIGHT:float = 1.72
const HUMAN_COLLIDER_RADIUS:float = 0.28
const HUMAN_COLLIDER_HEIGHT:float = 1.82

var player: CharacterBody3D
var player_visual: Node3D
var camera: Camera3D
var camera_pivot: Node3D
var sun: DirectionalLight3D
var world_env: WorldEnvironment
var city: Node3D
var cars: Array[Node3D] = []
var npcs: Array[Node3D] = []
var health := 100.0
var money := 12500
var wanted := 0
var mission := 0
var graphics := 1
var elapsed := 0.0
var rng := RandomNumberGenerator.new()
var move_input := Vector2.ZERO
var shooting := false
var driving := false
var gyro_enabled := true
var look_touching := false
var last_touch := Vector2.ZERO
var camera_yaw := 0.0
var camera_pitch := -12.0
var fire_cooldown := 0.0
var quality_changed := false
var boot_label: Label
var player_arms: Array[Node3D] = []
var player_legs: Array[Node3D] = []
var player_head: Node3D
var player_body: Node3D
var player_anim_phase := 0.0
var player_neck: Node3D
var player_jaw: Node3D
var player_mouth: Node3D
var player_eye_nodes: Array[Node3D] = []
var player_pupil_nodes: Array[Node3D] = []
var car_wheel_sets: Array[Array] = []
var joystick_id := -1
var look_id := -1
var joystick_center := Vector2.ZERO
var joystick_radius := 82.0
var jump_requested := false
var engine_audio: AudioStreamPlayer
var ambience_audio: AudioStreamPlayer
var fps_label: Label
var sensitivity := 0.12
var camera_distance := 7.0
var camera_height := 2.8
var external_human_scene: PackedScene
var external_car_scene: PackedScene
var external_car_scenes: Array[PackedScene] = []
var kenney_building_paths:Array[String]=[]
var kenney_car_paths:Array[String]=[]
var kenney_tree_paths:Array[String]=[]
var kenney_street_prop_paths:Array[String]=[]
var kenney_furniture_paths:Array[String]=[]
var kenney_food_paths:Array[String]=[]
var kenney_graveyard_paths:Array[String]=[]
var kenney_space_paths:Array[String]=[]
var kenney_total_paths:Array[String]=[]
var drive_button: Button
var active_car: Node3D
var building_obstacles: Array[Rect2] = []
var sidewalk_waypoints: Array[Vector3] = []
var external_lamp_scene: PackedScene
var asphalt_pbr: StandardMaterial3D
var concrete_pbr: StandardMaterial3D
var player_model_animation: AnimationPlayer
var player_idle_anim := ""
var player_walk_anim := ""
var player_run_anim := ""
var ui_layer: CanvasLayer
var settings_panel: Panel
var health_bar: ProgressBar
var stamina_bar: ProgressBar
var mission_value: Label
var district_value: Label
var cash_value: Label
var wanted_value: Label
var map_panel: Panel
var joystick_ring: Panel
var joystick_knob: Panel

var hud: Label
var objective: Label
var status: Label
var crosshair: Label
var graphics_button: Button
var gyro_button: Button
var mission_marker: MeshInstance3D

var missions := [
    ["SUNSET RUN", Vector3(-58,0.5,54)],
    ["NEON PACKAGE", Vector3(52,0.5,40)],
    ["RIVER DISTRICT", Vector3(57,0.5,-42)],
    ["HIGHWAY HEAT", Vector3(-52,0.5,-55)],
    ["NIGHT WOLVES", Vector3(68,0.5,68)],
    ["CITY TAKEOVER", Vector3(0,0.5,-76)]
]
var districts := ["VICE RAY", "SAN VALORA", "LIBERTY BAY"]

func _ready():
    rng.seed = 90210
    RenderingServer.set_default_clear_color(Color("#050a18"))
    _setup_ui()
    _spawn_player()
    boot_label.text = "STARTING • CAMERA ONLINE"
    set_process(true)
    call_deferred("_boot_world")

func _boot_world():
    await get_tree().process_frame
    boot_label.text = "LOADING • WORLD LIGHTING"
    _setup_world()
    _prepare_external_assets()
    await get_tree().process_frame
    boot_label.text = "LOADING • CITY 10%"
    await _build_city_async()
    boot_label.text = "LOADING • EXTERNAL 3D ASSETS"
    _prepare_external_assets()
    await get_tree().process_frame
    _replace_player_with_external_asset()
    boot_label.text = "LOADING • PEOPLE & TRAFFIC"
    await _spawn_population_async()
    _new_mission()
    _apply_quality()
    boot_label.text = "STREET SOVEREIGN • READY"
    await get_tree().create_timer(0.7).timeout
    if is_instance_valid(boot_label):
        boot_label.queue_free()

func _prepare_external_assets():
    if ResourceLoader.exists("res://assets/external/human.glb"):
        external_human_scene=load("res://assets/external/human.glb") as PackedScene
    if ResourceLoader.exists("res://assets/external/CarConcept.glb"):
        external_car_scene=load("res://assets/external/CarConcept.glb") as PackedScene
    external_car_scenes.clear()
    var fleet:=["sedan","sedan-sports","suv","suv-luxury","hatchback-sports","taxi","police","ambulance","firetruck","truck","van","delivery","race","CarConcept"]
    for model_name in fleet:
        var path:="res://assets/external/kenney-cars/%s.glb"%model_name
        if model_name=="CarConcept":
            path="res://assets/external/CarConcept.glb"
        if ResourceLoader.exists(path):
            var scene:=load(path) as PackedScene
            if scene: external_car_scenes.append(scene)
    if ResourceLoader.exists("res://assets/external/polyhaven/street_lamp_01/street_lamp_01_2k.gltf"):
        external_lamp_scene=load("res://assets/external/polyhaven/street_lamp_01/street_lamp_01_2k.gltf") as PackedScene
    _prepare_kenney_asset_catalog()
    asphalt_pbr=_make_pbr_material(
        "res://assets/external/polyhaven/asphalt_07_diff_2k.jpg",
        "res://assets/external/polyhaven/asphalt_07_nor_gl_2k.jpg",
        "res://assets/external/polyhaven/asphalt_07_rough_2k.jpg",
        Vector3(3.0,3.0,3.0)
    )
    concrete_pbr=_make_pbr_material(
        "res://assets/external/polyhaven/concrete_diff_2k.jpg",
        "res://assets/external/polyhaven/concrete_nor_gl_2k.jpg",
        "res://assets/external/polyhaven/concrete_rough_2k.jpg",
        Vector3(1.25,1.25,1.25)
    )

func _scan_glb_paths(root_path:String)->Array[String]:
    var results:Array[String]=[]
    var dir:=DirAccess.open(root_path)
    if dir==null:
        return results
    dir.list_dir_begin()
    while true:
        var file_name:String=dir.get_next()
        if file_name=="":
            break
        if file_name.begins_with("."):
            continue
        var full_path:String=root_path+"/"+file_name
        if dir.current_is_dir():
            results.append_array(_scan_glb_paths(full_path))
        elif file_name.to_lower().ends_with(".glb"):
            results.append(full_path)
    dir.list_dir_end()
    results.sort()
    return results

func _prepare_kenney_asset_catalog():
    var base:="res://assets/external/kenney-library/packs"
    kenney_building_paths.clear()
    kenney_car_paths.clear()
    kenney_tree_paths.clear()
    kenney_street_prop_paths.clear()
    kenney_furniture_paths.clear()
    kenney_food_paths.clear()
    kenney_graveyard_paths.clear()
    kenney_space_paths.clear()
    kenney_total_paths=_scan_glb_paths(base)

    var commercial:Array[String]=_scan_glb_paths(base+"/city-kit-commercial")
    var suburban:Array[String]=_scan_glb_paths(base+"/city-kit-suburban")
    var building_sources:Array[String]=[]
    building_sources.append_array(commercial)
    building_sources.append_array(suburban)
    for path:String in building_sources:
        var n:String=path.get_file().to_lower()
        if n.contains("building"):
            kenney_building_paths.append(path)

    for path:String in _scan_glb_paths(base+"/car-kit"):
        var n:String=path.get_file().to_lower()
        if not n.contains("debris") and not n.contains("wheel") and not n.contains("cone") and not n.contains("barrier"):
            kenney_car_paths.append(path)

    for path:String in _scan_glb_paths(base+"/nature-kit"):
        var n:String=path.get_file().to_lower()
        if n.contains("tree") or n.contains("bush") or n.contains("plant") or n.contains("flower") or n.contains("grass"):
            kenney_tree_paths.append(path)

    for path:String in _scan_glb_paths(base+"/city-kit-roads"):
        var n:String=path.get_file().to_lower()
        if not n.contains("road") and not n.contains("sidewalk") and not n.contains("tile") and not n.contains("lane"):
            kenney_street_prop_paths.append(path)

    for path:String in _scan_glb_paths(base+"/furniture-kit"):
        var n:String=path.get_file().to_lower()
        if n.contains("chair") or n.contains("table") or n.contains("bench") or n.contains("lamp") or n.contains("trash") or n.contains("plant") or n.contains("shelf") or n.contains("cabinet"):
            kenney_furniture_paths.append(path)

    for path:String in _scan_glb_paths(base+"/food-kit"):
        kenney_food_paths.append(path)

    for path:String in _scan_glb_paths(base+"/graveyard-kit"):
        kenney_graveyard_paths.append(path)

    for path:String in _scan_glb_paths(base+"/space-kit"):
        kenney_space_paths.append(path)

func _instantiate_glb(path:String)->Node3D:
    var packed:=load(path) as PackedScene
    if packed==null:
        return null
    return packed.instantiate() as Node3D

func _normalize_model_to_box(root:Node3D,target_size:Vector3)->float:
    var min_v:=Vector3(INF,INF,INF)
    var max_v:=Vector3(-INF,-INF,-INF)
    var found:=false
    for mesh_node in root.find_children("*","MeshInstance3D",true,false):
        var mi:=mesh_node as MeshInstance3D
        if mi==null or mi.mesh==null:
            continue
        var a:=mi.get_aabb()
        var corners:Array[Vector3]=[
            Vector3(a.position.x,a.position.y,a.position.z),
            Vector3(a.end.x,a.position.y,a.position.z),
            Vector3(a.position.x,a.end.y,a.position.z),
            Vector3(a.position.x,a.position.y,a.end.z),
            Vector3(a.end.x,a.end.y,a.position.z),
            Vector3(a.end.x,a.position.y,a.end.z),
            Vector3(a.position.x,a.end.y,a.end.z),
            Vector3(a.end.x,a.end.y,a.end.z)]
        for corner in corners:
            var world_p:Vector3=mi.to_global(corner)
            var local_p:Vector3=root.to_local(world_p)
            min_v.x=min(min_v.x,local_p.x)
            min_v.y=min(min_v.y,local_p.y)
            min_v.z=min(min_v.z,local_p.z)
            max_v.x=max(max_v.x,local_p.x)
            max_v.y=max(max_v.y,local_p.y)
            max_v.z=max(max_v.z,local_p.z)
            found=true
    if not found:
        return 1.0
    var size:=max_v-min_v
    if size.x<0.01 or size.y<0.01 or size.z<0.01:
        return 1.0
    var scale_factor:float=min(target_size.x/size.x,min(target_size.y/size.y,target_size.z/size.z))
    root.scale*=scale_factor
    var center_x:float=(min_v.x+max_v.x)*0.5
    var center_z:float=(min_v.z+max_v.z)*0.5
    root.position=Vector3(-center_x*scale_factor,-min_v.y*scale_factor,-center_z*scale_factor)
    return scale_factor

func _make_pbr_material(albedo_path:String,normal_path:String,rough_path:String,tiling:Vector3)->StandardMaterial3D:
    var m:=StandardMaterial3D.new()
    if ResourceLoader.exists(albedo_path):
        m.albedo_texture=load(albedo_path) as Texture2D
    if ResourceLoader.exists(normal_path):
        m.normal_enabled=true
        m.normal_texture=load(normal_path) as Texture2D
    if ResourceLoader.exists(rough_path):
        m.roughness_texture=load(rough_path) as Texture2D
        m.roughness_texture_channel=BaseMaterial3D.TEXTURE_CHANNEL_RED
    m.roughness=0.78
    m.uv1_scale=tiling
    return m

func _normalize_model_height(root:Node3D,target_height:float)->float:
    # Normalize from the complete rendered mesh bounds, including nested GLB transforms.
    # This prevents imported humanoid assets with unusual source units from becoming oversized.
    var min_y:float=INF
    var max_y:float=-INF
    var found:=false
    for mesh_node in root.find_children("*","MeshInstance3D",true,false):
        var mi:=mesh_node as MeshInstance3D
        if mi==null or mi.mesh==null:
            continue
        var a:=mi.get_aabb()
        var corners:Array[Vector3]=[
            Vector3(a.position.x,a.position.y,a.position.z),
            Vector3(a.end.x,a.position.y,a.position.z),
            Vector3(a.position.x,a.end.y,a.position.z),
            Vector3(a.position.x,a.position.y,a.end.z),
            Vector3(a.end.x,a.end.y,a.position.z),
            Vector3(a.end.x,a.position.y,a.end.z),
            Vector3(a.position.x,a.end.y,a.end.z),
            Vector3(a.end.x,a.end.y,a.end.z)]
        for corner in corners:
            var world_p:Vector3=mi.to_global(corner)
            var local_p:Vector3=root.to_local(world_p)
            min_y=min(min_y,local_p.y)
            max_y=max(max_y,local_p.y)
            found=true
    if not found or max_y-min_y<0.01:
        return 1.0
    var current_height:float=max_y-min_y
    var scale_factor:float=target_height/current_height
    root.scale*=scale_factor
    root.position.y=-min_y*scale_factor
    return scale_factor

func _tint_model(model:Node3D,index:int):
    # Preserve embedded textures while giving NPCs distinct clothing/overall palette variation.
    var tints=[Color("#d7b79c"),Color("#9db7d9"),Color("#b7d09f"),Color("#d3a5c2"),Color("#c5a777"),Color("#9bc6c5")]
    var tint:Color=tints[index%tints.size()]
    for mesh_node in model.find_children("*","MeshInstance3D",true,false):
        var mi:=mesh_node as MeshInstance3D
        if mi==null: continue
        var count:=mi.get_surface_override_material_count()
        if count<=0 and mi.mesh:
            count=mi.mesh.get_surface_count()
        for si in range(count):
            var base:Material=mi.get_active_material(si)
            if base is StandardMaterial3D:
                var m:StandardMaterial3D=(base as StandardMaterial3D).duplicate()
                m.albedo_color=Color(
                    clamp(m.albedo_color.r*tint.r*1.35,0.0,1.0),
                    clamp(m.albedo_color.g*tint.g*1.35,0.0,1.0),
                    clamp(m.albedo_color.b*tint.b*1.35,0.0,1.0),
                    m.albedo_color.a)
                mi.set_surface_override_material(si,m)

func _replace_player_with_external_asset():
    if external_human_scene==null or not is_instance_valid(player):
        return
    if is_instance_valid(player_visual):
        player_visual.queue_free()
    player_arms.clear()
    player_legs.clear()
    player_eye_nodes.clear()
    player_pupil_nodes.clear()
    player_visual=Node3D.new()
    player_visual.name="HeroRealHuman"
    player.add_child(player_visual)
    var model:=external_human_scene.instantiate()
    player_visual.add_child(model)
    _normalize_model_height(model,HUMAN_HEIGHT)
    _tint_model(model,0)
    player_model_animation=_find_animation_player(model)
    if player_model_animation:
        # The imported rig's animation tracks are responsible for the detached/
        # oversized limb artifact seen on mobile. Use the clean bind pose and
        # animate the character root procedurally instead.
        player_model_animation.stop()
        player_model_animation.active=false
        player_idle_anim=""
        player_walk_anim=""
        player_run_anim=""


func _find_animation_player(root:Node)->AnimationPlayer:
    var players:=root.find_children("*","AnimationPlayer",true,false)
    if players.size()>0:
        return players[0] as AnimationPlayer
    return null

func _find_animation(ap:AnimationPlayer,wanted:Array[String])->String:
    if ap==null:
        return ""
    for anim_name in ap.get_animation_list():
        var lower:=anim_name.to_lower()
        for token in wanted:
            if lower.contains(token):
                return anim_name
    return ""

func _build_city_async():
    city = Node3D.new()
    city.name = "OptimizedMegaCity"
    add_child(city)
    var district_centers := [-62.0, -20.0, 24.0, 67.0]
    var palettes := [
        [Color("#242b46"),Color("#47517a"),Color("#f04f79")],
        [Color("#263c42"),Color("#426e78"),Color("#3ed6e8")],
        [Color("#352c42"),Color("#65506f"),Color("#ffbf4b")],
        [Color("#263d35"),Color("#4e765f"),Color("#8ef08d")]
    ]
    var total:=72
    var done:=0
    for ds in range(4):
        for i in range(18):
            var bx: float = float(district_centers[ds]) + rng.randf_range(-16.0,16.0)
            var bz: float = rng.randf_range(-82.0,82.0)
            if abs(bx) < 8: bx += 12.0
            _create_building(bx,bz,rng.randf_range(6,11),rng.randf_range(8,30),rng.randf_range(6,12),palettes[ds],i)
            done+=1
            if done%4==0:
                boot_label.text="LOADING • CITY %d%%"%int(float(done)/total*100.0)
                await get_tree().process_frame
    _build_roads()
    await get_tree().process_frame
    _build_landmarks()
    _build_instanced_props()
    box(city,Vector3(0,-0.15,0),Vector3(10,0.25,WORLD),material(Color("#0b3454"),0.15,0.55),"River")
    box(city,Vector3(-5.8,0.08,0),Vector3(0.35,0.18,WORLD),material(Color("#21a8d8"),0.2,0.3,Color("#21a8d8")),"RiverGlow")
    box(city,Vector3(5.8,0.08,0),Vector3(0.35,0.18,WORLD),material(Color("#21a8d8"),0.2,0.3,Color("#21a8d8")),"RiverGlow")
    for z in [-62.0,0.0,62.0]:
        _create_bridge(z)
        await get_tree().process_frame

func _spawn_population_async():
    _build_sidewalk_waypoints()
    for i in range(20):
        var n:=CharacterBody3D.new()
        n.name="Civilian_%02d"%i
        n.collision_layer=2
        n.collision_mask=7
        n.safe_margin=0.08
        n.max_slides=6
        n.floor_snap_length=0.15
        var npc_shape:=CollisionShape3D.new()
        var npc_capsule:=CapsuleShape3D.new()
        # Civilians use the EXACT same physical body dimensions as the hero.
        npc_capsule.radius=HUMAN_COLLIDER_RADIUS
        npc_capsule.height=HUMAN_COLLIDER_HEIGHT
        npc_shape.shape=npc_capsule
        npc_shape.position.y=HUMAN_COLLIDER_HEIGHT*0.5
        n.add_child(npc_shape)
        n.position=_find_npc_spawn_point()
        if external_human_scene:
            _create_external_human(n,i)
        else:
            _create_npc_visual(n,i)
        add_child(n)
        n.set_meta("phase",rng.randf_range(0,TAU))
        n.set_meta("speed",rng.randf_range(0.34,0.48))
        n.set_meta("target",_random_sidewalk_point(n.position))
        npcs.append(n)
        if i%3==0:
            await get_tree().process_frame

    for i in range(20):
        var c:=AnimatableBody3D.new()
        c.name="Traffic_%02d"%i
        c.collision_layer=4
        c.collision_mask=3
        var car_shape:=CollisionShape3D.new()
        var car_box:=BoxShape3D.new()
        car_box.size=Vector3(2.05,1.35,4.45)
        car_shape.shape=car_box
        car_shape.position.y=0.68
        c.add_child(car_shape)
        c.position=Vector3([ -82.0,-40.0,42.0,82.0 ][i%4],0.0,rng.randf_range(-88,88))
        if i==0:
            c.position=PLAYER_START+Vector3(4.0,0.0,3.5)
            c.set_meta("player_car",true)
            c.set_meta("speed",0.0)
            c.set_meta("dir",1.0)
        var fleet_index:int=i%max(1,kenney_car_paths.size())
        if not kenney_car_paths.is_empty():
            _create_kenney_car(c,i)
        elif external_car_scenes.size()>0:
            fleet_index=i%max(1,external_car_scenes.size())
            _create_external_car(c,fleet_index)
        elif external_car_scene:
            _create_external_car_from_scene(c,external_car_scene,fleet_index)
        else:
            _create_car(c,i)
        if not bool(c.get_meta("player_car",false)):
            c.set_meta("speed",rng.randf_range(4.0,8.0))
            c.set_meta("dir",1.0 if i%2==0 else -1.0)
        c.set_meta("lane",c.position.x)
        cars.append(c)
        add_child(c)
        if i%3==0:
            await get_tree().process_frame

func _create_external_human(root:Node3D,index:int):
    if external_human_scene==null:
        return
    var model:=external_human_scene.instantiate()
    root.add_child(model)
    # Keep civilians clearly human-sized and a little smaller than the hero,
    # with gentle variation so the crowd does not look cloned.
    # Match the hero's rendered height EXACTLY. No height variation.
    var target_height:float=HUMAN_HEIGHT
    _normalize_model_height(model,target_height)
    _tint_model(model,index+1)
    # Imported walk animation can contain transform tracks. Preserve the
    # normalized root scale every frame so walking never changes character size.
    root.set_meta("visual_height",target_height)
    root.set_meta("base_model_scale",model.scale)
    var ap:=_find_animation_player(model)
    root.set_meta("model",model)
    root.set_meta("anim_player",ap)
    root.set_meta("idle_anim",_find_animation(ap,["idle","stand"]))
    root.set_meta("walk_anim",_find_animation(ap,["walk"]))
    root.set_meta("run_anim",_find_animation(ap,["run"]))
    if ap:
        var idle_name:String=String(root.get_meta("idle_anim"))
        if idle_name!="":
            ap.play(idle_name)

func _create_kenney_car(root:Node3D,index:int):
    if kenney_car_paths.is_empty():
        return
    var model:=_instantiate_glb(kenney_car_paths[index%kenney_car_paths.size()])
    if model==null:
        return
    root.add_child(model)
    _normalize_model_length(model,4.35)
    model.position.y=0.05
    root.set_meta("model",model)
    root.set_meta("fleet_index",index%kenney_car_paths.size())
    root.set_meta("source","Kenney Car Kit")
    _add_exhaust_smoke(root,Vector3(0,0.28,2.0))

func _create_external_car(root:Node3D,index:int):
    if external_car_scenes.is_empty():
        return
    var model:=external_car_scenes[index%external_car_scenes.size()].instantiate()
    root.add_child(model)
    _normalize_model_length(model,4.35)
    model.position.y=0.05
    root.set_meta("model",model)
    root.set_meta("fleet_index",index%external_car_scenes.size())
    _add_exhaust_smoke(root,Vector3(0,0.28,2.0))

func _create_external_car_from_scene(root:Node3D,scene:PackedScene,index:int):
    if scene==null:
        return
    var model:=scene.instantiate()
    root.add_child(model)
    _normalize_model_length(model,4.35)
    model.position.y=0.05
    root.set_meta("model",model)
    root.set_meta("fleet_index",index)
    _add_exhaust_smoke(root,Vector3(0,0.28,2.0))

func _normalize_model_length(root:Node3D,target_length:float):
    var min_z:float=INF
    var max_z:float=-INF
    var found:=false
    for mesh_node in root.find_children("*","MeshInstance3D",true,false):
        var mi:=mesh_node as MeshInstance3D
        if mi==null or mi.mesh==null:
            continue
        var a:=mi.get_aabb()
        var corners:Array[Vector3]=[
            Vector3(a.position.x,a.position.y,a.position.z),
            Vector3(a.end.x,a.position.y,a.position.z),
            Vector3(a.position.x,a.position.y,a.end.z),
            Vector3(a.end.x,a.position.y,a.end.z)]
        for c in corners:
            var local_p:=root.to_local(mi.to_global(c))
            min_z=min(min_z,local_p.z)
            max_z=max(max_z,local_p.z)
            found=true
    if found and max_z-min_z>0.01:
        var f:float=target_length/(max_z-min_z)
        root.scale*=f
        root.position.z=-min_z*f

func material(color: Color, roughness := 0.72, metallic := 0.0, emission := Color.TRANSPARENT) -> StandardMaterial3D:
    var m := StandardMaterial3D.new()
    m.albedo_color = color
    m.roughness = roughness
    m.metallic = metallic
    if emission != Color.TRANSPARENT:
        m.emission_enabled = true
        m.emission = emission
        m.emission_energy_multiplier = 2.0
    return m

func box(parent: Node3D, pos: Vector3, size: Vector3, mat: Material, node_name := "Box") -> MeshInstance3D:
    var n := MeshInstance3D.new()
    n.name = node_name
    var mesh := BoxMesh.new()
    mesh.size = size
    n.mesh = mesh
    n.position = pos
    n.material_override = mat
    parent.add_child(n)
    return n

func cylinder(parent: Node3D, pos: Vector3, radius: float, height: float, mat: Material, node_name := "Cylinder") -> MeshInstance3D:
    var n := MeshInstance3D.new()
    n.name = node_name
    var mesh := CylinderMesh.new()
    mesh.top_radius = radius
    mesh.bottom_radius = radius
    mesh.height = height
    mesh.radial_segments = 10
    n.mesh = mesh
    n.position = pos
    n.material_override = mat
    parent.add_child(n)
    return n

func _setup_world():
    world_env = WorldEnvironment.new()
    var env: Environment = Environment.new()
    env.background_mode = Environment.BG_COLOR
    env.background_color = Color("#081226")
    env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
    env.ambient_light_color = Color("#8fa8c8")
    env.ambient_light_energy = 0.9
    env.tonemap_mode = Environment.TONE_MAPPER_FILMIC
    env.glow_enabled = false
    world_env.environment = env
    add_child(world_env)

    sun = DirectionalLight3D.new()
    sun.rotation_degrees = Vector3(-52,-28,0)
    sun.light_energy = 1.2
    sun.light_color = Color("#fff1dc")
    sun.shadow_enabled = false
    sun.directional_shadow_max_distance = 75.0
    add_child(sun)

    var ground := StaticBody3D.new()
    ground.name = "Ground"
    ground.collision_layer=1
    ground.collision_mask=7
    add_child(ground)
    box(ground, Vector3(0,-1.1,0), Vector3(WORLD,2,WORLD), material(Color("#26332d")), "GroundMesh")
    var shape := CollisionShape3D.new()
    var bs := BoxShape3D.new()
    bs.size = Vector3(WORLD,2,WORLD)
    shape.shape = bs
    shape.position.y = -1.1
    ground.add_child(shape)

func _build_optimized_city():
    city = Node3D.new()
    city.name = "OptimizedMegaCity"
    add_child(city)

    # Four strong visual districts, fewer expensive nodes, richer individual buildings.
    var district_centers := [-62.0, -20.0, 24.0, 67.0]
    var palettes := [
        [Color("#242b46"),Color("#47517a"),Color("#f04f79")],
        [Color("#263c42"),Color("#426e78"),Color("#3ed6e8")],
        [Color("#352c42"),Color("#65506f"),Color("#ffbf4b")],
        [Color("#263d35"),Color("#4e765f"),Color("#8ef08d")]
    ]
    for s in range(4):
        for i in range(30):
            var bx: float = float(district_centers[s]) + rng.randf_range(-16.0,16.0)
            var bz := rng.randf_range(-82,82)
            if abs(bx) < 8: bx += 12.0
            var h: float = rng.randf_range(8.0,30.0)
            var w: float = rng.randf_range(6.0,11.0)
            var d: float = rng.randf_range(6.0,12.0)
            _create_building(bx,bz,w,h,d,palettes[s],i)
    _build_roads()
    _build_landmarks()
    _build_instanced_props()

    # River with emissive banks.
    box(city,Vector3(0,-0.15,0),Vector3(10,0.25,WORLD),material(Color("#0b3454"),0.15,0.55),"River")
    box(city,Vector3(-5.8,0.08,0),Vector3(0.35,0.18,WORLD),material(Color("#21a8d8"),0.2,0.3,Color("#21a8d8")),"RiverGlow")
    box(city,Vector3(5.8,0.08,0),Vector3(0.35,0.18,WORLD),material(Color("#21a8d8"),0.2,0.3,Color("#21a8d8")),"RiverGlow")
    for z in [-62.0,0.0,62.0]:
        _create_bridge(z)

func _create_building(x:float,z:float,w:float,h:float,d:float,palette:Array,i:int):
    building_obstacles.append(Rect2(x-w*0.5,z-d*0.5,w,d))
    var root:=Node3D.new()
    root.position=Vector3(x,0,z)
    city.add_child(root)

    if not kenney_building_paths.is_empty():
        var model:=_instantiate_glb(kenney_building_paths[i%kenney_building_paths.size()])
        if model:
            root.add_child(model)
            # Fit every imported building to the same real-world street scale.
            _normalize_model_to_box(model,Vector3(w*0.96,h*0.98,d*0.96))
            model.set_meta("source","Kenney City Kit")
            var body:=StaticBody3D.new()
            body.name="BuildingCollision"
            body.collision_layer=1
            body.collision_mask=7
            var shape:=CollisionShape3D.new()
            var box_shape:=BoxShape3D.new()
            box_shape.size=Vector3(w*0.92,h*0.96,d*0.92)
            shape.shape=box_shape
            shape.position.y=h*0.48
            body.add_child(shape)
            root.add_child(body)
            return

    # Safe fallback when an asset pack is unavailable.
    var facade:Color=palette[i%2]
    var facade_mat:=concrete_pbr.duplicate() if concrete_pbr else material(facade,0.65)
    facade_mat.albedo_color=facade
    box(root,Vector3(0,h*0.5,0),Vector3(w,h,d),facade_mat,"Building")
    var body:=StaticBody3D.new()
    body.name="BuildingCollision"
    body.collision_layer=1
    body.collision_mask=7
    var shape:=CollisionShape3D.new()
    var box_shape:=BoxShape3D.new()
    box_shape.size=Vector3(w,h,d)
    shape.shape=box_shape
    shape.position.y=h*0.5
    body.add_child(shape)
    root.add_child(body)
func _build_roads():
    var road_mat:Material=asphalt_pbr if asphalt_pbr else material(Color("#14171c"),0.92)
    var lane_mat:=material(Color("#d7b24f"),0.48,0.05)
    var sidewalk_mat:Material=concrete_pbr if concrete_pbr else material(Color("#555a62"),0.88)
    for z in range(-84,85,21):
        box(city,Vector3(0,-0.02,z),Vector3(WORLD,0.18,6.5),road_mat,"Road")
        box(city,Vector3(0,0.08,z+4.15),Vector3(WORLD,0.16,1.2),sidewalk_mat,"Sidewalk")
        box(city,Vector3(0,0.08,z-4.15),Vector3(WORLD,0.16,1.2),sidewalk_mat,"Sidewalk")
        for x in range(-88,89,12):
            box(city,Vector3(x,0.10,z),Vector3(4.2,0.035,0.13),lane_mat,"Lane")
    for x in [-82.0,-40.0,42.0,82.0]:
        box(city,Vector3(x,0.0,0),Vector3(6.5,0.18,WORLD),road_mat,"Road")
        box(city,Vector3(x+4.15,0.08,0),Vector3(1.2,0.16,WORLD),sidewalk_mat,"Sidewalk")
        box(city,Vector3(x-4.15,0.08,0),Vector3(1.2,0.16,WORLD),sidewalk_mat,"Sidewalk")
        for z in range(-88,89,12):
            box(city,Vector3(x,0.10,z),Vector3(0.13,0.035,4.2),lane_mat,"Lane")

func _build_landmarks():
    var neon := [Color("#ff477e"),Color("#38d8ef"),Color("#ffc857"),Color("#8cf38c")]
    for i in range(4):
        var x: float = [-62.0,-20.0,24.0,67.0][i]
        var root := Node3D.new()
        root.position = Vector3(x,0,-78)
        city.add_child(root)
        box(root,Vector3(0,4,0),Vector3(20,8,3.5),material(Color("#171b27"),0.4,0.25),"Landmark")
        box(root,Vector3(0,7.7,1.85),Vector3(16,1.0,0.16),material(neon[i],0.2,0.25,neon[i]),"Sign")
        for k in range(5):
            cylinder(root,Vector3(-7+k*3.5,2.2,2.1),0.13,4.4,material(Color("#262b36"),0.5,0.2),"Light")

func _create_bridge(z:float):
    box(city,Vector3(0,0.8,z),Vector3(22,1.4,9),material(Color("#59616d"),0.5,0.18),"Bridge")
    for x in range(-9,10,3):
        cylinder(city,Vector3(x,2.4,z-3.5),0.10,3.0,material(Color("#d5b15a"),0.5,0.25),"BridgeLamp")

func _build_sidewalk_waypoints():
    sidewalk_waypoints.clear()
    var zs:Array[float]=[-84.0,-63.0,-42.0,-21.0,0.0,21.0,42.0,63.0,84.0]
    var xs:Array[float]=[-82.0,-40.0,42.0,82.0]
    for z in zs:
        for offset in [-4.15,4.15]:
            for x in range(-84,85,7):
                var p:=Vector3(float(x),0.0,z+offset)
                if not _walk_point_blocked(p):
                    sidewalk_waypoints.append(p)
    for x in xs:
        for offset in [-4.15,4.15]:
            for z in range(-84,85,7):
                var p:=Vector3(x+offset,0.0,float(z))
                if not _walk_point_blocked(p):
                    sidewalk_waypoints.append(p)

func _walk_point_blocked(p:Vector3)->bool:
    for r in building_obstacles:
        if r.grow(0.7).has_point(Vector2(p.x,p.z)):
            return true
    return false

func _nearest_safe_sidewalk_point(p:Vector3)->Vector3:
    var best:=Vector3(0,0,0)
    var best_d:=INF
    for wp in sidewalk_waypoints:
        var d:=Vector2(wp.x-p.x,wp.z-p.z).length()
        if d<best_d:
            best_d=d
            best=wp
    return best

func _next_sidewalk_point(from_pos:Vector3)->Vector3:
    if sidewalk_waypoints.is_empty():
        return from_pos
    var current:=_nearest_safe_sidewalk_point(from_pos)
    var candidates:Array[Vector3]=[]
    for wp in sidewalk_waypoints:
        var d:=wp.distance_to(current)
        if d<8.6 and d>0.2:
            candidates.append(wp)
    if candidates.is_empty():
        return current
    var best:=candidates[rng.randi_range(0,candidates.size()-1)]
    var best_score:float=-999.0
    var last_dir:=Vector2(from_pos.x-current.x,from_pos.z-current.z)
    for wp in candidates:
        var v:=Vector2(wp.x-current.x,wp.z-current.z).normalized()
        var score:float=rng.randf_range(0.0,0.25)
        if last_dir.length()>0.2:
            score += v.dot(last_dir.normalized())
        if score>best_score:
            best_score=score
            best=wp
    return best

func _random_sidewalk_point(from_pos:Vector3)->Vector3:
    return _next_sidewalk_point(from_pos)

func _is_on_road(p:Vector3)->bool:
    for z in range(-84,85,21):
        if abs(p.z-float(z))<4.2:
            return true
    for x in [-82.0,-40.0,42.0,82.0]:
        if abs(p.x-x)<4.2:
            return true
    return false

func _safe_prop_spot(p:Vector3,min_building_gap:float=1.5)->bool:
    if abs(p.x)<7.0:
        return false
    if _is_on_road(p):
        return false
    for r in building_obstacles:
        if r.grow(min_building_gap).has_point(Vector2(p.x,p.z)):
            return false
    return true

func _place_kenney_height_asset(path:String,position:Vector3,target_height:float,rotation_y:float=0.0)->Node3D:
    var node:=_instantiate_glb(path)
    if node==null:
        return null
    node.position=position
    node.rotation.y=rotation_y
    city.add_child(node)
    _normalize_model_height(node,target_height)
    return node

func _place_kenney_prop(path:String,position:Vector3,max_dimension:float,rotation_y:float=0.0)->Node3D:
    var node:=_instantiate_glb(path)
    if node==null:
        return null
    node.position=position
    node.rotation.y=rotation_y
    city.add_child(node)
    _normalize_model_to_box(node,Vector3(max_dimension,max_dimension,max_dimension))
    return node

func _build_instanced_props():
    # Trees and foliage: real 3D Nature Kit assets, scaled to adult-world proportions.
    if not kenney_tree_paths.is_empty():
        for i in range(92):
            var p:=Vector3(rng.randf_range(-90.0,90.0),0.0,rng.randf_range(-90.0,90.0))
            if not _safe_prop_spot(p,2.0):
                continue
            var tree_path:String=kenney_tree_paths[i%kenney_tree_paths.size()]
            var tree:=_place_kenney_height_asset(tree_path,p,rng.randf_range(3.2,6.2),rng.randf_range(0.0,TAU))
            if tree:
                tree.set_meta("source","Kenney Nature Kit")

    # Urban street furniture and public-space props.
    if not kenney_street_prop_paths.is_empty():
        for i in range(60):
            var wp:=_random_sidewalk_point(Vector3(rng.randf_range(-86,86),0,rng.randf_range(-86,86)))
            var p:=wp+Vector3(rng.randf_range(-0.7,0.7),0.0,rng.randf_range(-0.7,0.7))
            var prop_path:String=kenney_street_prop_paths[i%kenney_street_prop_paths.size()]
            var prop:=_place_kenney_prop(prop_path,p,rng.randf_range(0.6,1.8),rng.randf_range(0.0,TAU))
            if prop:
                prop.set_meta("source","Kenney City Kit Roads")

    # A smaller amount of furniture gives shop fronts and public areas physical detail.
    if not kenney_furniture_paths.is_empty():
        for i in range(28):
            var p:=Vector3(rng.randf_range(-86,86),0.0,rng.randf_range(-86,86))
            if not _safe_prop_spot(p,1.0):
                continue
            var prop:=_place_kenney_prop(kenney_furniture_paths[i%kenney_furniture_paths.size()],p,rng.randf_range(0.8,1.8),rng.randf_range(0.0,TAU))
            if prop:
                prop.set_meta("source","Kenney Furniture Kit")

    # Food props become small market/stall dressing around selected city blocks.
    if not kenney_food_paths.is_empty():
        for i in range(18):
            var p:=Vector3(rng.randf_range(-80,80),0.0,rng.randf_range(-80,80))
            if not _safe_prop_spot(p,1.0):
                continue
            var prop:=_place_kenney_prop(kenney_food_paths[i%kenney_food_paths.size()],p,rng.randf_range(0.35,1.0),rng.randf_range(0.0,TAU))
            if prop:
                prop.set_meta("source","Kenney Food Kit")

    # A few graveyard/space props create optional themed micro-zones without replacing the city.
    var special_paths:Array[String]=kenney_graveyard_paths+kenney_space_paths
    if not special_paths.is_empty():
        for i in range(14):
            var zone_center:Vector3=Vector3(74.0,0.0,74.0) if i<7 else Vector3(-74.0,0.0,-74.0)
            var p:=zone_center+Vector3(rng.randf_range(-9.0,9.0),0.0,rng.randf_range(-9.0,9.0))
            if not _safe_prop_spot(p,1.0):
                continue
            var prop:=_place_kenney_prop(special_paths[i%special_paths.size()],p,rng.randf_range(0.5,2.0),rng.randf_range(0.0,TAU))
            if prop:
                prop.set_meta("source","Kenney 3D Library")


func _spawn_player():
    player = CharacterBody3D.new()
    player.name = "Player"
    player.position = PLAYER_START
    add_child(player)
    var cs := CollisionShape3D.new()
    var capsule := CapsuleShape3D.new()
    capsule.radius = HUMAN_COLLIDER_RADIUS
    capsule.height = HUMAN_COLLIDER_HEIGHT
    cs.shape = capsule
    cs.position.y = HUMAN_COLLIDER_HEIGHT*0.5
    player.collision_layer = 1
    player.collision_mask = 7
    player.add_child(cs)

    player_visual = Node3D.new()
    player_visual.name = "HeroVisual"
    player.add_child(player_visual)
    _create_humanoid(player_visual)
    # Fallback procedural hero is authored around 2.6m; normalize it to a real adult height.
    player_visual.scale = Vector3(0.68,0.68,0.68)

    camera_pivot = Node3D.new()
    camera_pivot.position = Vector3(0,1.25,0)
    player.add_child(camera_pivot)
    camera = Camera3D.new()
    camera.fov = 67
    camera.near = 0.08
    camera.far = 180.0
    add_child(camera)
    camera.current = true
    camera.global_position = player.global_position + Vector3(0,3.0,6.5)
    camera.look_at(player.global_position + Vector3(0,1.2,0), Vector3.UP)

func _create_humanoid(root:Node3D):
    var skin:=material(Color("#c78369"),0.57)
    var skin_shadow:=material(Color("#8f5140"),0.62)
    var hair:=material(Color("#4c190e"),0.42)
    var hair_hi:=material(Color("#a63a18"),0.36)
    var leather:=material(Color("#28140d"),0.58)
    var armor:=material(Color("#4a5360"),0.27,0.72)
    var dark_armor:=material(Color("#1a222c"),0.33,0.68)
    var green:=material(Color("#173d2d"),0.50)
    var green_trim:=material(Color("#b49449"),0.36,0.42)
    var pants:=material(Color("#241c1b"),0.62)
    var boot_mat:=material(Color("#2b160d"),0.46,0.15)
    var eye_white:=material(Color("#f6f7fb"),0.16)
    var iris:=material(Color("#4f9a64"),0.12,0.06)
    var eye_dark:=material(Color("#17181c"),0.22)
    var lip:=material(Color("#7a3041"),0.42)
    var mouth_dark:=material(Color("#260e13"),0.35)

    # Organic silhouette: tapered chest, pelvis, limbs, no rectangular torso.
    player_body=capsule(root,Vector3(0,1.34,0),0.33,0.96,leather,"Torso")
    player_body.scale=Vector3(1.12,1.0,0.72)
    capsule(root,Vector3(0,1.03,0),0.30,0.34,pants,"Hip")
    box(root,Vector3(0,-0.325,1.38),Vector3(0.21,0.018,0.25),dark_armor,"ChestPlate")

    player_neck=cylinder(root,Vector3(0,1.91,0),0.145,0.18,skin,"Neck")
    var head:=sphere_part(root,Vector3(0,2.24,0),0.34,skin,"Head")
    head.scale=Vector3(0.99,1.06,0.92)
    player_head=head

    # Hair mass with layered front locks.
    sphere_part(root,Vector3(0,2.49,0.015),0.35,hair,"HairMass")
    var hair_specs:Array=[[-0.26,2.49],[-0.13,2.58],[0.0,2.62],[0.14,2.58],[0.27,2.50]]
    for spec in hair_specs:
        var lock:=sphere_part(root,Vector3(float(spec[0]),-0.25,float(spec[1])),0.12,hair_hi,"HairLock")
        lock.scale=Vector3(1.05,1.55,0.55)

    # Expressive eyes and independently controllable pupils.
    for side in [-1.0,1.0]:
        var eye:=sphere_part(root,Vector3(0.115*side,2.28,-0.315),0.060,eye_white,"Eye")
        var pupil:=sphere_part(root,Vector3(0.115*side,2.28,-0.365),0.028,iris,"Pupil")
        sphere_part(root,Vector3(0.115*side,2.282,-0.389),0.010,eye_white,"EyeSpark")
        box(root,Vector3(0.115*side,2.36,-0.320),Vector3(0.13,0.022,0.025),hair,"Brow")
        player_eye_nodes.append(eye)
        player_pupil_nodes.append(pupil)
        sphere_part(root,Vector3(0.31*side,2.23,0),0.075,skin_shadow,"Ear")

    # Nose bridge/tip and real mouth construction.
    var nose_bridge:=cylinder(root,Vector3(0,2.22,-0.345),0.040,0.14,skin_shadow,"NoseBridge")
    nose_bridge.rotation_degrees.x=90.0
    sphere_part(root,Vector3(0,2.17,-0.405),0.055,skin,"NoseTip")
    player_jaw=sphere_part(root,Vector3(0,-0.018,2.065),0.205,skin,"Jaw")
    player_jaw.scale=Vector3(1.15,0.58,0.55)
    player_mouth=box(root,Vector3(0,-0.351,2.075),Vector3(0.105,0.018,0.038),mouth_dark,"Mouth")
    box(root,Vector3(0,-0.366,2.088),Vector3(0.074,0.012,0.012),lip,"UpperLip")
    box(root,Vector3(0,-0.366,2.056),Vector3(0.066,0.012,0.010),lip,"LowerLip")

    # Layered shoulders, arms, gloves, pants and boots.
    for side in [-1.0,1.0]:
        var s:=-1 if side<0 else 1
        sphere_part(root,Vector3(0.51*side,1.74,0),0.20,armor,"Shoulder")
        capsule(root,Vector3(0.55*side,1.45,0),0.11,0.58,leather,"UpperArm")
        capsule(root,Vector3(0.57*side,1.02,0),0.095,0.46,armor,"Forearm")
        sphere_part(root,Vector3(0.57*side,0.73,0),0.11,skin,"Hand")
        capsule(root,Vector3(0.21*side,0.63,0),0.16,0.82,pants,"Thigh")
        capsule(root,Vector3(0.21*side,0.18,0),0.13,0.58,boot_mat,"Shin")
        box(root,Vector3(0.21*side,-0.12,-0.12),Vector3(0.20,0.34,0.14),boot_mat,"Boot")

    # Cape panels and gold trim give the silhouette from the supplied reference.
    var cape_l:=sphere_part(root,Vector3(-0.31,1.27,0.10),0.65,green,"Cape")
    cape_l.scale=Vector3(0.62,1.20,0.18)
    var cape_r:=sphere_part(root,Vector3(0.31,1.27,0.10),0.65,green,"Cape")
    cape_r.scale=Vector3(0.62,1.20,0.18)
    box(root,Vector3(-0.47,-0.20,1.35),Vector3(0.025,0.018,0.56),green_trim,"CapeTrim")
    box(root,Vector3(0.47,-0.20,1.35),Vector3(0.025,0.018,0.56),green_trim,"CapeTrim")
    box(root,Vector3(0,-0.37,1.08),Vector3(0.42,0.018,0.035),green_trim,"BeltTrim")

func capsule(parent:Node3D,pos:Vector3,radius:float,height:float,mat:Material,node_name:String)->MeshInstance3D:
    var n:=MeshInstance3D.new()
    var m:=CapsuleMesh.new()
    m.radius=radius
    m.height=height
    m.radial_segments=12
    m.rings=4
    n.mesh=m
    n.name=node_name
    n.position=pos
    n.material_override=mat
    parent.add_child(n)
    return n

func sphere_part(parent:Node3D,pos:Vector3,radius:float,mat:Material,node_name:String)->MeshInstance3D:
    var n:=MeshInstance3D.new()
    var m:=SphereMesh.new()
    m.radius=radius
    m.height=radius*2.0
    n.mesh=m
    n.position=pos
    n.material_override=mat
    parent.add_child(n)
    return n

func _spawn_population():
    for i in range(20):
        var n := Node3D.new()
        n.name="Civilian_%02d"%i
        n.position=Vector3(rng.randf_range(-86,86),0,rng.randf_range(-86,86))
        _create_npc_visual(n,i)
        add_child(n)
        n.set_meta("phase",rng.randf_range(0,TAU))
        n.set_meta("speed",rng.randf_range(0.35,0.85))
        npcs.append(n)

    for i in range(20):
        var c := Node3D.new()
        c.name="Traffic_%02d"%i
        c.position=Vector3([-82,-40,42,82][i%4],0.45,rng.randf_range(-90,90))
        _create_car(c,i)
        c.set_meta("speed",rng.randf_range(4.0,8.0))
        c.set_meta("dir",1.0 if i%2==0 else -1.0)
        cars.append(c)
        add_child(c)

func _create_npc_visual(root:Node3D,index:int):
    var skins=[Color("#9b604a"),Color("#d08a67"),Color("#704738"),Color("#c99a76"),Color("#7f523f"),Color("#d9a27c")]
    var outfits=[Color("#7d3348"),Color("#355f94"),Color("#3d765c"),Color("#754f98"),Color("#8f622f"),Color("#327985")]
    var hairs=[Color("#1a1210"),Color("#4a2116"),Color("#a0441f"),Color("#252a32"),Color("#6b351d"),Color("#3a273e")]
    var skin:=material(skins[index%skins.size()],0.62)
    var cloth:=material(outfits[index%outfits.size()],0.58,0.04)
    var hair:=material(hairs[index%hairs.size()],0.42)
    var eye_white:=material(Color("#f2f5f8"),0.19)
    var iris:=material([Color("#4f9a64"),Color("#527bad"),Color("#8d6b38")][index%3],0.15,0.04)
    var lip:=material(Color("#73323d"),0.45)
    capsule(root,Vector3(0,1.05,0),0.27,0.78,cloth,"Body").scale=Vector3(1.05,1.0,0.76)
    capsule(root,Vector3(0,0.60,0),0.18,0.50,cloth,"Hip")
    cylinder(root,Vector3(0,1.50,0),0.105,0.12,skin,"Neck")
    var head:=sphere_part(root,Vector3(0,1.67,0),0.245,skin,"Head")
    head.scale=Vector3(0.98,1.05,0.92)
    sphere_part(root,Vector3(0,1.86,0.015),0.255,hair,"Hair")
    for side in [-1.0,1.0]:
        sphere_part(root,Vector3(0.18*side,1.70,-0.22),0.048,eye_white,"Eye")
        sphere_part(root,Vector3(0.18*side,1.70,-0.263),0.024,iris,"Pupil")
        box(root,Vector3(0.18*side,1.775,-0.222),Vector3(0.09,0.016,0.018),hair,"Brow")
        sphere_part(root,Vector3(0.245*side,1.67,0),0.060,skin,"Ear")
        capsule(root,Vector3(0.31*side,1.09,0),0.085,0.56,cloth,"Arm")
        sphere_part(root,Vector3(0.31*side,0.78,0),0.085,skin,"Hand")
        capsule(root,Vector3(0.16*side,0.52,0),0.125,0.64,cloth,"Leg")
        sphere_part(root,Vector3(0.16*side,0.15,-0.08),0.13,cloth,"Foot")
    var npc_nose:=cylinder(root,Vector3(0,1.67,-0.275),0.038,0.10,skin,"Nose")
    npc_nose.rotation_degrees.x=90.0
    var mouth:=box(root,Vector3(0,-0.247,1.58),Vector3(0.075,0.013,0.023),lip,"Mouth")
    var jaw:=sphere_part(root,Vector3(0,-0.02,1.59),0.15,skin,"Jaw")
    jaw.scale=Vector3(1.12,0.60,0.55)
    root.set_meta("head_node",head)
    root.set_meta("jaw_node",jaw)
    root.set_meta("mouth_node",mouth)
    root.set_meta("eye_l",root.get_node_or_null("Eye"))
    root.set_meta("eye_r",root.get_node_or_null("Eye2"))
    root.set_meta("pupil_l",root.get_node_or_null("Pupil"))
    root.set_meta("pupil_r",root.get_node_or_null("Pupil2"))

func _create_car(root:Node3D,index:int):
    var colors=[Color("#d83d55"),Color("#3274d8"),Color("#d6a33d"),Color("#40ad7b"),Color("#9b5ed0"),Color("#e1e5e8")]
    var paint:=material(colors[index%colors.size()],0.32,0.45)
    var glass:=material(Color("#0b1623"),0.12,0.75)
    var tire:=material(Color("#090a0d"),0.92)
    var style:=index%4
    var body_w:float=2.0 if style!=1 else 2.15
    var body_h:=0.62 if style!=3 else 0.82
    box(root,Vector3(0,0,0),Vector3(body_w,body_h,4.1+float(style)*0.25),paint,"CarBody")
    box(root,Vector3(0,0.45,-0.15),Vector3(1.45,0.48,1.85 if style<3 else 1.55),glass,"Cabin")
    if style==1: box(root,Vector3(0,0.78,0.65),Vector3(1.9,0.18,1.45),paint,"SUVRoof")
    if style==2: box(root,Vector3(0,0.68,1.0),Vector3(1.8,0.14,1.30),paint,"SportDeck")
    if style==3: box(root,Vector3(0,0.90,1.20),Vector3(2.0,0.22,1.45),paint,"TruckBed")
    box(root,Vector3(0,0.47,-1.05),Vector3(1.48,0.32,0.08),paint,"Hood")
    box(root,Vector3(0,0.46,1.15),Vector3(1.48,0.25,0.08),paint,"Trunk")
    var wheels:Array[Node3D]=[]
    for x in [-0.92,0.92]:
        for z in [-1.25,1.25]:
            var wheel:=MeshInstance3D.new()
            var wm:=CylinderMesh.new();wm.top_radius=0.38;wm.bottom_radius=0.38;wm.height=0.18;wm.radial_segments=12
            wheel.mesh=wm;wheel.rotation_degrees=Vector3(90,0,0);wheel.position=Vector3(x, -0.05,z);wheel.material_override=tire;root.add_child(wheel)
            wheels.append(wheel)
    car_wheel_sets.append(wheels)
    box(root,Vector3(-0.58,0.20, -2.05),Vector3(0.32,0.14,0.06),material(Color("#ffe7a1"),0.25,0.1,Color("#ffe7a1")),"Headlight")
    box(root,Vector3(0.58,0.20,-2.05),Vector3(0.32,0.14,0.06),material(Color("#ffe7a1"),0.25,0.1,Color("#ffe7a1")),"Headlight")
    cylinder(root,Vector3(-0.42,0.18,2.15),0.055,0.18,material(Color("#11151b"),0.4,0.5),"Exhaust")
    cylinder(root,Vector3(0.42,0.18,2.15),0.055,0.18,material(Color("#11151b"),0.4,0.5),"Exhaust")
    _add_exhaust_smoke(root,Vector3(-0.42,0.18,2.28))
    _add_exhaust_smoke(root,Vector3(0.42,0.18,2.28))

func _add_exhaust_smoke(root:Node3D,pos:Vector3):
    var smoke:=CPUParticles3D.new()
    smoke.name="ExhaustSmoke"; smoke.position=pos; smoke.emitting=true
    smoke.amount=16; smoke.lifetime=1.25; smoke.speed_scale=1.0
    smoke.direction=Vector3(0,0,1); smoke.spread=20.0
    smoke.initial_velocity_min=0.4; smoke.initial_velocity_max=1.0
    smoke.scale_amount_min=0.08; smoke.scale_amount_max=0.20
    smoke.color=Color(0.55,0.58,0.62,0.28); root.add_child(smoke)

func _setup_audio():
    engine_audio=AudioStreamPlayer.new(); engine_audio.stream=_make_tone_stream(96.0,0.35,1.6); engine_audio.volume_db=-26.0; engine_audio.autoplay=true; add_child(engine_audio)
    ambience_audio=AudioStreamPlayer.new(); ambience_audio.stream=_make_tone_stream(180.0,0.10,3.0); ambience_audio.volume_db=-32.0; ambience_audio.autoplay=true; add_child(ambience_audio)

func _make_tone_stream(freq:float,amp:float,seconds:float)->AudioStreamWAV:
    var rate:=22050; var count:=int(rate*seconds); var bytes:=PackedByteArray(); bytes.resize(count*2)
    for i in range(count):
        var t:float=float(i)/rate; var fade:float=min(1.0,t*20.0)*min(1.0,(seconds-t)*12.0)
        bytes.encode_s16(i*2,int(sin(TAU*freq*t)*amp*fade*32767.0))
    var wav:=AudioStreamWAV.new(); wav.format=AudioStreamWAV.FORMAT_16_BITS; wav.mix_rate=rate; wav.stereo=false; wav.data=bytes
    wav.loop_mode=AudioStreamWAV.LOOP_FORWARD; wav.loop_begin=0; wav.loop_end=count; return wav

func _setup_ui():
    ui_layer=CanvasLayer.new()
    ui_layer.name="PremiumHUD"
    add_child(ui_layer)

    var profile:=_ui_panel(Vector2(16,14),Vector2(286,92),Color(0.035,0.055,0.075,0.90),Color(0.20,0.72,0.86,0.38),14)
    _ui_label(profile,"STREET SOVEREIGN",Vector2(16,9),Vector2(200,24),18,Color("#f2fbff"))
    district_value=_ui_label(profile,"NIGHT CITY",Vector2(16,34),Vector2(180,18),12,Color("#83d9ee"))
    cash_value=_ui_label(profile,"$ 12,500",Vector2(190,11),Vector2(82,22),15,Color("#f6d26b"),HORIZONTAL_ALIGNMENT_RIGHT)
    wanted_value=_ui_label(profile,"W 0/5",Vector2(200,38),Vector2(72,18),12,Color("#ff8a72"),HORIZONTAL_ALIGNMENT_RIGHT)

    health_bar=ProgressBar.new()
    health_bar.position=Vector2(16,58)
    health_bar.size=Vector2(160,10)
    health_bar.show_percentage=false
    health_bar.value=100.0
    var hb_bg:=StyleBoxFlat.new(); hb_bg.bg_color=Color(0.06,0.09,0.12,1); hb_bg.corner_radius_top_left=5; hb_bg.corner_radius_top_right=5; hb_bg.corner_radius_bottom_left=5; hb_bg.corner_radius_bottom_right=5
    var hb_fill:=StyleBoxFlat.new(); hb_fill.bg_color=Color("#49d6a7"); hb_fill.corner_radius_top_left=5; hb_fill.corner_radius_top_right=5; hb_fill.corner_radius_bottom_left=5; hb_fill.corner_radius_bottom_right=5
    health_bar.add_theme_stylebox_override("background",hb_bg); health_bar.add_theme_stylebox_override("fill",hb_fill); profile.add_child(health_bar)

    stamina_bar=ProgressBar.new()
    stamina_bar.position=Vector2(16,73)
    stamina_bar.size=Vector2(160,7)
    stamina_bar.show_percentage=false
    stamina_bar.value=85.0
    var sb_bg:=StyleBoxFlat.new(); sb_bg.bg_color=Color(0.06,0.09,0.12,1); sb_bg.corner_radius_top_left=4; sb_bg.corner_radius_top_right=4; sb_bg.corner_radius_bottom_left=4; sb_bg.corner_radius_bottom_right=4
    var sb_fill:=StyleBoxFlat.new(); sb_fill.bg_color=Color("#4b9dff"); sb_fill.corner_radius_top_left=4; sb_fill.corner_radius_top_right=4; sb_fill.corner_radius_bottom_left=4; sb_fill.corner_radius_bottom_right=4
    stamina_bar.add_theme_stylebox_override("background",sb_bg); stamina_bar.add_theme_stylebox_override("fill",sb_fill); profile.add_child(stamina_bar)

    var mission_panel:=_ui_panel(Vector2(16,114),Vector2(286,78),Color(0.025,0.040,0.055,0.86),Color(0.95,0.72,0.26,0.32),12)
    _ui_label(mission_panel,"CURRENT OBJECTIVE",Vector2(14,9),Vector2(240,16),10,Color("#8da6b7"))
    mission_value=_ui_label(mission_panel,"SUNSET RUN",Vector2(14,27),Vector2(248,22),15,Color("#ffe18a"))
    objective=_ui_label(mission_panel,"Reach the gold marker",Vector2(14,50),Vector2(250,18),11,Color("#d9e5ed"))

    var compass:=_ui_panel(Vector2(0,12),Vector2(260,38),Color(0.02,0.03,0.04,0.68),Color(0.40,0.82,0.96,0.22),18)
    compass.set_anchors_preset(Control.PRESET_TOP_WIDE,Control.PRESET_MODE_MINSIZE)
    compass.position.x=0
    _ui_label(compass,"W       NW       N       NE       E",Vector2(18,6),Vector2(224,26),11,Color("#d9eef8"),HORIZONTAL_ALIGNMENT_CENTER)

    map_panel=_ui_panel(Vector2(-132,14),Vector2(112,112),Color(0.02,0.035,0.045,0.82),Color(0.48,0.86,0.95,0.36),56)
    map_panel.set_anchors_preset(Control.PRESET_TOP_RIGHT,Control.PRESET_MODE_MINSIZE)
    map_panel.position.x=-132
    _ui_label(map_panel,"N",Vector2(46,8),Vector2(20,18),11,Color("#f2fbff"),HORIZONTAL_ALIGNMENT_CENTER)
    _ui_label(map_panel,"+",Vector2(46,46),Vector2(20,20),15,Color("#8ee8ff"),HORIZONTAL_ALIGNMENT_CENTER)
    _ui_label(map_panel,"MISSION",Vector2(23,82),Vector2(66,16),9,Color("#ffe18a"),HORIZONTAL_ALIGNMENT_CENTER)

    graphics_button=_ui_button(ui_layer,"SET",Vector2(-204,14),Vector2(60,48),12)
    graphics_button.set_anchors_preset(Control.PRESET_TOP_RIGHT,Control.PRESET_MODE_MINSIZE)
    graphics_button.position.x=-204
    graphics_button.pressed.connect(_toggle_settings)

    var fire:=_ui_button(ui_layer,"FIRE",Vector2(-126,-146),Vector2(108,108),17)
    fire.set_anchors_preset(Control.PRESET_BOTTOM_RIGHT,Control.PRESET_MODE_MINSIZE)
    fire.position.x=-126; fire.position.y=-146
    fire.button_down.connect(func(): shooting=true)
    fire.button_up.connect(func(): shooting=false)

    var jump:=_ui_button(ui_layer,"JUMP",Vector2(-220,-92),Vector2(76,76),13)
    jump.set_anchors_preset(Control.PRESET_BOTTOM_RIGHT,Control.PRESET_MODE_MINSIZE)
    jump.position.x=-220; jump.position.y=-92
    jump.pressed.connect(func(): jump_requested=true)

    drive_button=_ui_button(ui_layer,"ENTER CAR",Vector2(-250,-174),Vector2(110,58),11)
    drive_button.set_anchors_preset(Control.PRESET_BOTTOM_RIGHT,Control.PRESET_MODE_MINSIZE)
    drive_button.position.x=-250; drive_button.position.y=-174
    drive_button.z_index=30
    drive_button.pressed.connect(_toggle_drive)

    joystick_ring=_ui_panel(Vector2(24,-198),Vector2(176,176),Color(0.02,0.035,0.045,0.30),Color(0.58,0.82,0.90,0.30),88)
    joystick_ring.set_anchors_preset(Control.PRESET_BOTTOM_LEFT,Control.PRESET_MODE_MINSIZE)
    joystick_ring.position.x=24; joystick_ring.position.y=-198
    joystick_knob=Panel.new()
    joystick_knob.position=Vector2(58,58)
    joystick_knob.size=Vector2(60,60)
    joystick_knob.add_theme_stylebox_override("panel",_ui_style(Color(0.68,0.86,0.93,0.18),Color(0.82,0.95,1,0.45),30))
    joystick_ring.add_child(joystick_knob)
    _ui_label(ui_layer,"MOVE",Vector2(24,-220),Vector2(176,18),9,Color("#b7cad5"),HORIZONTAL_ALIGNMENT_CENTER)
    var touch_hint:=_ui_label(ui_layer,"DRAG RIGHT = LOOK",Vector2(-210,-30),Vector2(190,18),9,Color(0.72,0.84,0.90,0.60),HORIZONTAL_ALIGNMENT_RIGHT)
    touch_hint.set_anchors_preset(Control.PRESET_BOTTOM_RIGHT,Control.PRESET_MODE_MINSIZE)
    touch_hint.position.x=-210; touch_hint.position.y=-30

    boot_label=Label.new()
    boot_label.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    boot_label.horizontal_alignment=HORIZONTAL_ALIGNMENT_CENTER
    boot_label.vertical_alignment=VERTICAL_ALIGNMENT_CENTER
    boot_label.add_theme_font_size_override("font_size",18)
    boot_label.add_theme_color_override("font_color",Color("#8ee8ff"))
    boot_label.text="STARTING 3D WORLD"
    ui_layer.add_child(boot_label)

    settings_panel=_ui_panel(Vector2(0,0),Vector2(330,350),Color(0.025,0.045,0.065,0.97),Color(0.46,0.82,0.94,0.48),20)
    settings_panel.set_anchors_preset(Control.PRESET_CENTER,Control.PRESET_MODE_MINSIZE)
    settings_panel.position=Vector2(-165,-175)
    _ui_label(settings_panel,"GRAPHICS & CONTROLS",Vector2(22,20),Vector2(285,28),18,Color("#f3fbff"))
    _ui_label(settings_panel,"Performance presets for mobile",Vector2(22,50),Vector2(285,18),10,Color("#8da6b7"))

    var low:=_ui_button(settings_panel,"LOW",Vector2(22,88),Vector2(86,48),12); low.pressed.connect(func(): graphics=0; _apply_quality())
    var med:=_ui_button(settings_panel,"MED",Vector2(122,88),Vector2(86,48),12); med.pressed.connect(func(): graphics=1; _apply_quality())
    var high:=_ui_button(settings_panel,"HIGH",Vector2(222,88),Vector2(86,48),12); high.pressed.connect(func(): graphics=2; _apply_quality())

    gyro_button=_ui_button(settings_panel,"GYRO: ON",Vector2(22,150),Vector2(130,48),11)
    gyro_button.pressed.connect(func():
        gyro_enabled=!gyro_enabled
        gyro_button.text="GYRO: ON" if gyro_enabled else "GYRO: OFF")
    var close:=_ui_button(settings_panel,"CLOSE",Vector2(178,150),Vector2(130,48),11)
    close.pressed.connect(_toggle_settings)
    _ui_label(settings_panel,"CAMERA SENSITIVITY",Vector2(22,224),Vector2(200,18),10,Color("#8da6b7"))
    _ui_label(settings_panel,"0.12",Vector2(245,224),Vector2(63,18),10,Color("#dff7ff"),HORIZONTAL_ALIGNMENT_RIGHT)
    _ui_label(settings_panel,"Real-time external GLB assets + PBR roads",Vector2(22,274),Vector2(286,36),10,Color("#72b9cb"))

    hud=Label.new()
    hud.visible=false
    ui_layer.add_child(hud)
    status=Label.new()
    status.visible=false
    ui_layer.add_child(status)
    crosshair=Label.new()
    crosshair.text="+"
    crosshair.set_anchors_and_offsets_preset(Control.PRESET_CENTER)
    crosshair.add_theme_font_size_override("font_size",20)
    crosshair.add_theme_color_override("font_color",Color(1,1,1,0.55))
    ui_layer.add_child(crosshair)
    fps_label=Label.new()
    fps_label.position=Vector2(16,198)
    fps_label.add_theme_font_size_override("font_size",10)
    fps_label.add_theme_color_override("font_color",Color("#89a2b0"))
    ui_layer.add_child(fps_label)
    settings_panel.visible=false

func _ui_style(bg:Color,border:Color,radius:int)->StyleBoxFlat:
    var sb:=StyleBoxFlat.new()
    sb.bg_color=bg
    sb.border_color=border
    sb.set_border_width_all(1)
    sb.corner_radius_top_left=radius
    sb.corner_radius_top_right=radius
    sb.corner_radius_bottom_left=radius
    sb.corner_radius_bottom_right=radius
    return sb

func _ui_panel(pos:Vector2,size:Vector2,bg:Color,border:Color,radius:int)->Panel:
    var p:=Panel.new()
    p.position=pos
    p.size=size
    p.add_theme_stylebox_override("panel",_ui_style(bg,border,radius))
    ui_layer.add_child(p)
    return p

func _ui_label(parent:Node,text_value:String,pos:Vector2,size:Vector2,font_size:int,font_color:Color,align:int=HORIZONTAL_ALIGNMENT_LEFT)->Label:
    var l:=Label.new()
    l.text=text_value
    l.position=pos
    l.size=size
    l.horizontal_alignment=align
    l.vertical_alignment=VERTICAL_ALIGNMENT_CENTER
    l.add_theme_font_size_override("font_size",font_size)
    l.add_theme_color_override("font_color",font_color)
    parent.add_child(l)
    return l

func _ui_button(parent:Node,text_value:String,pos:Vector2,size:Vector2,font_size:int)->Button:
    var b:=Button.new()
    b.text=text_value
    b.position=pos
    b.size=size
    b.z_index=25
    b.mouse_filter=Control.MOUSE_FILTER_STOP
    b.add_theme_font_size_override("font_size",font_size)
    b.add_theme_color_override("font_color",Color("#edf9ff"))
    b.add_theme_color_override("font_hover_color",Color("#ffffff"))
    b.add_theme_color_override("font_pressed_color",Color("#ffffff"))
    b.add_theme_stylebox_override("normal",_ui_style(Color(0.05,0.09,0.12,0.92),Color(0.39,0.70,0.82,0.55),18))
    b.add_theme_stylebox_override("pressed",_ui_style(Color(0.10,0.22,0.28,0.98),Color(0.55,0.90,1.0,0.85),18))
    b.add_theme_stylebox_override("hover",_ui_style(Color(0.08,0.15,0.19,0.96),Color(0.50,0.82,0.92,0.65),18))
    parent.add_child(b)
    return b


func _toggle_settings():
    if is_instance_valid(settings_panel):
        settings_panel.visible=not settings_panel.visible

func _new_mission():
    if is_instance_valid(mission_marker): mission_marker.queue_free()
    mission_marker=MeshInstance3D.new()
    var ring:=TorusMesh.new()
    ring.inner_radius=1.6;ring.outer_radius=2.05;ring.rings=16;ring.ring_segments=24
    mission_marker.mesh=ring
    mission_marker.material_override=material(Color("#ffd83d"),0.18,0.55,Color("#ffd83d"))
    mission_marker.position=missions[mission][1]
    add_child(mission_marker)

func _input(event):
    if event is InputEventScreenTouch:
        if event.pressed:
            if event.position.x < get_viewport().size.x*0.42 and joystick_id==-1:
                joystick_id=event.index; joystick_center=event.position; move_input=Vector2.ZERO
            elif event.position.x >= get_viewport().size.x*0.42 and look_id==-1:
                look_id=event.index
        else:
            if event.index==joystick_id: joystick_id=-1; move_input=Vector2.ZERO
            if event.index==look_id: look_id=-1
    elif event is InputEventScreenDrag:
        if event.index==joystick_id:
            var offset: Vector2 = event.position-joystick_center
            if offset.length()>joystick_radius: offset=offset.normalized()*joystick_radius
            move_input=offset/joystick_radius
        elif event.index==look_id:
            camera_yaw-=event.relative.x*sensitivity
            camera_pitch=clamp(camera_pitch-event.relative.y*sensitivity,-55.0,35.0)

func _process(delta):
    elapsed+=delta
    fire_cooldown=max(0.0,fire_cooldown-delta)
    _animate_player(delta)
    _update_day_night()
    _update_hud()
    if is_instance_valid(mission_marker):
        mission_marker.rotation.y+=delta*1.6
        mission_marker.position.y=0.55+sin(elapsed*2.5)*0.15
        if player.global_position.distance_to(mission_marker.global_position)<5.0:
            money+=1000+mission*450
            wanted=max(0,wanted-1)
            mission=(mission+1)%missions.size()
            _new_mission()

func _physics_process(delta):
    _move_player(delta)
    _animate_npcs(delta)
    _animate_cars(delta)

func _move_player(delta):
    var input_vec:=move_input
    var keyboard:=Input.get_vector("ui_left","ui_right","ui_up","ui_down")
    if keyboard.length()>0.1: input_vec=keyboard
    var yaw:=deg_to_rad(camera_yaw)
    var forward:=Vector3(-sin(yaw),0,-cos(yaw))
    var right:=Vector3(cos(yaw),0,-sin(yaw))
    var dir:Vector3=(right*input_vec.x+forward*(-input_vec.y))
    if dir.length()>1: dir=dir.normalized()

    if driving and is_instance_valid(active_car):
        var car_speed:float=10.5
        var steer:=input_vec.x
        if input_vec.length()>0.05:
            active_car.rotation.y=lerp_angle(active_car.rotation.y,active_car.rotation.y-steer*delta*2.6,delta*5.0)
            var cf:=active_car.transform.basis.z.normalized()
            var motion:Vector3=(-cf)*(-input_vec.y)*car_speed*delta
            var collision:KinematicCollision3D=active_car.move_and_collide(motion)
            if collision==null:
                active_car.position.x=clamp(active_car.position.x,-93.0,93.0)
                active_car.position.z=clamp(active_car.position.z,-93.0,93.0)
        player.global_position=active_car.global_position+Vector3(0,0.05,0)
        player.velocity=Vector3.ZERO
    else:
        var speed:=5.8
        player.velocity.x=move_toward(player.velocity.x,dir.x*speed,delta*22.0)
        player.velocity.z=move_toward(player.velocity.z,dir.z*speed,delta*22.0)
        player.velocity.y=0.0
        player.move_and_slide()
        if jump_requested and player.is_on_floor():
            player.velocity.y=7.0
            jump_requested=false

    player.position.x=clamp(player.position.x,-93.0,93.0)
    player.position.z=clamp(player.position.z,-93.0,93.0)

    if dir.length()>0.1 and not driving:
        player_visual.rotation.y=lerp_angle(player_visual.rotation.y,atan2(dir.x,dir.z),delta*10.0)

    var cam_yaw_rad:=deg_to_rad(camera_yaw)
    var cam_pitch_rad:=deg_to_rad(camera_pitch)
    var rot:=Basis(Vector3.UP,cam_yaw_rad)*Basis(Vector3.RIGHT,cam_pitch_rad)
    var cam_target:=active_car.global_position+Vector3(0,1.3,0) if driving and is_instance_valid(active_car) else player.global_position+Vector3(0,1.15,0)
    var desired_cam:=cam_target+rot*Vector3(0,0,camera_distance)
    camera.global_position=camera.global_position.lerp(desired_cam,1.0-exp(-delta*14.0))
    camera.look_at(cam_target,Vector3.UP)
    if gyro_enabled and not look_touching:
        var acc:=Input.get_accelerometer()
        if acc.length()>0.3:
            camera_yaw-=clamp(acc.x,-4.0,4.0)*delta*1.8
            camera_pitch=clamp(camera_pitch+clamp(acc.y,-3.0,3.0)*delta*0.9,-35.0,18.0)
    if shooting and not driving and fire_cooldown<=0:
        _fire()

func _nearest_car()->Node3D:
    var best:Node3D=null
    var dist:=999.0
    for c in cars:
        var d:=player.global_position.distance_to(c.global_position)
        if d<dist: dist=d;best=c
    return best

func _fire():
    fire_cooldown=0.22
    shooting=false
    wanted=min(5,wanted+1)
    for n in npcs:
        if n.global_position.distance_to(player.global_position)<7.0:
            n.set_meta("speed",min(2.0,float(n.get_meta("speed"))+0.8))

func _animate_player(delta):
    if player_model_animation:
        var speed_now:=Vector2(player.velocity.x,player.velocity.z).length()
        var clip:=player_idle_anim
        if speed_now>7.5 and player_run_anim!="":
            clip=player_run_anim
        elif speed_now>0.35 and player_walk_anim!="":
            clip=player_walk_anim
        if clip!="" and player_model_animation.current_animation!=clip:
            player_model_animation.play(clip)
        player_model_animation.speed_scale=clamp(speed_now/3.2,0.72,1.08) if speed_now>0.35 else 1.0
        return
    if not is_instance_valid(player_visual):
        return
    var speed_fallback:=Vector2(player.velocity.x,player.velocity.z).length()
    var moving_fallback:=speed_fallback>0.35
    player_anim_phase+=delta*(7.0+speed_fallback*1.5 if moving_fallback else 1.6)
    var stride:float=sin(player_anim_phase)
    var stride2:float=sin(player_anim_phase+PI)
    player_visual.position.y=abs(sin(player_anim_phase*0.5))*(0.040 if moving_fallback else 0.010)
    if player_head:
        var heading:=player_visual.rotation.y
        var body_to_camera:=wrapf(deg_to_rad(camera_yaw)-heading,-PI,PI)
        player_head.rotation.y=clamp(body_to_camera*0.34,-0.50,0.50)+sin(elapsed*0.42)*0.13
        player_head.rotation.x=clamp(-deg_to_rad(camera_pitch)*0.18,-0.16,0.16)
    if player_arms.size()>=2:
        player_arms[0].rotation.x=stride*0.62 if moving_fallback else 0.0
        player_arms[1].rotation.x=stride2*0.62 if moving_fallback else 0.0
    if player_legs.size()>=2:
        player_legs[0].rotation.x=stride2*0.68 if moving_fallback else 0.0
        player_legs[1].rotation.x=stride*0.68 if moving_fallback else 0.0

func _animate_npcs(delta):
    for n in npcs:
        var body:=n as CharacterBody3D
        if body==null: continue

        var target:Vector3=n.get_meta("target",n.global_position)
        var to_target:Vector3=target-n.global_position
        to_target.y=0.0
        var speed:float=float(n.get_meta("speed"))
        var life:float=float(n.get_meta("life_clock",0.0))+delta
        n.set_meta("life_clock",life)

        if to_target.length()<1.0:
            n.set_meta("target",_next_sidewalk_point(n.position))
            target=Vector3(n.get_meta("target"))
            to_target=target-n.position
            to_target.y=0.0

        var moving:bool=to_target.length()>0.8
        var dir:Vector3=to_target.normalized() if moving else Vector3.ZERO

        # Two-layer crowd avoidance:
        # 1) steer away before movement;
        # 2) resolve any remaining overlap after movement.
        var separation:=Vector3.ZERO
        var crowd_blocked:=false
        for other in npcs:
            if other==n:
                continue
            var offset:Vector3=n.global_position-other.global_position
            offset.y=0.0
            var distance:float=offset.length()
            if distance>0.01 and distance<0.95:
                separation += offset.normalized()*((0.95-distance)/0.95)
            if moving and distance>0.01 and distance<0.72:
                var toward_other:=(-offset).normalized()
                if dir.dot(toward_other)>0.15:
                    crowd_blocked=true
        if separation.length()>0.01:
            dir=(dir+separation.normalized()*1.35).normalized()
        if crowd_blocked and separation.length()<0.1:
            body.velocity=Vector3.ZERO
        else:
            body.velocity=dir*speed if moving else Vector3.ZERO
        body.velocity.y=0.0
        var hit:KinematicCollision3D=body.move_and_collide(body.velocity*delta)
        if hit:
            body.velocity=Vector3.ZERO
            body.set_meta("target",_next_sidewalk_point(body.position))

        # Hard minimum spacing stops two civilians from ever visually crossing
        # or occupying one another's capsule, even between physics frames.
        for other in npcs:
            if other==n:
                continue
            var post_offset:Vector3=body.global_position-other.global_position
            post_offset.y=0.0
            var post_distance:float=post_offset.length()
            var min_spacing:float=0.72
            if post_distance>0.01 and post_distance<min_spacing:
                var push:Vector3=post_offset.normalized()*((min_spacing-post_distance)*0.65)
                body.global_position += push
                body.velocity=Vector3.ZERO

        # CharacterBody3D collision now blocks buildings, cars, the player,
        # and other pedestrians. On contact, choose another connected point.
        if body.is_on_wall():
            body.velocity=Vector3.ZERO
            body.set_meta("target",_next_sidewalk_point(body.position))
            var next_target:Vector3=body.get_meta("target")
            var next_dir:Vector3=next_target-body.position
            next_dir.y=0.0
            if next_dir.length()>0.1:
                body.rotation.y=lerp_angle(body.rotation.y,atan2(next_dir.x,next_dir.z),delta*5.5)
        elif moving:
            body.rotation.y=lerp_angle(body.rotation.y,atan2(dir.x,dir.z),delta*5.5)

        var ap:AnimationPlayer=n.get_meta("anim_player",null) as AnimationPlayer
        if ap:
            var idle_name:String=String(n.get_meta("idle_anim",""))
            var walk_name:String=String(n.get_meta("walk_anim",""))
            var chosen:String=walk_name if moving else idle_name
            if chosen!="" and ap.current_animation!=chosen:
                if ap.current_animation!=chosen:
                    ap.play(chosen)
                    # Rig animation is intentionally disabled; use subtle root motion
            # instead so the walk never stretches or detaches the mesh.
            ap.stop()
            ap.active=false

        var npc_model:Node3D=n.get_meta("model",null) as Node3D
        if npc_model:
            var base_scale:Vector3=n.get_meta("base_model_scale",Vector3.ONE)
            npc_model.scale=base_scale
            if moving:
                npc_model.position.y=0.025+sin(life*9.0)*0.018
                npc_model.rotation.z=sin(life*4.5)*0.018
            else:
                npc_model.position.y=0.025
                npc_model.rotation.z=0.0

        n.position.y=0.0

func _find_npc_spawn_point()->Vector3:
    # Prevent two civilians from spawning on top of each other or inside the
    # same tiny patch of sidewalk.
    var preferred:=PLAYER_START + Vector3(rng.randf_range(-34.0,34.0),0.0,rng.randf_range(-34.0,34.0))
    for attempt in range(24):
        var candidate:=_nearest_safe_sidewalk_point(preferred + Vector3(rng.randf_range(-10.0,10.0),0.0,rng.randf_range(-10.0,10.0)))
        var clear:=true
        for other in npcs:
            if is_instance_valid(other) and candidate.distance_to(other.global_position)<1.05:
                clear=false
                break
        if clear:
            return candidate
        preferred+=Vector3(rng.randf_range(-14.0,14.0),0.0,rng.randf_range(-14.0,14.0))
    return _nearest_safe_sidewalk_point(preferred)

func _nearest_npc(source:Node3D)->Node3D:
    var best:Node3D=null
    var best_d:=999.0
    for other in npcs:
        if other==source: continue
        var d:=source.global_position.distance_to(other.global_position)
        if d<best_d:
            best_d=d
            best=other
    return best

func _animate_cars(delta):
    for c in cars:
        if bool(c.get_meta("player_car",false)):
            continue
        var speed:float=float(c.get_meta("speed",5.0))
        var dir:float=float(c.get_meta("dir",1.0))
        c.position.z+=speed*dir*delta
        if c.position.z>96: c.position.z=-96
        if c.position.z<-96: c.position.z=96
        c.rotation.y=0.0 if dir>0.0 else PI
        c.position.y=0.45
        var throttle:float=clamp(speed/14.0,0.0,1.0)
        var smoke_nodes:Array[Node]=c.find_children("ExhaustSmoke","CPUParticles3D",true,false)
        for smoke_node in smoke_nodes:
            var smoke:=smoke_node as CPUParticles3D
            if smoke:
                smoke.amount=10+int(throttle*32.0)
                smoke.speed_scale=0.7+throttle*1.6

func _update_day_night():
    var hour:=fmod(18.0+elapsed*0.12,24.0)
    sun.rotation_degrees=Vector3(-38.0-(hour-12.0)*2.0,-28,0)
    sun.light_energy=0.42 if hour>20.0 or hour<6.0 else 1.2

func _cycle_graphics():
    graphics=(graphics+1)%3
    _apply_quality()

func _apply_quality():
    if not is_instance_valid(sun): return
    var names=["LOW","MED","HIGH"]
    graphics_button.text="SET" if is_instance_valid(graphics_button) else ""
    sun.shadow_enabled=graphics>1
    sun.directional_shadow_max_distance=[45.0,75.0,105.0][graphics]
    world_env.environment.ambient_light_energy=[0.68,0.82,0.95][graphics]
    world_env.environment.glow_enabled=false

func _update_hud():
    if is_instance_valid(health_bar):
        health_bar.value=health
    if is_instance_valid(stamina_bar):
        stamina_bar.value=clamp(70.0+sin(elapsed*1.4)*12.0,0.0,100.0)
    if is_instance_valid(cash_value):
        cash_value.text="$ %d"%money
    if is_instance_valid(wanted_value):
        wanted_value.text="W %d/5"%wanted
    if is_instance_valid(district_value):
        district_value.text=districts[mission%3]
    if is_instance_valid(mission_value):
        mission_value.text=missions[mission][0]
    if is_instance_valid(objective):
        objective.text="Reach the gold marker"
    if is_instance_valid(fps_label):
        var library_count:int=kenney_total_paths.size()
        fps_label.text="FPS %d  |  GFX %s  |  3D LIB %d"%[Engine.get_frames_per_second(),["LOW","MED","HIGH"][graphics],library_count]
    if is_instance_valid(joystick_knob):
        var knob_center:=Vector2(58,58)
        joystick_knob.position=knob_center+move_input*48.0

func _toggle_drive():
    if driving:
        driving=false
        player.collision_layer=1
        player.collision_mask=7
        if is_instance_valid(active_car):
            player.global_position=active_car.global_position+Vector3(2.2,0.0,0.0)
        player_visual.visible=true
        active_car=null
        if is_instance_valid(drive_button): drive_button.text="ENTER CAR"
        return
    var nearest:=_nearest_car()
    if nearest and player.global_position.distance_to(nearest.global_position)<6.5:
        driving=true
        player.collision_layer=0
        player.collision_mask=0
        active_car=nearest
        player.global_position=nearest.global_position+Vector3(0,0.05,0)
        player_visual.visible=false
        if is_instance_valid(drive_button): drive_button.text="EXIT CAR"
    else:
        if is_instance_valid(drive_button): drive_button.text="NEAR CAR"
