class_name BuildingGridRaycaster extends RayCast3D

@export var interact_distance: float = 4.0

var interact_cast_result
var last_known_grid: BuildingGrid
var last_cast_position

var cursor: CSGSphere3D

var test_thing = preload("res://building_scenes/ExampleGrowingPlot.tscn")

var build_mode = false;

func _ready() -> void:

	target_position = target_position.normalized() * interact_distance

	var new_sphere = CSGSphere3D.new()
	new_sphere.radius = 0.5
	new_sphere.position = Vector3(0,0,0)
	new_sphere.use_collision = false
	add_child(new_sphere)
	cursor = new_sphere
	cursor.visible = false
	cursor.top_level = true
	cursor.physics_interpolation_mode = Node.PHYSICS_INTERPOLATION_MODE_OFF

func _process(delta: float) -> void:
	pass

func _physics_process(delta: float) -> void:
	if Input.is_action_just_pressed("build_mode"):
		if build_mode:
			for grid in BuildingGrid.get_grids():
				grid.set_mesh_visibility(false)

			build_mode = false
		else:
			for grid: BuildingGrid in BuildingGrid.get_grids():
				grid.set_mesh_visibility(true)

			build_mode = true
			pass
		#print(build_mode)

	cast()

func cast() -> void:
	var current_cast_result = get_collider()
	var current_cast_position = get_collision_point()

	if is_colliding():
		last_cast_position = current_cast_position
		#print(last_cast_position)

	if current_cast_result != interact_cast_result:
		if interact_cast_result and interact_cast_result.has_user_signal("unfocused"):
			interact_cast_result.emit_signal("unfocused")
		
		interact_cast_result = current_cast_result

	if build_mode:
		if interact_cast_result and (interact_cast_result is BuildingGridChunk):

			cursor.visible = true

			last_known_grid = (interact_cast_result as BuildingGridChunk).parent_grid

			var collision_normal = get_collision_normal()

			var hit_pos: Vector3i = last_known_grid.position_to_voxel(last_cast_position - (collision_normal * 0.5))

			var hit_chunk: Vector3i = last_known_grid.position_to_chunk(last_cast_position + (collision_normal * 0.5))

			var hit_voxel: Vector3i = last_known_grid.position_to_chunk_voxel(last_cast_position + (collision_normal * 0.5))

			var projected_pos = hit_pos + Vector3i(collision_normal.x, collision_normal.y, collision_normal.z)

			cursor.visible = true
			cursor.global_position = Vector3(projected_pos.x, projected_pos.y, projected_pos.z) + last_known_grid.global_position + Vector3(0.5, 0.5, 0.5)
			if last_known_grid.is_position_valid(hit_pos):
				#print(last_known_grid.get_block(hit_pos))
				pass
			#cursor.global_position = last_cast_position
			#print(cursor.global_position)

			#print(hit_chunk)
			#print(hit_voxel)

			if Input.is_action_just_pressed("click"):

				var new_instance: BuildingGridPlacable = test_thing.instantiate();
				new_instance.position = Vector3(projected_pos.x, projected_pos.y, projected_pos.z) + Vector3(0.5, 0.5, 0.5) 

				var corner_1 = projected_pos + new_instance.get_box_from()
				var corner_2 = projected_pos + new_instance.get_box_to()

				var test_result = last_known_grid.is_area_free(corner_1, corner_2)

				if test_result[0]:
					var index = last_known_grid.get_free_index()
					print("index of " + str(index))

					if index != -1:
						last_known_grid.add_child(new_instance)

						var affected_chunks = last_known_grid.set_area(corner_1, corner_2, index)

						for chunk in affected_chunks:
							(chunk as BuildingGridChunk).generate_mesh();
							(chunk as BuildingGridChunk).generate_mesh_neighbors();

						new_instance.parent_grid = last_known_grid
						last_known_grid.set_placable(index, new_instance);

						new_instance.on_build()
				else:
					new_instance.queue_free()

		else:
			cursor.visible = false
	else:
		cursor.visible = false
			
			
			
			

		

		
