using System;
using Godot;


[GlobalClass]
[Tool]
public partial class BuildingGridChunk : StaticBody3D {
	public Material mesh_material;
	public int chunk_size = 16;
	static int max_faces = 4096;

	ArrayMesh mesh = new ArrayMesh();
	Vector3[] vertices = new Vector3[max_faces * 4];
	Int32[] indices = new Int32[max_faces * 6]; 
	Vector2[] uvs = new Vector2[max_faces * 4];

	Vector3[] collision_vertices = new Vector3[max_faces * 6];

	Godot.Collections.Array pre_mesh_data = new Godot.Collections.Array();

	MeshInstance3D mesh_instance;
	CollisionShape3D collision_shape;
	ConcavePolygonShape3D collision_polygon;

	public BuildingGrid parent_grid;

	int face_count = 0;
	int voxel_count = 0;

	float tex_div = 1f / 8;

	public Vector3I chunk_pos;

	public BuildingGridChunk chunk_left;
	public BuildingGridChunk chunk_right;
	public BuildingGridChunk chunk_up;
	public BuildingGridChunk chunk_down;
	public BuildingGridChunk chunk_forward;
	public BuildingGridChunk chunk_back;

	public VoxelData[][][] voxel_data = {};

	public bool mesh_gen_this_frame = false;

	[Signal]
	public delegate void OnChunkChangedEventHandler (BuildingGridChunk chunk);

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

		init_voxel_array();
		generate_mesh();

		if (!Engine.IsEditorHint()) {
			//mesh_instance.Visible = false;
		}
	}

	public override string ToString()
	{
		return Name;
	}

	public override void _PhysicsProcess(double delta)
	{
		mesh_gen_this_frame = false;
	}

	public BuildingGrid get_grid () {
		return parent_grid;
	}

	public VoxelData get_block (int x, int y, int z) {
		return voxel_data[x][y][z];
	}
	
	public VoxelData get_block (Vector3I pos) {
		return voxel_data[pos.X][pos.Y][pos.Z];
	}

	public void set_block (int x, int y, int z, VoxelData data) {
		voxel_data[x][y][z] = data;
		
	}

	public void set_block (Vector3I pos, VoxelData data) {
		voxel_data[pos.X][pos.Y][pos.Z] = data;
	}

	public void set_mesh_visibility (bool value) {
		mesh_instance.Visible = value;
	}


	public void init_voxel_array () {
		voxel_data = new VoxelData[chunk_size][][];
		for (int x = 0; x < chunk_size; x++) {
			voxel_data[x] = new VoxelData[chunk_size][];
			for (int y = 0; y < chunk_size; y++) {
				voxel_data[x][y] = new VoxelData[chunk_size];
				for (int z = 0; z < chunk_size; z++) {
					voxel_data[x][y][z] = new VoxelData{placable_index = -1};
				}
			}
		}
	}

	public void on_chunk_changed () {
		
		generate_mesh();
		generate_mesh_neighbors();

		emit();
		emit_neighbors();

	}

	public void emit () {
		EmitSignal(BuildingGridChunk.SignalName.OnChunkChanged, this);
	}

	public void emit_neighbors () {
		if (chunk_left != null) {
			chunk_left.emit();
		}

		if (chunk_right != null) {
			chunk_right.emit();
		}

		if (chunk_up != null) {
			chunk_up.emit();
		}

		if (chunk_down != null) {
			chunk_down.emit();
		}

		if (chunk_forward != null) {
			chunk_forward.emit();
		}

		if (chunk_back != null) {
			chunk_back.emit();
		}
	}
	
	public bool is_block_free (Vector3 pos) {

		if (pos.X < 0) {
			if (chunk_left != null) {
				return chunk_left.is_block_free(pos + Vector3.Right * chunk_size);
			} else {
				return true;
			}
		}

		if (pos.Y < 0) {
			if (chunk_down != null) {
				return chunk_down.is_block_free(pos + Vector3.Up * chunk_size);
			} else {
				return true;
			}
		}

		if (pos.Z < 0) {
			if (chunk_forward != null) {
				return chunk_forward.is_block_free(pos + Vector3.Back * chunk_size);
			} else {
				return true;
			}
		}

		if (pos.X >= chunk_size) {
			if (chunk_right != null) {
				return chunk_right.is_block_free(pos + Vector3.Left * chunk_size);
			} else {
				return true;
			}
		}

		if (pos.Y >= chunk_size) {
			if (chunk_up != null) {
				return chunk_up.is_block_free(pos + Vector3.Down * chunk_size);
			} else {
				return true;
			}
		}

		if (pos.Z >= chunk_size) {
			if (chunk_back != null) {
				return chunk_back.is_block_free(pos + Vector3.Forward * chunk_size);
			} else {
				return true;
			}
		}
		
		return voxel_data[(int)pos.X][(int)pos.Y][(int)pos.Z].placable_index == -1;
	}

	public void add_uvs (int index = 0) {
		uvs[face_count * 4 + 0] = new Vector2(tex_div * index, 0);
		uvs[face_count * 4 + 1] = new Vector2(tex_div * index + tex_div, 0);
		uvs[face_count * 4 + 2] = new Vector2(tex_div * index + tex_div, tex_div);
		uvs[face_count * 4 + 3] = new Vector2(tex_div * index, tex_div);
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

	public void add_cube (Vector3 pos, BuildDirectionFlags flags) {
		if (is_block_free(pos + Vector3.Left)) {
			vertices[face_count * 4 + 0] = pos + ltb;
			vertices[face_count * 4 + 1] = pos + ltf;
			vertices[face_count * 4 + 2] = pos + lbf;
			vertices[face_count * 4 + 3] = pos + lbb;

			if (flags == BuildDirectionFlags.Any || (((int) flags & 2) >> 1) == 1) {

			}

			add_uvs();
			add_collision_tris();
			add_tris();
		}

		if (is_block_free(pos + Vector3.Right)) {
			vertices[face_count * 4 + 0] = pos + rtf;
			vertices[face_count * 4 + 1] = pos + rtb;
			vertices[face_count * 4 + 2] = pos + rbb;
			vertices[face_count * 4 + 3] = pos + rbf;

			if (flags == BuildDirectionFlags.Any || (((int) flags & 4) >> 2) == 1) {

			}

			add_uvs();
			add_collision_tris();
			add_tris();
		}

		if (is_block_free(pos + Vector3.Up)) {
			vertices[face_count * 4 + 0] = pos + ltb;
			vertices[face_count * 4 + 1] = pos + rtb;
			vertices[face_count * 4 + 2] = pos + rtf;
			vertices[face_count * 4 + 3] = pos + ltf;

			if (flags == BuildDirectionFlags.Any || (((int) flags & 8) >> 3) == 1) {
				
			}

			add_uvs();
			add_collision_tris();
			add_tris();
		}

		if (is_block_free(pos + Vector3.Down)) {
			vertices[face_count * 4 + 0] = pos + lbf;
			vertices[face_count * 4 + 1] = pos + rbf;
			vertices[face_count * 4 + 2] = pos + rbb;
			vertices[face_count * 4 + 3] = pos + lbb;

			if (flags == BuildDirectionFlags.Any || (((int) flags & 16) >> 4) == 1) {

			}

			add_uvs();
			add_collision_tris();
			add_tris();
		}

		if (is_block_free(pos + Vector3.Forward) ) {
			vertices[face_count * 4 + 0] = pos + rtb;
			vertices[face_count * 4 + 1] = pos + ltb;
			vertices[face_count * 4 + 2] = pos + lbb;
			vertices[face_count * 4 + 3] = pos + rbb;

			if (flags == BuildDirectionFlags.Any || (((int) flags & 32) >> 5) == 1) {
				
			}

			add_uvs();
			add_collision_tris();
			add_tris();
		}

		if (is_block_free(pos + Vector3.Back)) {
			vertices[face_count * 4 + 0] = pos + ltf;
			vertices[face_count * 4 + 1] = pos + rtf;
			vertices[face_count * 4 + 2] = pos + rbf;
			vertices[face_count * 4 + 3] = pos + lbf;

			if (flags == BuildDirectionFlags.Any || (((int) flags & 64) >> 6) == 1) {
				
			}

			add_uvs();
			add_collision_tris();
			add_tris();
		}
	}

	public void make_floor () {
		for (int x = 0; x < chunk_size; x++) {
			for (int z = 0; z < chunk_size; z++) {
				if (is_block_free(new Vector3(x, 0, z))) {
					vertices[face_count * 4 + 0] = new Vector3(  x,   0f,  z);
					vertices[face_count * 4 + 1] = new Vector3(  x + 1,   0f,  z);
					vertices[face_count * 4 + 2] = new Vector3(  x + 1,   0f,  z + 1);
					vertices[face_count * 4 + 3] = new Vector3(  x,   0f,  z + 1);
					
					add_uvs();
					add_collision_tris();
					add_tris();
				}
			}
		}
	}

	// TODO: Add greedy meshing / a method to reduce amount of triangles sent 

	// TODO: Divide building grid into chunks to reduce amount of voxels needing to be modified every time

	public void generate_mesh_neighbors () {
		if (chunk_left != null) {
			chunk_left.generate_mesh();
		}

		if (chunk_right != null) {
			chunk_right.generate_mesh();
		}

		if (chunk_up != null) {
			chunk_up.generate_mesh();
		}

		if (chunk_down != null) {
			chunk_down.generate_mesh();
		}

		if (chunk_forward != null) {
			chunk_forward.generate_mesh();
		}

		if (chunk_back != null) {
			chunk_back.generate_mesh();
		}
	}

	public void generate_mesh () {

		if (mesh_gen_this_frame) {
			return;
		} else {
			mesh_gen_this_frame = true;
		}

		ulong total_start = Time.GetTicksUsec();

		mesh = new ArrayMesh();
		//mesh.ClearSurfaces();
		vertices = new Vector3[max_faces * 4];
		indices = new Int32[max_faces * 6]; 
		uvs = new Vector2[max_faces * 4];

		face_count = 0;
		voxel_count = 0;

		if (chunk_pos.Y == 0) {
			make_floor();
		}

		ulong start = Time.GetTicksUsec();

		for (int x = 0; x < chunk_size; x++) {
			for (int y = 0; y < chunk_size; y++) {
				for (int z = 0; z < chunk_size; z++) {
					if (voxel_data[x][y][z].placable_index >= 0) {
						add_cube(new Vector3(x, y, z), voxel_data[x][y][z].support_directions);
						voxel_count += 1;
					}
				}
			}
		}
		ulong time = Time.GetTicksUsec() - start;
		//GD.Print("C# ARRAY TIME:" + time.ToString());

		
		Godot.Collections.Array array = new Godot.Collections.Array();
		array.Resize((int) Mesh.ArrayType.Max);
		array[(int) Mesh.ArrayType.Vertex] = vertices;
		array[(int) Mesh.ArrayType.Index] = indices;
		array[(int) Mesh.ArrayType.TexUV] = uvs;

		start = Time.GetTicksUsec();
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array);
		time = Time.GetTicksUsec() - start;
		//GD.Print("C# MESH TIME:" + time.ToString());

		start = Time.GetTicksUsec();
		if (collision_vertices[1] != Vector3.Zero) {
			collision_polygon.SetFaces(collision_vertices);
		} else {
			//GD.Print("skipping collision for " + Name);
		}
		
		time = Time.GetTicksUsec() - start;
		//GD.Print("C# COLLISION TIME:" + time.ToString());

		start = Time.GetTicksUsec();
		//collision_shape.Shape = new_collision_shape;
		time = Time.GetTicksUsec() - start;
		//GD.Print("C# COLLISION SET TIME:" + time.ToString());

		
		

		mesh_instance.Mesh = mesh;

		//GD.Print("C# FACE COUNT: " + face_count.ToString() + " / " + max_faces.ToString());
		//GD.Print("C# VOXEL COUNT: " + voxel_count.ToString());

		time = Time.GetTicksUsec() - total_start;
		//GD.Print("C# TOTAL MESH TIME:" + time.ToString());
		
	}

}