@tool
class_name BuildingGridPlacableGD extends Node3D

@export_category("Grid Properties")
@export var grid_width: int = 3 :
	set(value):
		grid_width = value
		adjust_box()
@export var grid_height: int = 3 :
	set(value):
		grid_height = value
		adjust_box()
@export var grid_length: int = 3 :
	set(value):
		grid_length = value
		adjust_box()

@export var grid_offset: Vector3 = Vector3.ZERO :
	set(value):
		grid_offset = value
		adjust_box()

var visualiser: MeshInstance3D

func _ready() -> void:
	if Engine.is_editor_hint():
		visualiser = MeshInstance3D.new()
		visualiser.mesh = BoxMesh.new()
		var material = load("res://selection_box_texture.tres").duplicate()
		visualiser.set_surface_override_material(0, material)
		add_child(visualiser)
		adjust_box()

func adjust_box () -> void:
	if visualiser != null:
		visualiser.mesh.size = Vector3(grid_width, grid_height, grid_length)
		(visualiser.get_surface_override_material(0) as StandardMaterial3D).uv1_scale = Vector3(3 * grid_width, 2 * grid_height, 1 * grid_length)
		visualiser.position = grid_offset


func on_build() -> void:
	pass

func _process(delta: float) -> void:
	pass

func _physics_process(delta: float) -> void:
	pass
