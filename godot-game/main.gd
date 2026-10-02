extends Node3D

# STREET SOVEREIGN 3D — original open-world Android game.
# Procedural content is used to create a large 3D city without copying GTA assets.

var player: CharacterBody3D
var camera: Camera3D
var sun: DirectionalLight3D
var world_env: WorldEnvironment
var health := 100.0
var money := 12500
var wanted := 0
var mission := 0
var district := 0
var driving := false
var shooting := false
var graphics := 2
var elapsed := 0.0
var rng := RandomNumberGenerator.new()
var cars: Array[Node3D] = []
var npcs: Array[Node3D] = []
var mission_marker: MeshInstance3D
var hud: Label
var objective: Label
var status: Label
var fire_audio: AudioStreamPlayer
var engine_audio: AudioStreamPlayer
var touch_dir := Vector2.ZERO
var camera_yaw := 0.0
var camera_pitch := -0.25

var mission_names = [
    "SUNSET RUN",
    "NEON PACKAGE",
    "RIVER DISTRICT",
    "HIGHWAY HEAT",
    "NIGHT WOLVES",
    "CITY TAKEOVER"
]
var districts = ["VICE RAY", "SAN VALORA", "LIBERTY BAY"]

func _ready():
    rng.seed = 24017
    _setup_world()
    _build_city()
    _spawn_player()
    _spawn_population()
    _setup_ui()
    _setup_audio()
    _new_mission()
    _update_quality()
    set_process(true)

func mat(color: Color, rough := 0.7, metallic := 0.0) -> StandardMaterial3D:
    var m = StandardMaterial3D.new()
    m.albedo_color = color
    m.roughness = rough
    m.metallic = metallic
    return m

func mesh_box(parent: Node3D, pos: Vector3, size: Vector3, material: Material, name := "prop") -> MeshInstance3D:
    var n = MeshInstance3D.new()
    n.name = name
    var b = BoxMesh.new()
    b.size = size
    n.mesh = b
    n.position = pos
    n.material_override = material
    parent.add_child(n)
    return n

func mesh_cyl(parent: Node3D, pos: Vector3, radius: float, height: float, material: Material, name := "cylinder") -> MeshInstance3D:
    var n = MeshInstance3D.new()
    n.name = name
    var c = CylinderMesh.new()
    c.top_radius = radius
    c.bottom_radius = radius
    c.height = height
    n.mesh = c
    n.position = pos
    n.material_override = material
    parent.add_child(n)
    return n

func _setup_world():
    world_env = WorldEnvironment.new()
    var env = Environment.new()
    env.background_mode = Environment.BG_SKY
    var sky = Sky.new()
    var sky_mat = ProceduralSkyMaterial.new()
    sky_mat.sky_top_color = Color("#091329")
    sky_mat.sky_horizon_color = Color("#f16b55")
    sky_mat.ground_bottom_color = Color("#05070c")
    sky_mat.ground_horizon_color = Color("#2b3344")
    sky.sky_material = sky_mat
    env.sky = sky
    env.ambient_light_source = Environment.AMBIENT_SOURCE_SKY
    env.ambient_light_energy = 0.75
    env.tonemap_mode = Environment.TONE_MAPPER_FILMIC
    world_env.environment = env
    add_child(world_env)

    sun = DirectionalLight3D.new()
    sun.rotation_degrees = Vector3(-48, -32, 0)
    sun.light_energy = 1.15
    sun.shadow_enabled = true
    sun.directional_shadow_max_distance = 90.0
    add_child(sun)

    var moon = OmniLight3D.new()
    moon.position = Vector3(0, 35, 0)
    moon.omni_range = 150
    moon.light_energy = 0.25
    moon.light_color = Color("#7899ff")
    add_child(moon)

    var floor = StaticBody3D.new()
    floor.name = "CityGround"
    add_child(floor)
    mesh_box(floor, Vector3(0, -1.2, 0), Vector3(210, 2, 210), mat(Color("#243329")), "ground")
    var shape = CollisionShape3D.new()
    var box = BoxShape3D.new()
    box.size = Vector3(210, 2, 210)
    shape.shape = box
    shape.position.y = -1.2
    floor.add_child(shape)

func _build_city():
    var city = Node3D.new()
    city.name = "ThreeDistrictMegaCity"
    add_child(city)

    # Three original districts with distinct visual identities.
    for sector in range(3):
        var x0 = -70.0 + sector * 70.0
        var district_mat = [mat(Color("#5a596b")), mat(Color("#465b62")), mat(Color("#5b4e58"))][sector]
        for i in range(185):
            var bx = x0 + rng.randf_range(-31, 31)
            var bz = rng.randf_range(-82, 82)
            var h = rng.randf_range(7, 34) * (1.0 if sector != 1 else 1.25)
            var w = rng.randf_range(5, 12)
            var d = rng.randf_range(5, 12)
            var building = mesh_box(city, Vector3(bx, h/2.0, bz), Vector3(w,h,d), district_mat, "Building_%03d" % i)
            # Rooftop equipment makes the skyline denser.
            if i % 3 == 0:
                mesh_cyl(city, Vector3(bx, h + 2.0, bz), 0.7, 4.0, mat(Color("#30343d"),0.5,0.3), "Rooftop")
            if i % 7 == 0:
                mesh_box(city, Vector3(bx, h + 4.0, bz), Vector3(4,0.25,0.7), mat(Color("#ff4058"),0.3,0.1), "NeonSign")
        _build_roads(city, x0)
        _build_landmarks(city, x0, sector)

    # River and bridges separating the districts.
    mesh_box(city, Vector3(0, -0.25, 0), Vector3(14, 0.4, 205), mat(Color("#123c64"),0.2,0.4), "GrandRiver")
    for z in [-62.0, 0.0, 62.0]:
        mesh_box(city, Vector3(0, 0.7, z), Vector3(22, 1.5, 12), mat(Color("#5e626a"),0.5,0.15), "Bridge")
        for k in range(-5,6):
            mesh_cyl(city, Vector3(k*2.0, 2.2, z-4.0), 0.12, 3.0, mat(Color("#d6b45c"),0.5,0.3), "BridgeLamp")

    # Thousands of small visual assets: lamps, signs, barriers, trees and crates.
    for i in range(1300):
        var px = rng.randf_range(-98,98)
        var pz = rng.randf_range(-98,98)
        if abs(px) < 9: continue
        var kind = i % 5
        if kind == 0:
            mesh_cyl(city, Vector3(px, 2.0, pz), 0.08, 4.0, mat(Color("#30333a"),0.6,0.2), "StreetLamp")
        elif kind == 1:
            mesh_box(city, Vector3(px, 0.45, pz), Vector3(0.7,0.9,0.7), mat(Color("#7b4e35")), "Crate")
        elif kind == 2:
            mesh_cyl(city, Vector3(px, 1.4, pz), 0.9, 2.8, mat(Color("#1d5b3b")), "Tree")
        elif kind == 3:
            mesh_box(city, Vector3(px, 0.9, pz), Vector3(1.6,1.8,0.35), mat(Color("#4e5665")), "Barrier")
        else:
            mesh_box(city, Vector3(px, 2.0, pz), Vector3(0.25,4,0.25), mat(Color("#20232a"),0.5,0.2), "Pole")

func _build_roads(city: Node3D, center_x: float):
    for z in range(-90, 91, 18):
        mesh_box(city, Vector3(center_x, 0, z), Vector3(62,0.25,7), mat(Color("#15181e"),0.95), "Road")
        for lane in range(-7,8,2):
            mesh_box(city, Vector3(center_x+lane*2.0, 0.16, z), Vector3(1.2,0.04,0.25), mat(Color("#d9b94e"),0.6), "RoadMark")
    for x in range(int(center_x)-30,int(center_x)+31,15):
        mesh_box(city, Vector3(x,0,-82), Vector3(6,0.25,180), mat(Color("#15181e"),0.95), "CrossRoad")

func _build_landmarks(city: Node3D, x0: float, sector: int):
    var colors=[Color("#ef4f6b"),Color("#41b9e8"),Color("#f2b84b")]
    mesh_box(city,Vector3(x0,3,-72),Vector3(20,6,5),mat(Color("#252a35"),0.5,0.2),"Landmark")
    mesh_box(city,Vector3(x0,6.2,-72),Vector3(14,1,0.5),mat(colors[sector],0.2,0.4),"LandmarkNeon")
    for a in range(8):
        mesh_cyl(city,Vector3(x0-8+a*2.3,1.8,-74.8),0.12,3.6,mat(Color("#242832"),0.4,0.3),"LandmarkLamp")

func _spawn_player():
    player = CharacterBody3D.new()
    player.name = "HamzaPlayer"
    add_child(player)
    player.position = Vector3(-62, 2, 58)
    var collision = CollisionShape3D.new()
    var capsule = CapsuleShape3D.new()
    capsule.radius = 0.55
    capsule.height = 1.9
    collision.shape = capsule
    player.add_child(collision)
    var body = MeshInstance3D.new()
    var capsule_mesh = CapsuleMesh.new()
    capsule_mesh.radius = 0.55
    capsule_mesh.height = 1.9
    body.mesh = capsule_mesh
    body.material_override = mat(Color("#e8e8ee"),0.65,0.05)
    player.add_child(body)

    camera = Camera3D.new()
    camera.position = Vector3(0, 3.8, 6.8)
    camera.rotation_degrees.x = -14
    player.add_child(camera)
    camera.current = true

func _spawn_population():
    for i in range(110):
        var n = CharacterBody3D.new()
        n.name = "NPC_%03d" % i
        n.position = Vector3(rng.randf_range(-92,92),1.1,rng.randf_range(-92,92))
        var cs=CollisionShape3D.new()
        var sh=CapsuleShape3D.new()
        sh.radius=0.3;sh.height=1.5;cs.shape=sh;n.add_child(cs)
        var mesh=MeshInstance3D.new()
        var cm=CapsuleMesh.new()
        cm.radius=0.3;cm.height=1.5;mesh.mesh=cm
        mesh.material_override=mat([Color("#d99a78"),Color("#79a4d8"),Color("#c96a7a"),Color("#8bcf9b")][i%4])
        n.add_child(mesh)
        n.set_meta("phase",rng.randf_range(0,6.28))
        n.set_meta("speed",rng.randf_range(0.3,1.0))
        n.set_meta("cop",i%17==0)
        npcs.append(n);add_child(n)

    for i in range(65):
        var c=Node3D.new()
        c.name="Vehicle_%03d"%i
        c.position=Vector3(rng.randf_range(-92,92),0.9,rng.randf_range(-92,92))
        c.set_meta("speed",rng.randf_range(2.5,7.0))
        c.set_meta("dir",1 if i%2==0 else -1)
        var body=mesh_box(c,Vector3.ZERO,Vector3(2.1,0.65,4.3),mat([Color("#d94655"),Color("#3974d7"),Color("#d8a63e"),Color("#42aa77"),Color("#9a62ce"),Color("#e1e1e1")][i%6],0.35,0.35),"CarBody")
        mesh_box(c,Vector3(0,0.45,0),Vector3(1.4,0.35,2.0),mat(Color("#141820"),0.1,0.6),"CarGlass")
        cars.append(c);add_child(c)

func _setup_audio():
    fire_audio=AudioStreamPlayer.new()
    var fire=AudioStreamGenerator.new()
    fire.mix_rate=22050;fire.buffer_length=0.12
    fire_audio.stream=fire;add_child(fire_audio);fire_audio.play()
    engine_audio=AudioStreamPlayer.new()
    var engine=AudioStreamGenerator.new()
    engine.mix_rate=22050;engine.buffer_length=0.3
    engine_audio.stream=engine;add_child(engine_audio);engine_audio.play()

func _tone(player_audio: AudioStreamPlayer, frequency: float, duration: float):
    var playback=player_audio.get_stream_playback()
    if playback==null:return
    var frames=int(22050*duration)
    var data=PackedVector2Array()
    data.resize(frames)
    for i in range(frames):
        var v=sin(TAU*frequency*float(i)/22050.0)*0.16
        data[i]=Vector2(v,v)
    playback.push_buffer(data)

func _setup_ui():
    var layer=CanvasLayer.new()
    add_child(layer)
    hud=Label.new()
    hud.position=Vector2(26,22);hud.add_theme_font_size_override("font_size",25);hud.add_theme_color_override("font_color",Color.WHITE)
    layer.add_child(hud)
    objective=Label.new()
    objective.position=Vector2(26,72);objective.add_theme_font_size_override("font_size",18);objective.add_theme_color_override("font_color",Color("#ffd85c"))
    layer.add_child(objective)
    status=Label.new()
    status.position=Vector2(26,108);status.add_theme_font_size_override("font_size",17)
    layer.add_child(status)

    var fire=Button.new();fire.text="FIRE";fire.position=Vector2(1090,560);fire.size=Vector2(145,100);fire.add_theme_font_size_override("font_size",24);layer.add_child(fire)
    fire.button_down.connect(func():shooting=true)
    fire.button_up.connect(func():shooting=false)

    var drive=Button.new();drive.text="ENTER / EXIT";drive.position=Vector2(920,560);drive.size=Vector2(155,100);drive.add_theme_font_size_override("font_size",17);layer.add_child(drive)
    drive.pressed.connect(func():driving=!driving)

    var graphics_btn=Button.new();graphics_btn.text="GRAPHICS";graphics_btn.position=Vector2(1030,25);graphics_btn.size=Vector2(180,58);graphics_btn.add_theme_font_size_override("font_size",16);layer.add_child(graphics_btn)
    graphics_btn.pressed.connect(func():graphics=(graphics+1)%3;_update_quality())

    var map=Button.new();map.text="MAP";map.position=Vector2(840,25);map.size=Vector2(165,58);map.add_theme_font_size_override("font_size",16);layer.add_child(map)
    map.pressed.connect(func():_show_map())

    var left=Button.new();left.text="◀";left.position=Vector2(30,575);left.size=Vector2(85,90);left.add_theme_font_size_override("font_size",30);layer.add_child(left)
    left.button_down.connect(func():touch_dir.x=-1);left.button_up.connect(func():touch_dir.x=0)
    var right=Button.new();right.text="▶";right.position=Vector2(220,575);right.size=Vector2(85,90);right.add_theme_font_size_override("font_size",30);layer.add_child(right)
    right.button_down.connect(func():touch_dir.x=1);right.button_up.connect(func():touch_dir.x=0)
    var up=Button.new();up.text="▲";up.position=Vector2(125,500);up.size=Vector2(85,90);up.add_theme_font_size_override("font_size",30);layer.add_child(up)
    up.button_down.connect(func():touch_dir.y=-1);up.button_up.connect(func():touch_dir.y=0)
    var down=Button.new();down.text="▼";down.position=Vector2(125,665);down.size=Vector2(85,55);down.add_theme_font_size_override("font_size",24);layer.add_child(down)
    down.button_down.connect(func():touch_dir.y=1);down.button_up.connect(func():touch_dir.y=0)

func _new_mission():
    if mission_marker: mission_marker.queue_free()
    mission_marker=MeshInstance3D.new()
    var ring=TorusMesh.new()
    ring.inner_radius=1.8;ring.outer_radius=2.2;ring.rings=24;ring.ring_segments=32
    mission_marker.mesh=ring
    mission_marker.material_override=mat(Color("#ffd83d"),0.2,0.6)
    mission_marker.position=[Vector3(-62,0.5,58),Vector3(52,0.5,44),Vector3(58,0.5,-42),Vector3(-52,0.5,-55),Vector3(70,0.5,70),Vector3(0,0.5,-75)][mission]
    add_child(mission_marker)

func _process(delta):
    elapsed+=delta
    _move_player(delta)
    _animate_population(delta)
    _animate_cars(delta)
    _update_time(delta)
    _update_hud()
    if mission_marker and player and player.global_position.distance_to(mission_marker.global_position)<5.0:
        money += 1000 + mission*450
        wanted=max(0,wanted-1)
        mission=(mission+1)%mission_names.size()
        district=mission%3
        _new_mission()

func _move_player(delta):
    var input_vec=Input.get_vector("left","right","forward","back")
    if touch_dir.length()>0.1: input_vec=touch_dir
    var speed=12.0 if driving else 7.0
    if input_vec.length()>1:input_vec=input_vec.normalized()
    var dir=Vector3(input_vec.x,0,input_vec.y)
    player.velocity.x=dir.x*speed
    player.velocity.z=dir.z*speed
    player.velocity.y=0
    player.move_and_slide()
    player.global_position.x=clamp(player.global_position.x,-98.0,98.0)
    player.global_position.z=clamp(player.global_position.z,-98.0,98.0)
    camera.position=Vector3(0,3.5,7.0 if not driving else 9.0)
    camera.look_at(player.global_position+Vector3(0,1.1,0),Vector3.UP)
    if shooting and not driving:
        _fire()

func _fire():
    if not is_instance_valid(player):return
    shooting=false
    wanted=min(5,wanted+1)
    health=max(0,health-0.2)
    _tone(fire_audio,95.0,0.08)
    var flash=OmniLight3D.new()
    flash.light_color=Color("#ffbb55");flash.light_energy=7;flash.omni_range=6
    flash.position=player.global_position+Vector3(0,1,0)
    add_child(flash)
    get_tree().create_timer(0.06).timeout.connect(func():if is_instance_valid(flash):flash.queue_free())
    for n in npcs:
        if n.global_position.distance_to(player.global_position)<7 and rng.randf()<0.35:
            n.set_meta("speed",rng.randf_range(1.5,3.5))

func _animate_population(delta):
    for n in npcs:
        if not is_instance_valid(n):continue
        var phase=float(n.get_meta("phase"))+elapsed*float(n.get_meta("speed"))
        var dir=Vector3(sin(phase*0.31),0,cos(phase*0.27))
        n.position += dir*delta*0.55
        n.position.x=clamp(n.position.x,-96.0,96.0)
        n.position.z=clamp(n.position.z,-96.0,96.0)
        n.rotation.y=atan2(dir.x,dir.z)
        var bob=1.0+sin(phase*3.0)*0.04
        n.scale=Vector3(1,bob,1)

func _animate_cars(delta):
    for c in cars:
        if not is_instance_valid(c):continue
        var sp=float(c.get_meta("speed"));var dir=float(c.get_meta("dir"))
        c.position.z += sp*dir*delta
        if c.position.z>98:c.position.z=-98
        if c.position.z<-98:c.position.z=98
        c.rotation.y=0 if dir>0 else PI
    _tone(engine_audio,48.0+(driving*35.0),0.02)

func _update_time(delta):
    var hour=fmod(18.0+elapsed*0.18,24.0)
    sun.rotation_degrees=Vector3(-35.0-(hour-12.0)*2.0, -25,0)
    sun.light_energy=0.35 if hour>20 or hour<6 else 1.15

func _update_hud():
    hud.text="STREET SOVEREIGN 3D  |  HAMZA\n$%d   HP %d   WANTED %d/5   %s" % [money,int(health),wanted,districts[district]]
    objective.text="MISSION %d/6  —  %s\nReach the gold marker: %s" % [mission+1,mission_names[mission],districts[district]]
    status.text="3D WORLD • %d BUILDINGS • 1300+ PROPS • 110 NPCs • 65 VEHICLES   |   %s" % [555,["LOW","MEDIUM","ULTRA"][graphics]]

func _update_quality():
    if not sun:return
    sun.shadow_enabled=graphics>0
    sun.directional_shadow_max_distance=70 if graphics==0 else (100 if graphics==1 else 140)
    world_env.environment.ambient_light_energy=0.55 if graphics==0 else (0.75 if graphics==1 else 1.0)

func _show_map():
    status.text="MAP: VICE RAY  |  SAN VALORA  |  LIBERTY BAY  —  GOLD MARKER = CURRENT MISSION"
