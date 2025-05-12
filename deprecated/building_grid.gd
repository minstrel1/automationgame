@tool
class_name BuildingGridGD extends StaticBody3D

@export var update_thing: bool = false

@export var mesh_material: Material

@export_category("Grid Sizes")
@export var grid_width: int = 32
@export var grid_height: int = 32
@export var grid_length: int = 32

var mesh = ArrayMesh.new()
var vertices = PackedVector3Array()
var indices = PackedInt32Array()
var uvs = PackedVector2Array()

var pre_mesh_data = []

var mesh_instance: MeshInstance3D
var collision_shape: CollisionShape3D

var face_count = 0
var tex_div = 1

var voxel_data = [] 
var actual_voxels = []

func _ready() -> void:
	mesh_instance = MeshInstance3D.new()
	mesh_instance.material_override = mesh_material
	add_child(mesh_instance)

	collision_shape = CollisionShape3D.new()
	add_child(collision_shape)

	init_voxel_array()
	generate_pre_mesh()
	generate_mesh()
	pass

func _process(delta: float) -> void:
	if update_thing:
		update_thing = false
		generate_mesh()

func _physics_process(delta: float) -> void:
	pass

func set_block (x: int, y: int, z: int, value: int) -> void:
	voxel_data[x][y][z] = value

func init_voxel_array() -> void:
	voxel_data.resize(grid_width)
	for x in range(grid_width):
		voxel_data[x] = []
		for y in range(grid_height):
			voxel_data[x].append([])
			for z in range(grid_length):
				voxel_data[x][y].append(null)

func add_uvs(x: int, y: int) -> void:
	uvs.append(Vector2(tex_div * x, tex_div * y))
	uvs.append(Vector2(tex_div * x + tex_div, tex_div * y))
	uvs.append(Vector2(tex_div * x + tex_div, tex_div * y + tex_div))
	uvs.append(Vector2(tex_div * x, tex_div * y + tex_div))

func add_tris():
	indices.append(face_count * 4 + 0)
	indices.append(face_count * 4 + 1)
	indices.append(face_count * 4 + 2)
	indices.append(face_count * 4 + 0)
	indices.append(face_count * 4 + 2)
	indices.append(face_count * 4 + 3)
	face_count += 1

func add_cube (pos: Vector3) -> void:

	# vertices.append(pos + Vector3(-0.5,  0.5, -0.5))
	# vertices.append(pos + Vector3( 0.5,  0.5, -0.5))
	# vertices.append(pos + Vector3( 0.5,  0.5,  0.5))
	# vertices.append(pos + Vector3(-0.5,  0.5,  0.5))
	# vertices.append(pos + Vector3(-0.5, -0.5, -0.5))
	# vertices.append(pos + Vector3( 0.5, -0.5, -0.5))
	# vertices.append(pos + Vector3( 0.5, -0.5,  0.5))
	# vertices.append(pos + Vector3(-0.5, -0.5,  0.5))

	if is_block_free(pos + Vector3(0, 1, 0)): # top face
		#print("adding top")
		vertices.append(pos + Vector3(-0.5,  0.5, -0.5))
		vertices.append(pos + Vector3( 0.5,  0.5, -0.5))
		vertices.append(pos + Vector3( 0.5,  0.5,  0.5))
		vertices.append(pos + Vector3(-0.5,  0.5,  0.5))

		add_tris()
		add_uvs(0, 0)

	if is_block_free(pos + Vector3(0, -1, 0)): # bottom face
		#print("adding bottom")
		vertices.append(pos + Vector3(-0.5,  -0.5,  0.5))
		vertices.append(pos + Vector3( 0.5,  -0.5,  0.5))
		vertices.append(pos + Vector3( 0.5,  -0.5, -0.5))
		vertices.append(pos + Vector3(-0.5,  -0.5, -0.5))

		add_tris()
		add_uvs(0, 0)

	if is_block_free(pos + Vector3(1, 0, 0)): # east face
		#print("adding east")
		vertices.append(pos + Vector3( 0.5,  0.5,  0.5))
		vertices.append(pos + Vector3( 0.5,  0.5, -0.5))
		vertices.append(pos + Vector3( 0.5, -0.5, -0.5))
		vertices.append(pos + Vector3( 0.5, -0.5,  0.5))

		add_tris()
		add_uvs(0, 0)

	if is_block_free(pos + Vector3(-1, 0, 0)): # west face
		#print("adding west")
		vertices.append(pos + Vector3(-0.5,  0.5, -0.5))
		vertices.append(pos + Vector3(-0.5,  0.5,  0.5))
		vertices.append(pos + Vector3(-0.5, -0.5,  0.5))
		vertices.append(pos + Vector3(-0.5, -0.5, -0.5))

		add_tris()
		add_uvs(0, 0)

	if is_block_free(pos + Vector3(0, 0, 1)): # north face
		#print("adding north")
		vertices.append(pos + Vector3(-0.5,  0.5,  0.5))
		vertices.append(pos + Vector3( 0.5,  0.5,  0.5))
		vertices.append(pos + Vector3( 0.5, -0.5,  0.5))
		vertices.append(pos + Vector3(-0.5, -0.5,  0.5))
		
		add_tris()
		add_uvs(0, 0)

	if is_block_free(pos + Vector3(0, 0, -1)): # south face
		#print("adding south")
		vertices.append(pos + Vector3( 0.5,  0.5,  -0.5))
		vertices.append(pos + Vector3(-0.5,  0.5, -0.5))
		vertices.append(pos + Vector3(-0.5, -0.5, -0.5))
		vertices.append(pos + Vector3( 0.5, -0.5, -0.5))

		add_tris()
		add_uvs(0, 0)

	

func is_block_free (pos: Vector3) -> bool:
	if pos.y < 0:
		return false
	elif pos.x < 0 or pos.y < 0 or pos.z < 0:
		return true
	elif pos.x >= grid_width or pos.y >= grid_height or pos.z >= grid_length:
		return true
	
	
	return voxel_data[pos.x][pos.y][pos.z] == null

func generate_pre_mesh () -> void:
	vertices = PackedVector3Array()
	indices = PackedInt32Array()
	uvs = PackedVector2Array()

	face_count = 0

	for x in range(grid_width):
		for z in range(grid_length):
			add_cube(Vector3(x, -1, z))

	for x in range(grid_width):
		for y in range(grid_height):
			for z in range(grid_length):
				if voxel_data[x][y][z]:
					add_cube(Vector3(x, y, z))

	var array = []
	array.resize(Mesh.ARRAY_MAX)
	array[Mesh.ARRAY_VERTEX] = vertices
	array[Mesh.ARRAY_INDEX] = indices
	array[Mesh.ARRAY_TEX_UV] = uvs

	pre_mesh_data = array


func generate_mesh () -> void:
	mesh = ArrayMesh.new()
	vertices = PackedVector3Array()
	indices = PackedInt32Array()
	uvs = PackedVector2Array()

	face_count = 0

	var start = Time.get_ticks_usec()
	for x in range(grid_width):
		for y in range(grid_height):
			for z in range(grid_length):
				if voxel_data[x][y][z]:
					add_cube(Vector3(x, y, z))
	
	var time = Time.get_ticks_usec() - start
	print("GD ARRAY TIME:" + str(time))


	
	var array = []
	array.resize(Mesh.ARRAY_MAX)
	array[Mesh.ARRAY_VERTEX] = vertices
	array[Mesh.ARRAY_INDEX] = indices
	array[Mesh.ARRAY_TEX_UV] = uvs

	start = Time.get_ticks_usec()
	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, pre_mesh_data)
	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, array)
	time = Time.get_ticks_usec() - start
	print("GD MESH TIME:" + str(time))

	start = Time.get_ticks_usec()
	var new_collision_shape = mesh.create_trimesh_shape()
	collision_shape.shape = new_collision_shape
	time = Time.get_ticks_usec() - start
	print("GD COLLISION TIME:" + str(time))


	mesh_instance.mesh = mesh

	
