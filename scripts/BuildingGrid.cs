using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

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
	public int grid_width = 32;
	[Export]
	public int grid_height = 32;
	[Export]
	public int grid_length = 32;

	static int max_faces = 4092 * 4;

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
	float tex_div = 1f;
	int voxel_count = 0;

	public int[][][] voxel_data = {};

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

		init_voxel_array();
		generate_mesh();

		if (!Engine.IsEditorHint()) {
			mesh_instance.Visible = false;
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

	public void set_block (int x, int y, int z, int value) {
		voxel_data[x][y][z] = value;
	}

	public void set_block (Vector3I pos, int value) {
		voxel_data[pos.X][pos.Y][pos.Z] = value;
	}

	public int get_block (int x, int y, int z) {
		return voxel_data[x][y][z];
	}
	
	public int get_block (Vector3I pos) {
		return voxel_data[pos.X][pos.Y][pos.Z];
	}

	public void set_area (Vector3I from, Vector3I to, int value) {
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

		for (int x = from.X; x <= to.X; x++) {
			for (int y = from.Y; y <= to.Y; y++) {
				for (int z = from.Z; z <= to.Z; z++) {
					if (is_position_valid(x, y, z)) {
						set_block(x, y, z, value);
					}
				}
			}
		}
	}
	
	public void clear_block (int x, int y, int z) {
		voxel_data[x][y][z] = 0;
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

	public Godot.Collections.Array is_area_free (Vector3I from, Vector3I to) {
		Godot.Collections.Array result = new Godot.Collections.Array();
		result.Resize(1024);
		result[0] = true;

		GD.Print("Checking from " + from.ToString() + " to " + to.ToString());

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
							result.Append(new Vector3I(x, y, z));
						}
					} else {
						result[0] = false;
					}
				}
			}
		}

		GD.Print("Checks: " + checks.ToString());

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
		
		return voxel_data[(int)pos.X][(int)pos.Y][(int)pos.Z] == -1;
	}

	public bool is_block_free (int x, int y, int z) {
		if (y < 0) {
			return false;
		} else if (x < 0 || z < 0) {
			return true;
		} else if (x >= grid_width || y >= grid_height || z >= grid_length) {
			return true;
		}
		
		return voxel_data[x][y][z] == -1;
	}

	public void init_voxel_array () {
		voxel_data = new int[grid_width][][];
		for (int x = 0; x < grid_width; x++) {
			voxel_data[x] = new int[grid_height][];
			for (int y = 0; y < grid_height; y++) {
				voxel_data[x][y] = new int[grid_length];
				for (int z = 0; z < grid_length; z++) {
					voxel_data[x][y][z] = -1;
				}
			}
		}
	}

	public void add_uvs (int x, int y) {
		uvs[face_count * 4 + 0] = new Vector2(tex_div * x, tex_div * y);
		uvs[face_count * 4 + 1] = new Vector2(tex_div * x + tex_div, tex_div * y);
		uvs[face_count * 4 + 2] = new Vector2(tex_div * x + tex_div, tex_div * y + tex_div);
		uvs[face_count * 4 + 3] = new Vector2(tex_div * x, tex_div * y + tex_div);
	}

	public void add_tris () {
		indices[face_count * 6 + 0] = face_count * 4 + 0;
		indices[face_count * 6 + 1] = face_count * 4 + 1;
		indices[face_count * 6 + 2] = face_count * 4 + 2;
		indices[face_count * 6 + 3] = face_count * 4 + 0;
		indices[face_count * 6 + 4] = face_count * 4 + 2;
		indices[face_count * 6 + 5] = face_count * 4 + 3;

		face_count += 1;
	}

	public void add_collision_tris () {
		collision_vertices[face_count * 6 + 0] = vertices[face_count * 4 + 0];
		collision_vertices[face_count * 6 + 1] = vertices[face_count * 4 + 1];
		collision_vertices[face_count * 6 + 2] = vertices[face_count * 4 + 2];
		collision_vertices[face_count * 6 + 3] = vertices[face_count * 4 + 0];
		collision_vertices[face_count * 6 + 4] = vertices[face_count * 4 + 2];
		collision_vertices[face_count * 6 + 5] = vertices[face_count * 4 + 3];
	}

	// variable names are left / right, top / bottom, back / forward
	private static Vector3 ltb = new Vector3(0f, 1f, 0f);
	private static Vector3 rtb = new Vector3(1f, 1f, 0f);
	private static Vector3 ltf = new Vector3(0f, 1f, 1f);
	private static Vector3 rtf = new Vector3(1f, 1f, 1f);
	private static Vector3 lbb = new Vector3(0f, 0f, 0f);
	private static Vector3 rbb = new Vector3(1f, 0f, 0f);
	private static Vector3 lbf = new Vector3(0f, 0f, 1f);
	private static Vector3 rbf = new Vector3(1f, 0f, 1f);
	public void add_cube (Vector3 pos) {
		if (is_block_free(pos + Vector3.Up)) {
			vertices[face_count * 4 + 0] = pos + ltb;
			vertices[face_count * 4 + 1] = pos + rtb;
			vertices[face_count * 4 + 2] = pos + rtf;
			vertices[face_count * 4 + 3] = pos + ltf;

			add_uvs(0, 0);
			add_collision_tris();
			add_tris();
		}

		if (is_block_free(pos + Vector3.Down)) {
			vertices[face_count * 4 + 0] = pos + lbf;
			vertices[face_count * 4 + 1] = pos + rbf;
			vertices[face_count * 4 + 2] = pos + rbb;
			vertices[face_count * 4 + 3] = pos + lbb;

			add_uvs(0, 0);
			add_collision_tris();
			add_tris();
		}

		if (is_block_free(pos + Vector3.Right)) {
			vertices[face_count * 4 + 0] = pos + rtf;
			vertices[face_count * 4 + 1] = pos + rtb;
			vertices[face_count * 4 + 2] = pos + rbb;
			vertices[face_count * 4 + 3] = pos + rbf;

			add_uvs(0, 0);
			add_collision_tris();
			add_tris();
		}

		if (is_block_free(pos + Vector3.Left)) {
			vertices[face_count * 4 + 0] = pos + ltb;
			vertices[face_count * 4 + 1] = pos + ltf;
			vertices[face_count * 4 + 2] = pos + lbf;
			vertices[face_count * 4 + 3] = pos + lbb;

			add_uvs(0, 0);
			add_collision_tris();
			add_tris();
		}

		if (is_block_free(pos + Vector3.Back)) {
			vertices[face_count * 4 + 0] = pos + ltf;
			vertices[face_count * 4 + 1] = pos + rtf;
			vertices[face_count * 4 + 2] = pos + rbf;
			vertices[face_count * 4 + 3] = pos + lbf;

			add_uvs(0, 0);
			add_collision_tris();
			add_tris();
		}

		if (is_block_free(pos + Vector3.Forward)) {
			vertices[face_count * 4 + 0] = pos + rtb;
			vertices[face_count * 4 + 1] = pos + ltb;
			vertices[face_count * 4 + 2] = pos + lbb;
			vertices[face_count * 4 + 3] = pos + rbb;

			add_uvs(0, 0);
			add_collision_tris();
			add_tris();
		}
	}

	public void make_floor () {
		vertices[face_count * 4 + 0] = new Vector3(  0f,   0f,  0f);
		vertices[face_count * 4 + 1] = new Vector3(  0f + grid_width,   0f,  0f);
		vertices[face_count * 4 + 2] = new Vector3(  0f + grid_width,   0f,  0f + grid_length);
		vertices[face_count * 4 + 3] = new Vector3(  0f,   0f,  0f + grid_length);
		
		uvs[face_count * 4 + 0] = new Vector2(0, 0);
		uvs[face_count * 4 + 1] = new Vector2(tex_div * grid_width, 0);
		uvs[face_count * 4 + 2] = new Vector2(tex_div * grid_width, tex_div * grid_length);
		uvs[face_count * 4 + 3] = new Vector2(0, tex_div * grid_length);

		add_collision_tris();
		add_tris();
	}

	// TODO: Add greedy meshing / a method to reduce amount of triangles sent 

	// TODO: Divide building grid into chunks to reduce amount of voxels needing to be modified every time

	public void generate_mesh () {
		ulong total_start = Time.GetTicksUsec();

		mesh = new ArrayMesh();
		//mesh.ClearSurfaces();
		vertices = new Vector3[max_faces * 4];
		indices = new Int32[max_faces * 6]; 
		uvs = new Vector2[max_faces * 4];

		face_count = 0;
		voxel_count = 0;

		// for (int x = 0; x < grid_width; x++) {
		// 	for (int z = 0; z < grid_width; z++) {
		// 		add_cube(new Vector3(x, -1, z));
		// 	}
		// }
		make_floor();

		ulong start = Time.GetTicksUsec();

		for (int x = 0; x < grid_width; x++) {
			for (int y = 0; y < grid_height; y++) {
				for (int z = 0; z < grid_length; z++) {
					if (voxel_data[x][y][z] >= 0) {
						add_cube(new Vector3(x, y, z));
						voxel_count += 1;
					}
				}
			}
		}
		ulong time = Time.GetTicksUsec() - start;
		GD.Print("C# ARRAY TIME:" + time.ToString());

		
		Godot.Collections.Array array = new Godot.Collections.Array();
		array.Resize((int) Mesh.ArrayType.Max);
		array[(int) Mesh.ArrayType.Vertex] = vertices;
		array[(int) Mesh.ArrayType.Index] = indices;
		array[(int) Mesh.ArrayType.TexUV] = uvs;

		start = Time.GetTicksUsec();
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array);
		time = Time.GetTicksUsec() - start;
		GD.Print("C# MESH TIME:" + time.ToString());

		start = Time.GetTicksUsec();
		collision_polygon.SetFaces(collision_vertices);
		time = Time.GetTicksUsec() - start;
		GD.Print("C# COLLISION TIME:" + time.ToString());

		start = Time.GetTicksUsec();
		//collision_shape.Shape = new_collision_shape;
		time = Time.GetTicksUsec() - start;
		GD.Print("C# COLLISION SET TIME:" + time.ToString());

		
		

		mesh_instance.Mesh = mesh;

		GD.Print("C# FACE COUNT: " + face_count.ToString() + " / " + max_faces.ToString());
		GD.Print("C# VOXEL COUNT: " + voxel_count.ToString());

		time = Time.GetTicksUsec() - total_start;
		GD.Print("C# TOTAL MESH TIME:" + time.ToString());
		
	}
}