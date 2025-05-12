class_name GrowableGD extends Node3D

var plant_name = ""
var growth_time = 0
var current_growth_time = 0
var plant_set = false

var model

func _ready() -> void:
	var new_sphere = CSGSphere3D.new()
	new_sphere.radius = 0.0000001
	new_sphere.position = Vector3(0,0,0)
	new_sphere.use_collision = true
	add_child(new_sphere)
	model = new_sphere
	model.visible = false
	pass

func _physics_process(delta: float) -> void:
	if current_growth_time < growth_time:
		current_growth_time += 1
		var scale = float(current_growth_time) / growth_time
		model.radius = scale
		#print(current_growth_time)
		if current_growth_time == growth_time:
			pass

func _process(delta: float) -> void:
	pass

func set_plant (new_plant_name: String) -> void:

	if new_plant_name in Globals.growables:
		var plant_data = Globals.growables[new_plant_name]
		plant_name = new_plant_name
		current_growth_time = 0
		growth_time = Globals.hours_to_ticks(plant_data["time_to_grow"])
		plant_set = true

		model.visible = true
		model.radius = 0.000001