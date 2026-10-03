extends Node3D

# Nitro Street Rush
# Original arcade racer: procedural cars, road, traffic, neon UI.
# Built for touch-first Android play and lightweight 3D rendering.

const ROAD_WIDTH := 18.0
const ROAD_LENGTH := 240.0
const PLAYER_Z := 4.0
const WORLD_WRAP_Z := 78.0
const WORLD_START_Z := -220.0
const MAX_DISTANCE := 2000.0
const BASE_SPEED := 28.0

var rng := RandomNumberGenerator.new()
var camera: Camera3D
var sun: DirectionalLight3D
var env: WorldEnvironment
var player_car: Node3D
var traffic: Array[Node3D] = []
var lane_markers: Array[MeshInstance3D] = []
var scenery: Array[Node3D] = []
var road: MeshInstance3D
var shoulder_left: MeshInstance3D
var shoulder_right: MeshInstance3D

var in_race := false
var paused := false
var finished := false
var elapsed := 0.0
var race_distance := 0.0
var current_speed := BASE_SPEED
var nitro := 100.0
var steering := 0.0
var steer_target := 0.0
var crash_timer := 0.0
var camera_shake := 0.0
var touch_start := Vector2.ZERO
var swipe_active := false

var menu_layer: CanvasLayer
var menu_panel: Panel
var race_layer: CanvasLayer
var hud_speed: Label
var hud_distance: Label
var hud_place: Label
var hud_status: Label
var nitro_fill: ProgressBar
var pause_button: Button
var nitro_button: Button
var left_button: Button
var right_button: Button
var finish_panel: Panel
var countdown_label: Label

func _ready() -> void:
    rng.seed = 24092026
    _setup_world()
    _setup_track()
    _setup_scenery()
    _setup_player()
    _setup_traffic()
    _setup_menu()
    _setup_race_hud()
    _set_race_state(false)

func _process(delta: float) -> void:
    elapsed += delta
    if in_race and not paused and not finished:
        _update_race(delta)
    _update_visuals(delta)

func _setup_world() -> void:
    RenderingServer.set_default_clear_color(Color("#070b1a"))

    env = WorldEnvironment.new()
    env.name = "WorldEnvironment"
    var environment := Environment.new()
    environment.background_mode = Environment.BG_COLOR
    environment.background_color = Color("#07101e")
    environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
    environment.ambient_light_color = Color("#9db5d9")
    environment.ambient_light_energy = 0.9
    environment.tonemap_mode = Environment.TONE_MAPPER_FILMIC
    env.environment = environment
    add_child(env)

    sun = DirectionalLight3D.new()
    sun.rotation_degrees = Vector3(-52.0, -22.0, 0.0)
    sun.light_energy = 1.2
    sun.shadow_enabled = true
    sun.directional_shadow_max_distance = 95.0
    add_child(sun)

    camera = Camera3D.new()
    camera.position = Vector3(0.0, 5.6, 10.5)
    add_child(camera)
    camera.current = true
    camera.look_at(Vector3(0.0, 0.8, -45.0), Vector3.UP)

func _setup_track() -> void:
    road = _box(Vector3(ROAD_WIDTH, 0.24, ROAD_LENGTH), Vector3(0.0, -0.12, -72.0), _mat(Color("#171b24"), 0.0, 0.94), "Road")
    shoulder_left = _box(Vector3(5.0, 0.16, ROAD_LENGTH), Vector3(-11.5, -0.10, -72.0), _mat(Color("#303542"), 0.0, 0.92), "ShoulderL")
    shoulder_right = _box(Vector3(5.0, 0.16, ROAD_LENGTH), Vector3(11.5, -0.10, -72.0), _mat(Color("#303542"), 0.0, 0.92), "ShoulderR")

    for lane_x in [-3.0, 3.0]:
        for i in range(28):
            var marker := _box(
                Vector3(0.22, 0.035, 3.2),
                Vector3(lane_x, 0.04, WORLD_START_Z + float(i) * 10.0),
                _mat(Color("#dfe8f2"), 0.0, 0.58),
                "LaneMarker"
            )
            lane_markers.append(marker as MeshInstance3D)

    for side in [-1, 1]:
        for i in range(18):
            var pole := Node3D.new()
            pole.name = "LightPole"
            pole.position = Vector3(13.0 * side, 0.0, WORLD_START_Z + float(i) * 16.0)
            var post := _box(Vector3(0.18, 4.2, 0.18), Vector3(0, 2.1, 0), _mat(Color("#252b38"), 0.25, 0.55), "Post")
            pole.add_child(post)
            var lamp := _box(Vector3(0.7, 0.18, 0.7), Vector3(-0.65 * side, 4.2, 0), _mat(Color("#78dfff"), 0.1, 0.25, Color("#55d9ff")), "Lamp")
            pole.add_child(lamp)
            add_child(pole)
            scenery.append(pole)

func _setup_scenery() -> void:
    var building_mats := [
        _mat(Color("#27314a"), 0.1, 0.82),
        _mat(Color("#3d2a52"), 0.05, 0.88),
        _mat(Color("#233f43"), 0.1, 0.84),
        _mat(Color("#4b3540"), 0.05, 0.88)
    ]

    for i in range(28):
        var side := -1 if i % 2 == 0 else 1
        var root := Node3D.new()
        root.name = "Building_%02d" % i
        var z := WORLD_START_Z + rng.randf_range(0.0, 230.0)
        root.position = Vector3(side * rng.randf_range(18.0, 29.0), 0.0, z)
        var w := rng.randf_range(5.0, 10.0)
        var h := rng.randf_range(5.0, 17.0)
        var d := rng.randf_range(5.0, 11.0)
        root.add_child(_box(Vector3(w, h, d), Vector3(0, h * 0.5, 0), building_mats[i % building_mats.size()], "Building"))
        for row in range(3):
            var window := _box(
                Vector3(w * 0.55, 0.10, 0.18),
                Vector3(0, 1.1 + row * (h / 3.2), -d * 0.52),
                _mat(Color("#89c8ff"), 0.0, 0.3, Color("#4ca8ff")),
                "Window"
            )
            root.add_child(window)
        add_child(root)
        scenery.append(root)

    for i in range(20):
        var prop := Node3D.new()
        prop.name = "Barrier_%02d" % i
        var side := -1 if i % 2 == 0 else 1
        prop.position = Vector3(side * rng.randf_range(10.0, 14.0), 0.0, WORLD_START_Z + rng.randf_range(0.0, 220.0))
        prop.add_child(_box(Vector3(1.2, 0.9, 0.7), Vector3.ZERO, _mat(Color("#d74e2e"), 0.0, 0.72), "Barrier"))
        add_child(prop)
        scenery.append(prop)

func _setup_player() -> void:
    player_car = _make_car(Color("#ff3b30"), true)
    player_car.position = Vector3(0.0, 0.0, PLAYER_Z)
    add_child(player_car)

func _setup_traffic() -> void:
    var colors := [
        Color("#18d9ff"), Color("#ffd84d"), Color("#8c6bff"),
        Color("#35e78b"), Color("#f16cff"), Color("#ff7a18")
    ]
    for i in range(9):
        var car := _make_car(colors[i % colors.size()], false)
        var lane := [-6.0, 0.0, 6.0][i % 3]
        car.position = Vector3(lane, 0.0, WORLD_START_Z + float(i) * 24.0 + rng.randf_range(-5.0, 5.0))
        car.rotation.y = PI
        car.set_meta("speed_mul", rng.randf_range(0.78, 1.12))
        car.set_meta("lane", lane)
        add_child(car)
        traffic.append(car)

func _make_car(body_color: Color, is_player: bool) -> Node3D:
    var root := Node3D.new()
    root.name = "PlayerCar" if is_player else "TrafficCar"

    var body := _box(Vector3(2.8, 0.72, 5.2), Vector3(0, 0.72, 0), _mat(body_color, 0.28, 0.38), "Body")
    root.add_child(body)

    var lower := _box(Vector3(2.45, 0.35, 3.8), Vector3(0, 0.48, 0.20), _mat(body_color.lightened(0.16), 0.3, 0.32), "Lower")
    root.add_child(lower)

    var glass := _box(Vector3(2.15, 0.62, 1.85), Vector3(0, 1.16, -0.22), _mat(Color("#0d1828"), 0.42, 0.22), "Glass")
    root.add_child(glass)

    var hood := _box(Vector3(2.2, 0.22, 1.5), Vector3(0, 1.02, 1.55), _mat(body_color.lightened(0.05), 0.25, 0.34), "Hood")
    root.add_child(hood)

    for x in [-1.34, 1.34]:
        for z in [-1.72, 1.72]:
            var wheel := MeshInstance3D.new()
            var wm := CylinderMesh.new()
            wm.top_radius = 0.52
            wm.bottom_radius = 0.52
            wm.height = 0.34
            wheel.mesh = wm
            wheel.material_override = _mat(Color("#090c12"), 0.15, 0.96)
            wheel.rotation_degrees = Vector3(90, 0, 0)
            wheel.position = Vector3(x, 0.48, z)
            root.add_child(wheel)

    var light_mat := _mat(Color("#ffffff"), 0.0, 0.18, Color("#ffffff"))
    root.add_child(_box(Vector3(0.38, 0.16, 0.10), Vector3(-0.78, 0.93, 2.62), light_mat, "HeadLight"))
    root.add_child(_box(Vector3(0.38, 0.16, 0.10), Vector3(0.78, 0.93, 2.62), light_mat, "HeadLight"))
    root.add_child(_box(Vector3(0.44, 0.16, 0.12), Vector3(-0.82, 0.88, -2.62), _mat(Color("#ff3a45"), 0.0, 0.2, Color("#ff2638")), "TailLight"))
    root.add_child(_box(Vector3(0.44, 0.16, 0.12), Vector3(0.82, 0.88, -2.62), _mat(Color("#ff3a45"), 0.0, 0.2, Color("#ff2638")), "TailLight"))

    if is_player:
        var spoiler := _box(Vector3(2.35, 0.20, 0.25), Vector3(0, 1.22, -2.1), _mat(Color("#121725"), 0.5, 0.3), "Spoiler")
        root.add_child(spoiler)
        var nitro_left := _box(Vector3(0.34, 0.34, 0.34), Vector3(-0.72, 0.55, -2.72), _mat(Color("#ff922e"), 0.0, 0.2, Color("#ff5c20")), "NitroGlow")
        var nitro_right := nitro_left.duplicate()
        nitro_right.position.x = 0.72
        root.add_child(nitro_left)
        root.add_child(nitro_right)

    return root

func _update_race(delta: float) -> void:
    steering = lerp(steering, steer_target, min(1.0, delta * 10.0))

    var nitro_active := Input.is_action_pressed("ui_accept")
    if nitro_active and nitro > 0.5:
        current_speed = lerp(current_speed, BASE_SPEED * 1.9, delta * 8.0)
        nitro = max(0.0, nitro - 26.0 * delta)
        hud_status.text = "NITRO BOOST"
    else:
        current_speed = lerp(current_speed, BASE_SPEED, delta * 3.5)
        nitro = min(100.0, nitro + 7.0 * delta)
        if crash_timer <= 0.0:
            hud_status.text = "RACE LIVE"

    if abs(steering) > 0.05:
        player_car.rotation.y = lerp_angle(player_car.rotation.y, steering * 0.18, delta * 8.0)
    else:
        player_car.rotation.y = lerp_angle(player_car.rotation.y, 0.0, delta * 6.0)

    player_car.position.x = clamp(player_car.position.x + steering * 8.6 * delta, -6.4, 6.4)
    race_distance += current_speed * delta

    var world_scroll := current_speed * delta
    for marker in lane_markers:
        marker.position.z += world_scroll
        if marker.position.z > WORLD_WRAP_Z:
            marker.position.z -= 280.0

    for item in scenery:
        item.position.z += world_scroll
        if item.position.z > WORLD_WRAP_Z:
            item.position.z -= 280.0

    for car in traffic:
        car.position.z += world_scroll * float(car.get_meta("speed_mul", 1.0))
        if car.position.z > 18.0:
            car.position.z = WORLD_START_Z + rng.randf_range(-6.0, 18.0)
            car.position.x = [-6.0, 0.0, 6.0][rng.randi_range(0, 2)]
            car.set_meta("lane", car.position.x)
        if abs(car.position.z - PLAYER_Z) < 4.2 and abs(car.position.x - player_car.position.x) < 2.0:
            _crash()

    crash_timer = max(0.0, crash_timer - delta)
    if race_distance >= MAX_DISTANCE:
        _finish_race()

    hud_speed.text = "%03d" % int(current_speed * 3.6)
    hud_distance.text = "%04d M" % int(min(MAX_DISTANCE, race_distance))
    hud_place.text = "P %d/10" % (1 + int(race_distance / 380.0) % 3)
    nitro_fill.value = nitro

func _crash() -> void:
    if crash_timer > 0.0:
        return
    crash_timer = 1.05
    camera_shake = 0.55
    current_speed = BASE_SPEED * 0.58
    nitro = max(0.0, nitro - 22.0)
    hud_status.text = "CRASH!"

func _finish_race() -> void:
    finished = true
    hud_status.text = "FINISH!"
    finish_panel.visible = true
    var result_label := finish_panel.get_node("Result") as Label
    result_label.text = "FINISH\nDISTANCE  %04d M\nSPEED  %03d KM/H" % [int(MAX_DISTANCE), int(current_speed * 3.6)]

func _start_race() -> void:
    in_race = true
    paused = false
    finished = false
    race_distance = 0.0
    nitro = 100.0
    current_speed = BASE_SPEED
    player_car.position = Vector3(0, 0, PLAYER_Z)
    steering = 0.0
    steer_target = 0.0
    finish_panel.visible = false
    menu_panel.visible = false
    race_layer.visible = true
    hud_status.text = "RACE LIVE"
    _countdown()

func _countdown() -> void:
    countdown_label.visible = true
    countdown_label.text = "3"
    await get_tree().create_timer(0.55).timeout
    if not in_race: return
    countdown_label.text = "2"
    await get_tree().create_timer(0.55).timeout
    if not in_race: return
    countdown_label.text = "1"
    await get_tree().create_timer(0.55).timeout
    if not in_race: return
    countdown_label.text = "GO!"
    await get_tree().create_timer(0.45).timeout
    countdown_label.visible = false

func _set_race_state(active: bool) -> void:
    in_race = active
    race_layer.visible = active
    menu_panel.visible = not active

func _setup_menu() -> void:
    menu_layer = CanvasLayer.new()
    add_child(menu_layer)

    menu_panel = Panel.new()
    menu_panel.set_anchors_preset(Control.PRESET_FULL_RECT)
    menu_panel.add_theme_stylebox_override("panel", _panel_style(Color(0.02,0.03,0.07,0.94), 32))
    menu_layer.add_child(menu_panel)

    var title := Label.new()
    title.text = "NITRO\nSTREET RUSH"
    title.position = Vector2(70, 82)
    title.add_theme_font_size_override("font_size", 68)
    title.add_theme_color_override("font_color", Color("#ffffff"))
    menu_panel.add_child(title)

    var subtitle := Label.new()
    subtitle.text = "ARCADE HIGH-SPEED SPRINT"
    subtitle.position = Vector2(76, 245)
    subtitle.add_theme_font_size_override("font_size", 24)
    subtitle.add_theme_color_override("font_color", Color("#4bdcff"))
    menu_panel.add_child(subtitle)

    var info := Label.new()
    info.text = "TOUCH / SWIPE TO STEER   •   HOLD NITRO TO BOOST"
    info.position = Vector2(76, 298)
    info.add_theme_font_size_override("font_size", 20)
    info.add_theme_color_override("font_color", Color("#b9c9df"))
    menu_panel.add_child(info)

    var start := Button.new()
    start.text = "START RACE"
    start.position = Vector2(76, 380)
    start.size = Vector2(330, 82)
    start.add_theme_font_size_override("font_size", 30)
    start.add_theme_stylebox_override("normal", _panel_style(Color("#ff4d2e"), 18))
    start.add_theme_stylebox_override("hover", _panel_style(Color("#ff6c4c"), 18))
    start.pressed.connect(_start_race)
    menu_panel.add_child(start)

    var version := Label.new()
    version.text = "v1.0  •  ORIGINAL ARCADE BUILD"
    version.position = Vector2(78, 500)
    version.add_theme_font_size_override("font_size", 16)
    version.add_theme_color_override("font_color", Color("#64748b"))
    menu_panel.add_child(version)

func _setup_race_hud() -> void:
    race_layer = CanvasLayer.new()
    add_child(race_layer)
    race_layer.visible = false

    hud_status = _label("RACE LIVE", Vector2(24, 22), 22, Color("#8af7ff"))
    race_layer.add_child(hud_status)

    hud_place = _label("P 1/10", Vector2(1080, 24), 28, Color("#ffffff"))
    race_layer.add_child(hud_place)

    hud_distance = _label("0000 M", Vector2(1080, 62), 20, Color("#a7b4c8"))
    race_layer.add_child(hud_distance)

    var speed_title := _label("KM/H", Vector2(48, 558), 16, Color("#8ba0bc"))
    race_layer.add_child(speed_title)

    hud_speed = _label("101", Vector2(42, 578), 62, Color("#ffffff"))
    race_layer.add_child(hud_speed)

    nitro_fill = ProgressBar.new()
    nitro_fill.position = Vector2(40, 650)
    nitro_fill.size = Vector2(330, 26)
    nitro_fill.min_value = 0
    nitro_fill.max_value = 100
    nitro_fill.value = 100
    nitro_fill.show_percentage = false
    nitro_fill.add_theme_stylebox_override("background", _panel_style(Color("#162033"), 13))
    nitro_fill.add_theme_stylebox_override("fill", _panel_style(Color("#18d9ff"), 13))
    race_layer.add_child(nitro_fill)

    var nitro_text := _label("NITRO", Vector2(42, 620), 17, Color("#55dfff"))
    race_layer.add_child(nitro_text)

    left_button = _control_button("‹", Vector2(36, 350), Vector2(120, 120), 58)
    left_button.button_down.connect(func(): steer_target = -1.0)
    left_button.button_up.connect(func(): steer_target = 0.0)
    race_layer.add_child(left_button)

    right_button = _control_button("›", Vector2(165, 350), Vector2(120, 120), 58)
    right_button.button_down.connect(func(): steer_target = 1.0)
    right_button.button_up.connect(func(): steer_target = 0.0)
    race_layer.add_child(right_button)

    nitro_button = _control_button("N₂O", Vector2(1030, 530), Vector2(190, 130), 32)
    nitro_button.button_down.connect(_nitro_down)
    nitro_button.button_up.connect(_nitro_up)
    race_layer.add_child(nitro_button)

    pause_button = _control_button("Ⅱ", Vector2(1145, 22), Vector2(88, 58), 24)
    pause_button.pressed.connect(_toggle_pause)
    race_layer.add_child(pause_button)

    countdown_label = _label("3", Vector2(600, 270), 96, Color("#ffffff"))
    countdown_label.visible = false
    race_layer.add_child(countdown_label)

    finish_panel = Panel.new()
    finish_panel.name = "FinishPanel"
    finish_panel.position = Vector2(410, 165)
    finish_panel.size = Vector2(460, 390)
    finish_panel.add_theme_stylebox_override("panel", _panel_style(Color(0.03,0.04,0.10,0.96), 26))
    finish_panel.visible = false
    race_layer.add_child(finish_panel)

    var result := _label("FINISH", Vector2(58, 48), 54, Color("#ffffff"))
    result.name = "Result"
    finish_panel.add_child(result)

    var again := Button.new()
    again.text = "RACE AGAIN"
    again.position = Vector2(58, 245)
    again.size = Vector2(344, 72)
    again.add_theme_font_size_override("font_size", 26)
    again.add_theme_stylebox_override("normal", _panel_style(Color("#18aee8"), 16))
    again.pressed.connect(_start_race)
    finish_panel.add_child(again)

func _toggle_pause() -> void:
    paused = not paused
    hud_status.text = "PAUSED" if paused else "RACE LIVE"

func _nitro_down() -> void:
    Input.action_press("ui_accept")

func _nitro_up() -> void:
    Input.action_release("ui_accept")

func _unhandled_input(event: InputEvent) -> void:
    if not in_race or paused:
        return

    if event is InputEventScreenTouch:
        if event.pressed:
            touch_start = event.position
            swipe_active = true
        else:
            swipe_active = false
            steer_target = 0.0

    if event is InputEventScreenDrag and swipe_active:
        var dx := event.position.x - touch_start.x
        steer_target = clamp(dx / 180.0, -1.0, 1.0)

    if event is InputEventMouseMotion and Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
        var dxm := event.position.x - touch_start.x
        steer_target = clamp(dxm / 180.0, -1.0, 1.0)

func _nitro_visual(delta: float) -> void:
    if not player_car:
        return
    var active := Input.is_action_pressed("ui_accept") and nitro > 0.5
    for node in player_car.get_children():
        if String(node.name).begins_with("NitroGlow"):
            var mi := node as MeshInstance3D
            if mi:
                mi.scale = Vector3.ONE * (1.0 + (0.65 + sin(elapsed * 20.0) * 0.18 if active else 0.0))

func _update_visuals(delta: float) -> void:
    _nitro_visual(delta)
    if camera_shake > 0.0:
        camera_shake = max(0.0, camera_shake - delta * 2.8)
        camera.position = Vector3(
            rng.randf_range(-camera_shake, camera_shake),
            5.6 + rng.randf_range(-camera_shake, camera_shake),
            10.5
        )
        camera.look_at(Vector3(player_car.position.x * 0.45, 0.8, -45.0), Vector3.UP)
    else:
        camera.position = Vector3(0.0, 5.6, 10.5)
        camera.look_at(Vector3(player_car.position.x * 0.32, 0.8, -45.0), Vector3.UP)

func _label(text_value: String, pos: Vector2, font_size: int, color: Color) -> Label:
    var l := Label.new()
    l.text = text_value
    l.position = pos
    l.add_theme_font_size_override("font_size", font_size)
    l.add_theme_color_override("font_color", color)
    return l

func _control_button(text_value: String, pos: Vector2, size_value: Vector2, font_size: int) -> Button:
    var b := Button.new()
    b.text = text_value
    b.position = pos
    b.size = size_value
    b.add_theme_font_size_override("font_size", font_size)
    b.add_theme_color_override("font_color", Color("#ffffff"))
    b.add_theme_stylebox_override("normal", _panel_style(Color(0.05,0.08,0.15,0.88), 24))
    b.add_theme_stylebox_override("hover", _panel_style(Color(0.10,0.16,0.25,0.95), 24))
    b.add_theme_stylebox_override("pressed", _panel_style(Color(0.11,0.28,0.38,0.98), 24))
    return b

func _panel_style(color: Color, radius: int) -> StyleBoxFlat:
    var s := StyleBoxFlat.new()
    s.bg_color = color
    s.corner_radius_top_left = radius
    s.corner_radius_top_right = radius
    s.corner_radius_bottom_left = radius
    s.corner_radius_bottom_right = radius
    s.border_width_left = 1
    s.border_width_top = 1
    s.border_width_right = 1
    s.border_width_bottom = 1
    s.border_color = Color(1,1,1,0.10)
    return s

func _mat(color: Color, metallic: float = 0.0, roughness: float = 0.7, emission: Color = Color(0,0,0,0)) -> StandardMaterial3D:
    var m := StandardMaterial3D.new()
    m.albedo_color = color
    m.metallic = metallic
    m.roughness = roughness
    if emission.a > 0.0:
        m.emission_enabled = true
        m.emission = emission
        m.emission_energy_multiplier = 3.0
    return m

func _box(size_value: Vector3, pos: Vector3, material_value: Material, node_name: String) -> MeshInstance3D:
    var mi := MeshInstance3D.new()
    var mesh := BoxMesh.new()
    mesh.size = size_value
    mi.mesh = mesh
    mi.material_override = material_value
    mi.position = pos
    mi.name = node_name
    return mi
