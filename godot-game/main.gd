extends Node3D
## STREET SOVEREIGN 3D — mobile-first visual rebuild.
## Original procedural assets; optimized for Android using reusable meshes and batched scenery.

const WORLD := 190.0
const PLAYER_START := Vector3(-58, 0.1, 54)

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
var car_wheel_sets: Array[Array] = []

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
    await get_tree().process_frame
    boot_label.text = "LOADING • CITY 10%"
    await _build_city_async()
    boot_label.text = "LOADING • PEOPLE & TRAFFIC"
    await _spawn_population_async()
    _new_mission()
    _apply_quality()
    boot_label.text = "STREET SOVEREIGN • READY"
    await get_tree().create_timer(0.7).timeout
    if is_instance_valid(boot_label):
        boot_label.queue_free()

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
    var total:=120
    var done:=0
    for ds in range(4):
        for i in range(30):
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
    for i in range(44):
        var n := Node3D.new()
        n.name="Civilian_%02d"%i
        n.position=Vector3(rng.randf_range(-86,86),0,rng.randf_range(-86,86))
        _create_npc_visual(n,i)
        add_child(n)
        n.set_meta("phase",rng.randf_range(0,TAU))
        n.set_meta("speed",rng.randf_range(0.35,0.85))
        npcs.append(n)
        if i%4==0:
            await get_tree().process_frame
    for i in range(20):
        var c := Node3D.new()
        c.name="Traffic_%02d"%i
        c.position=Vector3([-82,-40,42,82][i%4],0.45,rng.randf_range(-90,90))
        _create_car(c,i)
        c.set_meta("speed",rng.randf_range(4.0,8.0))
        c.set_meta("dir",1.0 if i%2==0 else -1.0)
        cars.append(c)
        add_child(c)
        if i%4==0:
            await get_tree().process_frame

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
    var env := Environment.new()
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
    var root := Node3D.new()
    root.position = Vector3(x,0,z)
    city.add_child(root)
    var facade: Color = palette[i%2]
    box(root,Vector3(0,h*0.5,0),Vector3(w,h,d),material(facade,0.65),"Building")
    # Vertical side tower / architectural crown.
    if i%3==0:
        box(root,Vector3(w*0.28,h+1.5,0),Vector3(w*0.22,3.0,d*0.45),material(palette[1],0.55,0.15),"Crown")
    # Windows in two bands, recognizable as a building rather than a plain cube.
    var window_mat := material(Color("#101b2b"),0.22,0.45,palette[2])
    for band in range(min(5,int(h/4.5))):
        var yy := 2.0 + band*4.0
        box(root,Vector3(-w*0.18,yy,d*0.515),Vector3(w*0.23,1.0,0.08),window_mat,"Window")
        box(root,Vector3(w*0.18,yy,d*0.515),Vector3(w*0.23,1.0,0.08),window_mat,"Window")
    if i%4==0:
        box(root,Vector3(0,h*0.72,d*0.515),Vector3(w*0.78,0.22,0.08),material(palette[2],0.2,0.2,palette[2]),"NeonSign")
    if i%5==0:
        cylinder(root,Vector3(0,h+2,0),0.65,4.0,material(Color("#1c202a"),0.45,0.3),"RoofTank")

func _build_roads():
    var road_mat := material(Color("#10141b"),0.96)
    var lane_mat := material(Color("#f3d26b"),0.5,0.05)
    for z in range(-84,85,21):
        box(city,Vector3(0,-0.02,z),Vector3(WORLD,0.18,6.5),road_mat,"Road")
        for x in range(-88,89,12):
            box(city,Vector3(x,0.10,z),Vector3(4.2,0.035,0.13),lane_mat,"Lane")
    for x in [-82.0,-40.0,42.0,82.0]:
        box(city,Vector3(x,0.0,0),Vector3(6.5,0.18,WORLD),road_mat,"Road")
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

func _build_instanced_props():
    # Deliberately sparse decorative props around roads; avoids thousands of active nodes.
    for i in range(90):
        var x := rng.randf_range(-88,88)
        var z := rng.randf_range(-88,88)
        if abs(x) < 10: continue
        var root := Node3D.new()
        root.position = Vector3(x,0,z)
        city.add_child(root)
        if i%3==0:
            cylinder(root,Vector3(0,2.1,0),0.09,4.2,material(Color("#20252e"),0.55,0.25),"Lamp")
            box(root,Vector3(0,4.15,0),Vector3(0.65,0.12,0.65),material(Color("#ffd37a"),0.25,0.1,Color("#ffd37a")),"LampGlow")
        elif i%3==1:
            cylinder(root,Vector3(0,1.0,0),0.75,2.0,material(Color("#1f6944"),0.85),"Tree")
            cylinder(root,Vector3(0,0.65,0),0.22,1.3,material(Color("#63442d"),0.9),"Trunk")
        else:
            box(root,Vector3(0,0.5,0),Vector3(1.2,1.0,0.8),material(Color("#6c4c36"),0.85),"Crate")

func _spawn_player():
    player = CharacterBody3D.new()
    player.name = "Player"
    player.position = PLAYER_START
    add_child(player)
    var cs := CollisionShape3D.new()
    var capsule := CapsuleShape3D.new()
    capsule.radius = 0.42
    capsule.height = 1.65
    cs.shape = capsule
    cs.position.y = 0.95
    player.add_child(cs)

    player_visual = Node3D.new()
    player_visual.name = "HeroVisual"
    player.add_child(player_visual)
    _create_humanoid(player_visual)

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
    var skin := material(Color("#b97859"),0.72)
    var jacket := material(Color("#263a5b"),0.68,0.1)
    var shirt := material(Color("#e8edf2"),0.62)
    var pants := material(Color("#18202e"),0.78)
    var shoes := material(Color("#0a0c10"),0.55,0.15)
    # Torso, head, neck.
    player_body=box(root,Vector3(0,1.25,0),Vector3(0.72,0.92,0.42),jacket,"Torso")
    cylinder(root,Vector3(0,1.83,0),0.23,0.18,skin,"Neck")
    var head := MeshInstance3D.new()
    var sphere := SphereMesh.new()
    sphere.radius=0.31;sphere.height=0.62
    head.mesh=sphere;head.position=Vector3(0,2.2,0);head.material_override=skin;root.add_child(head)
    player_head=head
    # Hair/hat silhouette.
    cylinder(root,Vector3(0,2.49,0),0.34,0.13,material(Color("#111522"),0.7),"Hair")
    # Arms and hands.
    for side in [-1.0,1.0]:
        var arm:=box(root,Vector3(0.48*side,1.30,0),Vector3(0.18,0.72,0.22),jacket,"Arm")
        player_arms.append(arm)
        cylinder(root,Vector3(0.48*side,0.88,0),0.12,0.20,skin,"Hand")
        var leg:=box(root,Vector3(0.19*side,0.62,0),Vector3(0.30,0.85,0.30),pants,"Leg")
        player_legs.append(leg)
        box(root,Vector3(0.19*side,0.15,0.08),Vector3(0.34,0.18,0.56),shoes,"Shoe")
    box(root,Vector3(0,1.42,-0.225),Vector3(0.36,0.45,0.05),shirt,"Shirt")

func _spawn_population():
    for i in range(44):
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
    var skin_colors=[Color("#9b604a"),Color("#d08a67"),Color("#704738"),Color("#c99a76")]
    var clothes=[Color("#d44c5d"),Color("#3f73bd"),Color("#4aa878"),Color("#b46ad2")]
    var skin:=material(skin_colors[index%4],0.8)
    var cloth:=material(clothes[index%4],0.72)
    box(root,Vector3(0,1.05,0),Vector3(0.5,0.72,0.30),cloth,"Body")
    var head:=MeshInstance3D.new()
    var sm:=SphereMesh.new();sm.radius=0.22;sm.height=0.44
    head.mesh=sm;head.position=Vector3(0,1.62,0);head.material_override=skin;root.add_child(head)
    for side in [-1.0,1.0]:
        box(root,Vector3(0.16*side,0.50,0),Vector3(0.18,0.65,0.20),cloth,"Leg"+("2" if side>0 else ""))
        box(root,Vector3(0.34*side,1.08,0),Vector3(0.14,0.62,0.18),cloth,"Arm"+("2" if side>0 else ""))

func _create_car(root:Node3D,index:int):
    var colors=[Color("#d83d55"),Color("#3274d8"),Color("#d6a33d"),Color("#40ad7b"),Color("#9b5ed0"),Color("#e1e5e8")]
    var paint:=material(colors[index%colors.size()],0.32,0.45)
    var glass:=material(Color("#0b1623"),0.12,0.75)
    var tire:=material(Color("#090a0d"),0.92)
    box(root,Vector3(0,0,0),Vector3(2.0,0.62,4.1),paint,"CarBody")
    box(root,Vector3(0,0.45,-0.15),Vector3(1.45,0.48,1.85),glass,"Cabin")
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

func _setup_ui():
    var layer:=CanvasLayer.new()
    add_child(layer)

    hud=Label.new()
    hud.position=Vector2(22,18)
    hud.add_theme_font_size_override("font_size",18)
    hud.add_theme_color_override("font_color",Color.WHITE)
    layer.add_child(hud)

    objective=Label.new()
    objective.position=Vector2(22,70)
    objective.add_theme_font_size_override("font_size",15)
    objective.add_theme_color_override("font_color",Color("#ffd75a"))
    layer.add_child(objective)

    status=Label.new()
    status.position=Vector2(22,112)
    status.add_theme_font_size_override("font_size",13)
    status.add_theme_color_override("font_color",Color("#a9c6e8"))
    layer.add_child(status)

    crosshair=Label.new()
    crosshair.text="+"
    crosshair.position=Vector2(0,0)
    crosshair.set_anchors_and_offsets_preset(Control.PRESET_CENTER)
    crosshair.add_theme_font_size_override("font_size",28)
    crosshair.add_theme_color_override("font_color",Color("#ffffffcc"))
    layer.add_child(crosshair)

    boot_label=Label.new()
    boot_label.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    boot_label.horizontal_alignment=HORIZONTAL_ALIGNMENT_CENTER
    boot_label.vertical_alignment=VERTICAL_ALIGNMENT_CENTER
    boot_label.add_theme_font_size_override("font_size",22)
    boot_label.add_theme_color_override("font_color",Color("#63e6ff"))
    boot_label.text="BOOT 0/5  •  STARTING 3D WORLD"
    layer.add_child(boot_label)

    var fire:=_button(layer,"FIRE",Vector2(-170,-150),Vector2(150,90),24)
    fire.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_RIGHT,Control.PRESET_MODE_MINSIZE,18)
    fire.button_down.connect(func():shooting=true)
    fire.button_up.connect(func():shooting=false)

    var drive:=_button(layer,"ENTER CAR",Vector2(-335,-150),Vector2(145,70),16)
    drive.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_RIGHT,Control.PRESET_MODE_MINSIZE,18)
    drive.position.y-=100
    drive.pressed.connect(_toggle_drive)

    graphics_button=_button(layer,"GRAPHICS: MED",Vector2(0,0),Vector2(145,48),13)
    graphics_button.set_anchors_and_offsets_preset(Control.PRESET_TOP_RIGHT,Control.PRESET_MODE_MINSIZE,18)
    graphics_button.pressed.connect(_cycle_graphics)

    gyro_button=_button(layer,"GYRO: ON",Vector2(0,0),Vector2(125,48),13)
    gyro_button.set_anchors_and_offsets_preset(Control.PRESET_TOP_RIGHT,Control.PRESET_MODE_MINSIZE,18)
    gyro_button.position.x-=160
    gyro_button.pressed.connect(func():
        gyro_enabled=!gyro_enabled
        gyro_button.text="GYRO: ON" if gyro_enabled else "GYRO: OFF")

    # Virtual joystick: left bottom.
    var up:=_button(layer,"▲",Vector2(112,-180),Vector2(74,66),24)
    up.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_LEFT,Control.PRESET_MODE_MINSIZE,18)
    up.button_down.connect(func():move_input.y=-1)
    up.button_up.connect(func():move_input.y=0)
    var left:=_button(layer,"◀",Vector2(28,-112),Vector2(74,66),24)
    left.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_LEFT,Control.PRESET_MODE_MINSIZE,18)
    left.button_down.connect(func():move_input.x=-1)
    left.button_up.connect(func():move_input.x=0)
    var right:=_button(layer,"▶",Vector2(196,-112),Vector2(74,66),24)
    right.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_LEFT,Control.PRESET_MODE_MINSIZE,18)
    right.button_down.connect(func():move_input.x=1)
    right.button_up.connect(func():move_input.x=0)
    var down:=_button(layer,"▼",Vector2(112,-42),Vector2(74,50),20)
    down.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_LEFT,Control.PRESET_MODE_MINSIZE,18)
    down.button_down.connect(func():move_input.y=1)
    down.button_up.connect(func():move_input.y=0)

    var hint:=Label.new()
    hint.text="DRAG RIGHT SIDE = CAMERA • GYRO = CAMERA"
    hint.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_WIDE)
    hint.position.y=-28
    hint.horizontal_alignment=HORIZONTAL_ALIGNMENT_CENTER
    hint.add_theme_font_size_override("font_size",12)
    hint.add_theme_color_override("font_color",Color("#d7e5ff99"))
    layer.add_child(hint)

func _button(layer:CanvasLayer,text_value:String,pos:Vector2,size:Vector2,font_size:int)->Button:
    var b:=Button.new()
    b.text=text_value
    b.position=pos
    b.size=size
    b.add_theme_font_size_override("font_size",font_size)
    layer.add_child(b)
    return b

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
            last_touch=event.position
            look_touching=event.position.x>get_viewport().size.x*0.42
        else:
            look_touching=false
    elif event is InputEventScreenDrag and look_touching:
        var delta:Vector2=event.position-last_touch
        camera_yaw-=delta.x*0.18
        camera_pitch=clamp(camera_pitch-delta.y*0.12,-35.0,18.0)
        last_touch=event.position

func _process(delta):
    elapsed+=delta
    fire_cooldown=max(0.0,fire_cooldown-delta)
    _move_player(delta)
    _animate_player(delta)
    _animate_npcs(delta)
    _animate_cars(delta)
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

func _move_player(delta):
    var input_vec:=move_input
    var keyboard:=Input.get_vector("ui_left","ui_right","ui_up","ui_down")
    if keyboard.length()>0.1: input_vec=keyboard
    var speed:=11.0 if driving else 5.8
    var yaw:=deg_to_rad(camera_yaw)
    var forward:=Vector3(-sin(yaw),0,-cos(yaw))
    var right:=Vector3(cos(yaw),0,-sin(yaw))
    var dir:Vector3=(right*input_vec.x+forward*(-input_vec.y))
    if dir.length()>1: dir=dir.normalized()
    player.velocity.x=move_toward(player.velocity.x,dir.x*speed,delta*22.0)
    player.velocity.z=move_toward(player.velocity.z,dir.z*speed,delta*22.0)
    player.velocity.y=0
    player.move_and_slide()
    player.position.x=clamp(player.position.x,-93.0,93.0)
    player.position.z=clamp(player.position.z,-93.0,93.0)
    if dir.length()>0.1:
        player_visual.rotation.y=lerp_angle(player_visual.rotation.y,atan2(dir.x,dir.z),delta*10.0)
    # Smooth camera follows pivot; no per-frame look_at snapping.
    camera_pivot.rotation_degrees.y=camera_yaw
    camera_pivot.rotation_degrees.x=camera_pitch
    var cam_rot := Basis(Vector3.UP,deg_to_rad(camera_yaw))
    var cam_offset := cam_rot * Vector3(0,2.7,6.3)
    var desired_cam := player.global_position + cam_offset
    camera.global_position = camera.global_position.lerp(desired_cam,1.0-exp(-delta*12.0))
    camera.look_at(player.global_position + Vector3(0,1.25,0),Vector3.UP)
    if gyro_enabled and not look_touching:
        var acc:=Input.get_accelerometer()
        if acc.length()>0.3:
            camera_yaw-=clamp(acc.x,-4.0,4.0)*delta*1.8
            camera_pitch=clamp(camera_pitch+clamp(acc.y,-3.0,3.0)*delta*0.9,-35.0,18.0)
    if shooting and not driving and fire_cooldown<=0:
        _fire()

func _toggle_drive():
    var nearest:=_nearest_car()
    if nearest and player.global_position.distance_to(nearest.global_position)<4.5:
        driving=!driving
        if driving:
            player.position=nearest.position+Vector3(0,0.05,0)
            player_visual.visible=false
        else:
            player_visual.visible=true

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
    if not is_instance_valid(player_visual):
        return
    var moving:=Vector2(player.velocity.x,player.velocity.z).length()>0.35
    var speed_now:=Vector2(player.velocity.x,player.velocity.z).length()
    player_anim_phase+=delta*(7.0+speed_now*1.5 if moving else 2.2)
    var stride: float = sin(player_anim_phase)
    var stride2: float = sin(player_anim_phase+PI)
    var bob: float = abs(sin(player_anim_phase*0.5))*(0.045 if moving else 0.018)
    player_visual.position.y=lerp(player_visual.position.y,bob,1.0-exp(-delta*12.0))
    if player_body:
        player_body.rotation.z=lerp(player_body.rotation.z,(-0.035*stride if moving else 0.0),1.0-exp(-delta*10.0))
        player_body.scale.y=1.0+sin(player_anim_phase)*0.018
    if player_head:
        player_head.rotation.z=sin(player_anim_phase*0.5)*0.035
        player_head.rotation.x=sin(player_anim_phase*0.7)*0.025
    if player_arms.size()>=2:
        player_arms[0].rotation.x=lerp(player_arms[0].rotation.x,stride*0.65 if moving else sin(elapsed*1.8)*0.04,1.0-exp(-delta*14.0))
        player_arms[1].rotation.x=lerp(player_arms[1].rotation.x,stride2*0.65 if moving else -sin(elapsed*1.8)*0.04,1.0-exp(-delta*14.0))
    if player_legs.size()>=2:
        player_legs[0].rotation.x=lerp(player_legs[0].rotation.x,stride2*0.72 if moving else 0.0,1.0-exp(-delta*16.0))
        player_legs[1].rotation.x=lerp(player_legs[1].rotation.x,stride*0.72 if moving else 0.0,1.0-exp(-delta*16.0))
    if shooting:
        player_body.rotation.x=lerp(player_body.rotation.x,-0.10,1.0-exp(-delta*22.0))
    else:
        player_body.rotation.x=lerp(player_body.rotation.x,0.0,1.0-exp(-delta*10.0))

func _animate_npcs(delta):
    for n in npcs:
        var phase:=float(n.get_meta("phase"))+elapsed*float(n.get_meta("speed"))
        n.position.x+=sin(phase*0.7)*delta*0.22
        n.position.z+=cos(phase*0.53)*delta*0.22
        n.position.x=clamp(n.position.x,-90.0,90.0)
        n.position.z=clamp(n.position.z,-90.0,90.0)
        n.rotation.y=sin(phase)*0.35
        var walk:=sin(phase*1.8)
        var arm_l:=n.get_node_or_null("Arm")
        var leg_l:=n.get_node_or_null("Leg")
        if arm_l: arm_l.rotation.x=walk*0.45
        if leg_l: leg_l.rotation.x=-walk*0.55
        var arm_r:=n.get_node_or_null("Arm2")
        var leg_r:=n.get_node_or_null("Leg2")
        if arm_r: arm_r.rotation.x=-walk*0.45
        if leg_r: leg_r.rotation.x=walk*0.55
        n.position.y=abs(sin(phase*0.9))*0.035

func _animate_cars(delta):
    for c in cars:
        var speed:=float(c.get_meta("speed"))
        var dir:=float(c.get_meta("dir"))
        c.position.z+=speed*dir*delta
        if c.position.z>96:c.position.z=-96
        if c.position.z<-96:c.position.z=96
        c.rotation.y=0 if dir>0 else PI
        c.position.y=0.45+sin(elapsed*7.0+float(c.get_instance_id()%11))*0.035

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
    graphics_button.text="GRAPHICS: "+names[graphics] if is_instance_valid(graphics_button) else ""
    sun.shadow_enabled=graphics>1
    sun.directional_shadow_max_distance=[45.0,75.0,105.0][graphics]
    world_env.environment.ambient_light_energy=[0.68,0.82,0.95][graphics]
    world_env.environment.glow_enabled=false

func _update_hud():
    if not is_instance_valid(hud): return
    hud.text="STREET SOVEREIGN 3D\\n$%d   HP %d   WANTED %d/5   •   %s" % [money,int(health),wanted,districts[mission%3]]
    objective.text="MISSION %d/6  —  %s\\nReach the gold marker" % [mission+1,missions[mission][0]]
    status.text="44 CIVILIANS • 20 TRAFFIC CARS • TOUCH CAMERA • GYRO "+("ON" if gyro_enabled else "OFF");
