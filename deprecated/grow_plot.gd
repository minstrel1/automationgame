@tool
class_name GrowingPlotGD extends BuildingGridPlacableGD

var plot_width = 2
var plot_length = 2
var border = 0.25

var max_grow_slots = plot_width * plot_length
var grow_slots: Array = []
var grow_slots_physical: Array[Node3D] = []

@export var grow_area: CSGBox3D

func _ready() -> void:
	super()
	adjust_box()

func on_build() -> void:
	var grow_area_width = grow_area.size.x - (border * 2)
	var grow_area_length = grow_area.size.z - (border * 2)
	var grow_area_height = grow_area.size.y
	for x in range(plot_width):
		for z in range(plot_length):
			var offset = Vector3((grow_area_width / plot_width) * x, -grow_area_height / 2, (grow_area_length / plot_length) * z)
			offset = offset 
			var coordinate = grow_area.global_position + offset - (Vector3(grow_area_width / 4, 0, grow_area_length / 4))
			var new_node = Growable.new()
			add_child(new_node)
			new_node.global_position = coordinate
			new_node.name = "{0}, {1}".format([x, z])
			grow_slots_physical.append(new_node)
			new_node.set_plant("test_plant")

func _physics_process(delta: float) -> void:
	pass

func _process(delta: float) -> void:
	pass
