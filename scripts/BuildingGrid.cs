using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public struct VoxelData {
	public int placable_index;
	public BuildDirection direction_built;
	public BuildDirectionFlags support_directions;

	public bool is_special_voxel;
	public SpecialVoxel special_voxel;
	public BuildDirectionFlags special_directions;
	public SpecialVoxelFlags voxel_flags;

	public override string ToString () {
		return String.Format("VoxelData\nplacable_index {0}\ndirection_built {1}\nsupport_directions {2}\n\nis_special_voxel {3}\nspecial_index {4}\nspecial_directions {5}\nvoxel_flags {6}",
			placable_index,
			direction_built,
			support_directions,
			is_special_voxel,
			special_voxel,
			special_directions,
			voxel_flags
		);
	}
}

[Tool]
[GlobalClass]
public partial class BuildingGrid : StaticBody3D {

	public static Godot.Collections.Array grids = [];

	[Export]
	public bool update_thing = false;

	[Export]
	public Material mesh_material;

	[ExportGroup("Grid Sizes")]
	[Export]
	public int grid_width {
		get;
		set {
			field = (int) Math.Ceiling((float) value / chunk_size) * chunk_size;
		}
	} = 32;
	[Export]
	public int grid_height {
		get;
		set {
			field = (int) Math.Ceiling((float) value / chunk_size) * chunk_size;
		}
	} = 32;
	[Export]
	public int grid_length {
		get;
		set{
			field = (int) Math.Ceiling((float) value / chunk_size) * chunk_size;
		} 
	} = 32;

	[ExportGroup("Chunk Sizes")]
	[Export]
	public int chunk_size = 16;

	static int max_faces = 4096 * 4;

	ArrayMesh mesh = new ArrayMesh();
	Vector3[] vertices = new Vector3[max_faces * 4];
	Int32[] indices = new Int32[max_faces * 6]; 
	Vector2[] uvs = new Vector2[max_faces * 4];

	Vector3[] collision_vertices = new Vector3[max_faces * 6];

	Godot.Collections.Array pre_mesh_data = new Godot.Collections.Array();

	MeshInstance3D mesh_instance;
	CollisionShape3D collision_shape;
	ConcavePolygonShape3D collision_polygon;

	int face_count = 0;
	int voxel_count = 0;

	public BuildingGridChunk[][][] chunk_data = {};

	static int max_placables = 2048;
	// array of references to placables
	public BuildingGridPlacable[] placables = new BuildingGridPlacable[max_placables];
	public Stack<int> placable_indices = new Stack<int>();

	public override void _Ready()
	{
		base._Ready();

		mesh_instance = new MeshInstance3D ();
		mesh_instance.MaterialOverride = mesh_material;
		AddChild(mesh_instance);

		collision_shape = new CollisionShape3D ();
		AddChild(collision_shape);

		collision_polygon = new ConcavePolygonShape3D ();
		collision_shape.Shape = collision_polygon;

		init_placable_indices();

		grids.Add(this);

		init_chunk_array();

		if (!Engine.IsEditorHint()) {
			set_mesh_visibility(false);
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (update_thing) {
			GD.Print("alright lil bro");
			generate_mesh();
			update_thing = false;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}

	public void init_placable_indices () {
		for (int x = max_placables - 1; x >= 0; x--) {
			placable_indices.Push(x);
		}
	}

	public int get_free_index () {
		return placable_indices.Count > 0 ? placable_indices.Pop() : -1;
	}

	public void return_index (int x) {
		placable_indices.Push(x);
	}

	public void set_placable (int index, BuildingGridPlacable placable) {
		placables[index] = placable;
	}

	public void remove_placable (int index) {
		placables[index] = null;
	}

	public static Godot.Collections.Array get_grids () {
		return grids;
	}

	public void set_mesh_visibility (bool value) {
		mesh_instance.Visible = value;
		for (int x = 0; x < grid_width / chunk_size; x++) {
			for (int y = 0; y < grid_height / chunk_size; y++) {
				for (int z = 0; z < grid_length / chunk_size; z++) {
					chunk_data[x][y][z].set_mesh_visibility(value);
				}
			}
		}
	}

	public float floor (float input) {
		if (input < 0) {
			return (float)Math.Floor(input);
		} else {
			return (float)Math.Floor(input);
		}
	}

	public Vector3I position_to_voxel (Vector3 position) {
		position = position - GlobalPosition;
		return new Vector3I((int)floor(position.X), (int)floor(position.Y), (int)floor(position.Z));
	}
	
	public Vector3I position_to_chunk (Vector3 position) {
		position = position - GlobalPosition;
		return new Vector3I((int)floor(position.X / chunk_size), (int)floor(position.Y / chunk_size), (int)floor(position.Z / chunk_size));
	}

	public Vector3I position_to_chunk_voxel (Vector3 position) {
		Vector3I chunk_pos = position_to_chunk(position);
		position = position - GlobalPosition;
		return new Vector3I((int)floor(position.X), (int)floor(position.Y), (int)floor(position.Z)) - (chunk_pos * chunk_size);
	}

	public BuildingGridChunk set_block (int x, int y, int z, VoxelData data) {
		BuildingGridChunk chunk = get_chunk(x, y, z);
		chunk.set_block(x % chunk_size, y % chunk_size, z % chunk_size, data);
		return chunk;
	}

	public BuildingGridChunk set_block (Vector3I pos, VoxelData data) {
		BuildingGridChunk chunk = get_chunk(pos);
		chunk.set_block(pos % chunk_size, data);
		return chunk;
	}

	public VoxelData get_block (int x, int y, int z) {
		return get_chunk(x, y, z).get_block(x % chunk_size, y % chunk_size, z % chunk_size);
	}
	
	public VoxelData get_block (Vector3I pos) {
		return get_chunk(pos).get_block(pos % chunk_size);
	}

	public BuildingGridChunk get_chunk (int x, int y, int z) {
		return chunk_data[x / chunk_size][y / chunk_size][z / chunk_size];
	}

	public BuildingGridChunk get_chunk (Vector3I pos) {
		pos /= chunk_size;
		return chunk_data[pos.X][pos.Y][pos.Z];
	}

	public Array<BuildingGridChunk> set_area (Vector3I from, Vector3I to, VoxelData data) {
		int temp = 0;
		if (from.X > to.X) {
			temp = from.X;
			from.X = to.X;
			to.X = temp;
		}
		if (from.Y > to.Y) {
			temp = from.Y;
			from.Y = to.Y;
			to.Y = temp;
		}
		if (from.Z > to.Z) {
			temp = from.Z;
			from.Z = to.Z;
			to.Z = temp;
		}

		Array<BuildingGridChunk> affected_chunks = new Array<BuildingGridChunk>();

		for (int x = from.X; x <= to.X; x++) {
			for (int y = from.Y; y <= to.Y; y++) {
				for (int z = from.Z; z <= to.Z; z++) {
					if (is_position_valid(x, y, z)) {
						BuildingGridChunk chunk = set_block(x, y, z, data);
						if (!affected_chunks.Contains(chunk)) {
							affected_chunks.Add(chunk);
						}
					}
				}
			}
		}

		return affected_chunks;
	}
	
	public void clear_block (int x, int y, int z) {
		set_block(x, y, z, new VoxelData{placable_index = -1});
	}

	public void clear_area (Vector3 from, Vector3 to) {
		float temp = 0;
		if (from.X > to.X) {
			temp = from.X;
			from.X = to.X;
			to.X = temp;
		}
		if (from.Y > to.Y) {
			temp = from.Y;
			from.Y = to.Y;
			to.Y = temp;
		}
		if (from.Z > to.Z) {
			temp = from.Z;
			from.Z = to.Z;
			to.Z = temp;
		}

		Vector3I start = position_to_voxel(from);
		Vector3I end = position_to_voxel(to);

		for (int x = start.X; x < end.X; x++) {
			for (int y = start.Y; y < end.Y; y++) {
				for (int z = start.Z; z < end.Z; z++) {
					if (is_position_valid(x, y, z)) {
						clear_block(x, y, z);
					}
				}
			}
		}
	}

	public bool is_position_valid (int x, int y, int z) {
		return x < grid_width && x >= 0 && y < grid_height && y >= 0 && z < grid_length && z >= 0;
	}

	public bool is_position_valid (Vector3I pos) {
		return pos.X < grid_width && pos.X >= 0 && pos.Y < grid_height && pos.Y >= 0 && pos.Z < grid_length && pos.Z >= 0;
	}

	public bool is_chunk_valid (int x, int y, int z) {
		return x < grid_width / chunk_size && x >= 0 && y < grid_height / chunk_size && y >= 0 && z < grid_length / chunk_size && z >= 0;
	}

	public Godot.Collections.Array is_area_free (Vector3I from, Vector3I to) {
		Godot.Collections.Array result = new Godot.Collections.Array();
		result.Resize(1);
		result[0] = true;

		//GD.Print("Checking from " + from.ToString() + " to " + to.ToString());

		int temp = 0;
		if (from.X > to.X) {
			temp = from.X;
			from.X = to.X;
			to.X = temp;
		}
		if (from.Y > to.Y) {
			temp = from.Y;
			from.Y = to.Y;
			to.Y = temp;
		}
		if (from.Z > to.Z) {
			temp = from.Z;
			from.Z = to.Z;
			to.Z = temp;
		}

		int checks = 0;

		for (int x = from.X; x <= to.X; x++) {
			for (int y = from.Y; y <= to.Y; y++) {
				for (int z = from.Z; z <= to.Z; z++) {
					checks += 1;
					if (is_position_valid(x, y, z)) {
						if (! is_block_free(x, y, z)) {
							result[0] = false;
							result.Add(new Vector3I(x, y, z));
						}
					} else {
						result[0] = false;
					}
				}
			}
		}

		//GD.Print("Checks: " + checks.ToString());

		return result;
	}

	public bool is_block_free (Vector3 pos) {
		if (pos.Y < 0) {
			return false;
		} else if (pos.X < 0 || pos.Z < 0) {
			return true;
		} else if (pos.X >= grid_width || pos.Y >= grid_height || pos.Z >= grid_length) {
			return true;
		}
		
		return get_block((int)pos.X, (int)pos.Y, (int)pos.Z).placable_index == -1;
	}

	public bool is_block_free (int x, int y, int z) {
		if (y < 0) {
			return false;
		} else if (x < 0 || z < 0) {
			return true;
		} else if (x >= grid_width || y >= grid_height || z >= grid_length) {
			return true;
		}
		
		return get_block(x, y, z).placable_index == -1;
	}

	public bool place (BuildingGridPlacable placable, Vector3I grid_pos, Vector3 normal, int rotation) {

		placable.Position = Tools.v3I_to_v3(grid_pos) + new Vector3(0.5f, 0.5f, 0.5f);
		placable.Rotation = new Vector3(0, 0, 0);

		Tools.up_to_rot(placable, normal);
		placable.Rotate(normal, (float) (rotation * (Math.PI / 2.0f)));

		Vector3I corner_1 = Tools.apply_building_rotations(placable.get_box_from(), normal, rotation) + grid_pos; 
		Vector3I corner_2 = Tools.apply_building_rotations(placable.get_box_to(), normal, rotation) + grid_pos;

		int index = get_free_index();

		if (index == -1) {
			return false;
		}

		AddChild(placable, true);

		GD.Print("placable support dirs " + placable.support_directions.ToString());

		VoxelData data = new VoxelData{
			placable_index = index,
			direction_built = Tools.normal_to_enum(normal), 
			support_directions = placable.support_directions, 
			voxel_flags = SpecialVoxelFlags.None
		};

		Godot.Collections.Array<BuildingGridChunk> affected_chunks = set_area(corner_1, corner_2, data);

		foreach (string name in placable.special_voxels.Keys) {
			SpecialVoxel special_voxel = placable.special_voxels[name];
			Vector3I pos_to_affect = Tools.apply_building_rotations(special_voxel.voxel_position + Tools.v3_to_v3I(placable.grid_offset), normal, rotation) + grid_pos;

			if (is_position_valid(pos_to_affect)) {
				VoxelData special_data = new VoxelData{
					placable_index = index,
					direction_built = data.direction_built,
					support_directions = Tools.apply_building_rotations(special_voxel.support_directions, normal, rotation),

					is_special_voxel = true,
					voxel_flags = special_voxel.voxel_flags,
					special_directions = Tools.apply_building_rotations(special_voxel.flag_directions, normal, rotation),
					special_voxel = special_voxel,
				};

				special_voxel.placed_voxel_data = special_data;
				special_voxel.placed_voxel_pos = pos_to_affect;
				special_voxel.parent_grid = this;

				set_block(pos_to_affect, special_data);
			} else {
				GD.Print("invalid pos??? " + pos_to_affect.ToString());
			}
			
		}

		placable.parent_grid = this;
		set_placable(index, placable);

		placable.set_mesh_visibility(false);

		placable.on_build();

		foreach (BuildingGridChunk chunk in affected_chunks) {
			placable.occupied_chunks.Add(chunk);
			chunk.OnChunkChanged += placable.on_chunk_changed;
			chunk.on_chunk_changed();
		}

		return true;
	}

	public void init_chunk_array () {
		chunk_data = new BuildingGridChunk[grid_width / chunk_size][][];
		for (int x = 0; x < grid_width / chunk_size; x++) {
			chunk_data[x] = new BuildingGridChunk[grid_height / chunk_size][];
			for (int y = 0; y < grid_height / chunk_size; y++) {
				chunk_data[x][y] = new BuildingGridChunk[grid_length / chunk_size];
				for (int z = 0; z < grid_length / chunk_size; z++) {
					BuildingGridChunk new_instance = new BuildingGridChunk();

					new_instance.Position = new Vector3(x * chunk_size, y * chunk_size, z * chunk_size);
					new_instance.chunk_pos = new Vector3I(x, y, z);

					new_instance.Name = String.Format("Chunk {0} {1} {2}", x, y, z);

					new_instance.mesh_material = mesh_material;

					new_instance.CollisionLayer = CollisionLayer;
					
					new_instance.parent_grid = this;
					AddChild(new_instance);

					chunk_data[x][y][z] = new_instance;
				}
			}
		}

		for (int x = 0; x < grid_width / chunk_size; x++) {
			for (int y = 0; y < grid_height / chunk_size; y++) {
				for (int z = 0; z < grid_length / chunk_size; z++) {
					BuildingGridChunk chunk = chunk_data[x][y][z];

					if (is_chunk_valid(x - 1, y, z)) {
						chunk.chunk_left = chunk_data[x - 1][y][z];
					} 

					if (is_chunk_valid(x + 1, y, z)) {
						chunk.chunk_right = chunk_data[x + 1][y][z];
					}

					if (is_chunk_valid(x, y + 1, z)) {
						chunk.chunk_up = chunk_data[x][y + 1][z];
					}

					if (is_chunk_valid(x, y - 1, z)) {
						chunk.chunk_down = chunk_data[x][y - 1][z];
					}

					if (is_chunk_valid(x, y, z - 1)) {
						chunk.chunk_forward = chunk_data[x][y][z - 1];
					}

					if (is_chunk_valid(x, y, z + 1)) {
						chunk.chunk_back = chunk_data[x][y][z + 1];
					}
					
					chunk.generate_mesh();
				}
			}
		}
	}

	public void generate_mesh() {
		for (int x = 0; x < grid_width / chunk_size; x++) {
			for (int y = 0; y < grid_height / chunk_size; y++) {
				for (int z = 0; z < grid_length / chunk_size; z++) {
					chunk_data[x][y][z].generate_mesh();
				}
			}
		}
	}


}

