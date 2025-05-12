class_name PlayerGD extends CharacterBody3D

@export_category("Player")
@export_range(1, 35, 1) var speed: float = 10 # m/s
@export_range(10, 400, 1) var acceleration: float = 100 # m/s^2
@export_range(10, 400, 1) var deceleration: float = 100 # m/s^2

@export var can_jump: bool = false
@export_range(0.1, 3.0, 0.1) var jump_height: float = 1 # m
@export_range(0.1, 3.0, 0.1, "or_greater") var camera_sens: float = 1

@export_category("Camera")
@export var head_bob: bool = true
@export var side_amplitude: float = 0.08
@export var side_frequency: float = 3.3
@export var vertical_amplitude: float = 0.05
@export var vertical_frequency: float = 6.6

@export_category("Debug")
@export var draw_raycast_lines: bool = false

var jumping: bool = false
var mouse_captured: bool = false

var gravity: float = ProjectSettings.get_setting("physics/3d/default_gravity")

var move_dir: Vector2 # Input direction for movement
var look_dir: Vector2 # Input direction for look/aim

var walk_vel: Vector3 # Walking velocity 
var grav_vel: Vector3 # Gravity velocity 
var jump_vel: Vector3 # Jumping velocity

@onready var camera: Node3D = $Camera
@onready var actual_renderer: Camera3D = $Camera/Camera3D

# @onready var footstep_1: AudioStreamPlayer3D = $SoundEffects/Footstep1
# @onready var footstep_2: AudioStreamPlayer3D = $SoundEffects/Footstep2
# @onready var footstep_3: AudioStreamPlayer3D = $SoundEffects/Footstep3
# @onready var footstep_4: AudioStreamPlayer3D = $SoundEffects/Footstep4

var controls_locked = false
var phone_mode = false
var camera_mode = false

var walk_time
var stepped = false

var inventory: Inventory = Inventory.new_inventory(10)


func _ready () -> void:
	capture_mouse()
	var count = 527;
	while count > 527 - 500:
		var result = inventory.insert(InventoryItem.new_item("test_item", randi_range(1, 50)))
		print("Insert result: " + str(result))
		count -= result

	
	print(str(inventory))
	#Globals.player = self

func _process (delta: float) -> void:
	pass

func _physics_process (delta: float) -> void:
	
	if not controls_locked:
		walk_vel = _walk(delta)
		grav_vel = _gravity(delta)
		jump_vel = _jump(delta)
	else:
		walk_vel = Vector3.ZERO
		grav_vel = _gravity(delta)
		jump_vel = Vector3.ZERO
		
	velocity = walk_vel + grav_vel + jump_vel
	move_and_slide()
	
	if head_bob and delta:
		handle_bob(walk_vel, delta)

func _walk(delta: float) -> Vector3:
	move_dir = Input.get_vector("move_left", "move_right", "move_forward", "move_backward")
	var _forward: Vector3 = camera.global_transform.basis * Vector3(move_dir.x, 0, move_dir.y)
	var walk_dir: Vector3 = Vector3(_forward.x, 0, _forward.z).normalized()
	if walk_dir == Vector3.ZERO:
		walk_vel = walk_vel.move_toward(walk_dir * speed * move_dir.length(), deceleration * delta)
	else:
		walk_vel = walk_vel.move_toward(walk_dir * speed * move_dir.length(), acceleration * delta)
	return walk_vel

func _gravity(delta: float) -> Vector3:
	grav_vel = Vector3.ZERO if is_on_floor() else grav_vel.move_toward(Vector3(0, velocity.y - gravity, 0), gravity * delta)
	return grav_vel

func _jump(delta: float) -> Vector3:
	if not can_jump:
		return Vector3.ZERO
	if jumping:
		if is_on_floor(): jump_vel = Vector3(0, sqrt(4 * jump_height * gravity), 0)
		jumping = false
		return jump_vel
	jump_vel = Vector3.ZERO if is_on_floor() else jump_vel.move_toward(Vector3.ZERO, gravity * delta)
	return jump_vel

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		look_dir = event.relative * 0.001
		if mouse_captured: 
			if not controls_locked:
				_rotate_camera()
	if Input.is_action_just_pressed("jump"): jumping = true
	if Input.is_action_just_pressed("close"): 
		if mouse_captured:
			release_mouse()
		else:
			capture_mouse()

func handle_bob(walk_vel: Vector3, delta: float) -> void:
	var offset = actual_renderer.position
	if walk_vel.length() > 0.5:
		walk_time += delta
		var x = cos(walk_time * side_frequency) * side_amplitude
		var y = sin(walk_time * vertical_frequency) * vertical_amplitude
		
		if y < -0.03:
			if not stepped:
				stepped = true
				#play_footstep()
		else:
			stepped = false
			
		var new_offset = Vector3(x, y, 0)
		offset = offset.move_toward(new_offset, (walk_vel.length() / speed) * delta)
	else:
		walk_time = 0
		offset = offset.move_toward(Vector3(0, 0, 0), delta)
		
	actual_renderer.position = offset

func _rotate_camera(sens_mod: float = 1.0) -> void:
	camera.rotation.y -= look_dir.x * camera_sens * sens_mod
	camera.rotation.x = clamp(camera.rotation.x - look_dir.y * camera_sens * sens_mod, -1.5, 1.5)
	
# func play_footstep() -> void:
# 	var choices = [footstep_1, footstep_2, footstep_3, footstep_4]
# 	var result = choices.pick_random()
# 	result.play()
# 	pass
	
# func stop_footsteps() -> void:
# 	footstep_1.stop()
# 	footstep_2.stop()
# 	footstep_3.stop()
# 	footstep_4.stop()

func capture_mouse() -> void:
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	mouse_captured = true

func release_mouse() -> void:
	Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
	mouse_captured = false

func lock() -> void:
	controls_locked = true

func unlock() -> void:
	controls_locked = false

func on_interact_target_changed () -> void:
	pass
