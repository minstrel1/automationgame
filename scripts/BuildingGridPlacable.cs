using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
[Tool]
public partial class BuildingGridPlacable : Node3D {
	
	[ExportCategory("Grid Properties")]
	[Export]
	public int grid_width {
		get;
		set {
			field = value;
			adjust_box();
		}
	} = 3;
	[Export]
	public int grid_height {
		get;
		set {
			field = value;
			adjust_box();
		}
	} = 3;
	[Export]
	public int grid_length {
		get;
		set{
			field = value;
			adjust_box();
		} 
	} = 3;
	[Export]
	public Vector3 grid_offset {
		get;
		set{
			field = value;
			adjust_box();
		} 
	} = new Vector3(0,0,0);

	public BuildingGrid parent_grid;

	[ExportCategory("Placable Properties")]

	[Export(PropertyHint.Layers2DPhysics)]
	public int PlacableDirections {get; set;} = 1 << 4;
	public BuildDirectionFlags placable_directions = BuildDirectionFlags.Up;

	[Export(PropertyHint.Layers2DPhysics)]
	public int SupportDirections {get; set;} = 0;
	public BuildDirectionFlags support_directions = 0;

	[Export]
	public BuildDirection default_direction = BuildDirection.Up;

	[Export]
	public bool rotate_support = false;
	[Export]
	public bool rotatable = true;

	[Export]
	public Array<SpecialVoxelData> special_voxels {get; set {field = value; on_special_voxel_array_changed();}}

	[ExportToolButton("Add Special Voxel")]
	public Callable add_voxel_button => Callable.From(add_special_voxel);

	private MeshInstance3D visualiser;

	private static int max_visualiser_faces = 512;

	private ArrayMesh visualiser_mesh;
	private Vector3[] vertices = new Vector3[max_visualiser_faces * 4];
	private Int32[] indices = new Int32[max_visualiser_faces * 6]; 
	private Vector2[] uvs = new Vector2[max_visualiser_faces * 4];

	private int face_count = 0;
	private float h_tex_div = 1f / 14;
	private float v_tex_div = 1f / 2;

	private Array<MeshInstance3D> special_visualisers;

	public string packed_scene_path;

	public bool is_built = false;

	public Array<BuildingGridChunk> occupied_chunks;

	public override void _Ready()
	{
		foreach (SpecialVoxelData thing in special_voxels) {
			add_special_voxel(thing);
		}

		GD.Print("placable init");
		visualiser = new MeshInstance3D();
		visualiser.Position = Vector3.Zero;
		make_visualiser_mesh();
		visualiser.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
		StandardMaterial3D material = GD.Load<StandardMaterial3D>("res://building_grid_placable_box.tres");
		material = (StandardMaterial3D) material.Duplicate();
		material.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
		visualiser.SetSurfaceOverrideMaterial(0, material);

		special_visualisers = new Array<MeshInstance3D>();

		occupied_chunks = new Array<BuildingGridChunk>();
		
		AddChild(visualiser);
		adjust_box();
	}

	public void adjust_box () {
		if (visualiser != null) {
			//(visualiser.Mesh as BoxMesh).Size = new Vector3(grid_width, grid_height, grid_length);
			make_visualiser_mesh();
			(visualiser.GetSurfaceOverrideMaterial(0) as StandardMaterial3D).Uv1Scale = new Vector3(1, 1, 1);
			visualiser.Position = Vector3.Zero;
		}

		if (special_voxels != null) {
			foreach (SpecialVoxelData thing in special_voxels) {
				thing.parent_center = grid_offset;
			}
		}
	}

	public void make_visualiser_mesh () {
		ArrayMesh new_mesh = new ArrayMesh();

		vertices = new Vector3[max_visualiser_faces * 4];
		indices = new Int32[max_visualiser_faces * 6]; 
		uvs = new Vector2[max_visualiser_faces * 4];

		face_count = 0;

		make_box(grid_offset, new Vector3(grid_width, grid_height, grid_length));

		foreach (SpecialVoxelData special_voxel in special_voxels) {
			make_box(grid_offset + special_voxel.voxel_position, new Vector3(1, 1, 1), special_voxel.flag_directions, (int) special_voxel.voxel_flags);
			GD.Print((int) special_voxel.flag_directions);
		}

		Godot.Collections.Array array = new Godot.Collections.Array();
		array.Resize((int) Mesh.ArrayType.Max);
		array[(int) Mesh.ArrayType.Vertex] = vertices;
		array[(int) Mesh.ArrayType.Index] = indices;
		array[(int) Mesh.ArrayType.TexUV] = uvs;

		new_mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array);

		visualiser.Mesh = new_mesh;
	}

	private void make_box (Vector3 pos, Vector3 size, BuildDirectionFlags flags = BuildDirectionFlags.Any, int face = 0) {
		Vector3 ltb = (new Vector3(0f, 1f, 0f) - new Vector3(0.5f, 0.5f, 0.5f)) * size;
		Vector3 rtb = (new Vector3(1f, 1f, 0f) - new Vector3(0.5f, 0.5f, 0.5f)) * size;
		Vector3 ltf = (new Vector3(0f, 1f, 1f) - new Vector3(0.5f, 0.5f, 0.5f)) * size;
		Vector3 rtf = (new Vector3(1f, 1f, 1f) - new Vector3(0.5f, 0.5f, 0.5f)) * size;
		Vector3 lbb = (new Vector3(0f, 0f, 0f) - new Vector3(0.5f, 0.5f, 0.5f)) * size;
		Vector3 rbb = (new Vector3(1f, 0f, 0f) - new Vector3(0.5f, 0.5f, 0.5f)) * size;
		Vector3 lbf = (new Vector3(0f, 0f, 1f) - new Vector3(0.5f, 0.5f, 0.5f)) * size;
		Vector3 rbf = (new Vector3(1f, 0f, 1f) - new Vector3(0.5f, 0.5f, 0.5f)) * size;

		if (flags == BuildDirectionFlags.Any || (((int) flags & 2) >> 1) == 1) {
			vertices[face_count * 4 + 0 ] = pos + ltb;
			vertices[face_count * 4 + 1 ] = pos + ltb + new Vector3(0, 0, 0.5f);
			vertices[face_count * 4 + 2 ] = pos + ltb + new Vector3(0, -0.5f, 0.5f);
			vertices[face_count * 4 + 3 ] = pos + ltb + new Vector3(0, -0.5f, 0);

			add_uvs(face * 2);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + ltf + new Vector3(0, 0, -0.5f);
			vertices[face_count * 4 + 1 ] = pos + ltf;
			vertices[face_count * 4 + 2 ] = pos + ltf + new Vector3(0, -0.5f, 0);
			vertices[face_count * 4 + 3 ] = pos + ltf + new Vector3(0, -0.5f, -0.5f);

			add_uvs((face * 2) + 1);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + lbf + new Vector3(0, 0.5f, -0.5f);
			vertices[face_count * 4 + 1 ] = pos + lbf + new Vector3(0, 0.5f, 0);
			vertices[face_count * 4 + 2 ] = pos + lbf;
			vertices[face_count * 4 + 3 ] = pos + lbf + new Vector3(0, 0, -0.5f);

			add_uvs((face * 2) + 1, 1);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + lbb + new Vector3(0, 0.5f, 0);
			vertices[face_count * 4 + 1 ] = pos + lbb + new Vector3(0, 0.5f, 0.5f);
			vertices[face_count * 4 + 2 ] = pos + lbb + new Vector3(0, 0, 0.5f);
			vertices[face_count * 4 + 3 ] = pos + lbb;

			add_uvs(face * 2, 1);
			add_tris();
		}

		if (flags == BuildDirectionFlags.Any || (((int) flags & 4) >> 2) == 1) {
			vertices[face_count * 4 + 0 ] = pos + rtf;
			vertices[face_count * 4 + 1 ] = pos + rtf + new Vector3(0, 0, -0.5f);
			vertices[face_count * 4 + 2 ] = pos + rtf + new Vector3(0, -0.5f, -0.5f);
			vertices[face_count * 4 + 3 ] = pos + rtf + new Vector3(0, -0.5f, 0);

			add_uvs(face * 2);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + rtb + new Vector3(0, 0, 0.5f);
			vertices[face_count * 4 + 1 ] = pos + rtb;
			vertices[face_count * 4 + 2 ] = pos + rtb + new Vector3(0, -0.5f, 0);
			vertices[face_count * 4 + 3 ] = pos + rtb + new Vector3(0, -0.5f, 0.5f);

			add_uvs((face * 2) + 1);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + rbb + new Vector3(0, 0.5f, 0.5f);
			vertices[face_count * 4 + 1 ] = pos + rbb + new Vector3(0, 0.5f, 0);
			vertices[face_count * 4 + 2 ] = pos + rbb;
			vertices[face_count * 4 + 3 ] = pos + rbb + new Vector3(0, 0, 0.5f);

			add_uvs((face * 2) + 1, 1);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + rbf + new Vector3(0, 0.5f, 0);
			vertices[face_count * 4 + 1 ] = pos + rbf + new Vector3(0, 0.5f, -0.5f);
			vertices[face_count * 4 + 2 ] = pos + rbf + new Vector3(0, 0, -0.5f);
			vertices[face_count * 4 + 3 ] = pos + rbf;

			add_uvs(face * 2, 1);
			add_tris();
		}

		if (flags == BuildDirectionFlags.Any || (((int) flags & 8) >> 3) == 1) {
			vertices[face_count * 4 + 0 ] = pos + ltb;
			vertices[face_count * 4 + 1 ] = pos + ltb + new Vector3(0.5f, 0, 0);
			vertices[face_count * 4 + 2 ] = pos + ltb + new Vector3(0.5f, 0, 0.5f);
			vertices[face_count * 4 + 3 ] = pos + ltb + new Vector3(0, 0, 0.5f);

			add_uvs(face * 2);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + rtb + new Vector3(-0.5f, 0, 0);
			vertices[face_count * 4 + 1 ] = pos + rtb;
			vertices[face_count * 4 + 2 ] = pos + rtb + new Vector3(0, 0, 0.5f);
			vertices[face_count * 4 + 3 ] = pos + rtb + new Vector3(-0.5f, 0, 0.5f);

			add_uvs((face * 2) + 1);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + rtf + new Vector3(-0.5f, 0, -0.5f);
			vertices[face_count * 4 + 1 ] = pos + rtf + new Vector3(0, 0, -0.5f);
			vertices[face_count * 4 + 2 ] = pos + rtf;
			vertices[face_count * 4 + 3 ] = pos + rtf + new Vector3(-0.5f, 0, 0);

			add_uvs((face * 2) + 1, 1);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + ltf + new Vector3(0, 0, -0.5f);
			vertices[face_count * 4 + 1 ] = pos + ltf + new Vector3(0.5f, 0, -0.5f);
			vertices[face_count * 4 + 2 ] = pos + ltf + new Vector3(0.5f, 0, 0);
			vertices[face_count * 4 + 3 ] = pos + ltf;

			add_uvs(face * 2, 1);
			add_tris();
		}

		if (flags == BuildDirectionFlags.Any || (((int) flags & 16) >> 4) == 1) {
			vertices[face_count * 4 + 0 ] = pos + lbf;
			vertices[face_count * 4 + 1 ] = pos + lbf + new Vector3(0.5f, 0, 0);
			vertices[face_count * 4 + 2 ] = pos + lbf + new Vector3(0.5f, 0, -0.5f);
			vertices[face_count * 4 + 3 ] = pos + lbf + new Vector3(0, 0, -0.5f);

			add_uvs(face * 2);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + rbf + new Vector3(-0.5f, 0, 0);
			vertices[face_count * 4 + 1 ] = pos + rbf;
			vertices[face_count * 4 + 2 ] = pos + rbf + new Vector3(0, 0, -0.5f);
			vertices[face_count * 4 + 3 ] = pos + rbf + new Vector3(-0.5f, 0, -0.5f);

			add_uvs((face * 2) + 1);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + rbb + new Vector3(-0.5f, 0, 0.5f);
			vertices[face_count * 4 + 1 ] = pos + rbb + new Vector3(0, 0, 0.5f);
			vertices[face_count * 4 + 2 ] = pos + rbb;
			vertices[face_count * 4 + 3 ] = pos + rbb + new Vector3(-0.5f, 0, 0);

			add_uvs((face * 2) + 1, 1);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + lbb + new Vector3(0, 0, 0.5f);
			vertices[face_count * 4 + 1 ] = pos + lbb + new Vector3(0.5f, 0, 0.5f);
			vertices[face_count * 4 + 2 ] = pos + lbb + new Vector3(0.5f, 0, 0);
			vertices[face_count * 4 + 3 ] = pos + lbb;

			add_uvs(face * 2, 1);
			add_tris();
		}

		if (flags == BuildDirectionFlags.Any || (((int) flags & 32) >> 5) == 1) {
			vertices[face_count * 4 + 0 ] = pos + rtb;
			vertices[face_count * 4 + 1 ] = pos + rtb + new Vector3(-0.5f, 0, 0);
			vertices[face_count * 4 + 2 ] = pos + rtb + new Vector3(-0.5f, -0.5f, 0);
			vertices[face_count * 4 + 3 ] = pos + rtb + new Vector3(0, -0.5f, 0);

			add_uvs(face * 2);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + ltb + new Vector3(0.5f, 0, 0);
			vertices[face_count * 4 + 1 ] = pos + ltb;
			vertices[face_count * 4 + 2 ] = pos + ltb + new Vector3(0, -0.5f, 0);
			vertices[face_count * 4 + 3 ] = pos + ltb + new Vector3(0.5f, -0.5f, 0);

			add_uvs((face * 2) + 1);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + lbb + new Vector3(0.5f, 0.5f, 0);
			vertices[face_count * 4 + 1 ] = pos + lbb + new Vector3(0, 0.5f, 0);
			vertices[face_count * 4 + 2 ] = pos + lbb;
			vertices[face_count * 4 + 3 ] = pos + lbb + new Vector3(0.5f, 0, 0);

			add_uvs((face * 2) + 1, 1);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + rbb + new Vector3(0, 0.5f, 0);
			vertices[face_count * 4 + 1 ] = pos + rbb + new Vector3(-0.5f, 0.5f, 0);
			vertices[face_count * 4 + 2 ] = pos + rbb + new Vector3(-0.5f, 0, 0);
			vertices[face_count * 4 + 3 ] = pos + rbb;

			add_uvs(face * 2, 1);
			add_tris();
		}

		if (flags == BuildDirectionFlags.Any || (((int) flags & 64) >> 6) == 1) {
			vertices[face_count * 4 + 0 ] = pos + ltf;
			vertices[face_count * 4 + 1 ] = pos + ltf + new Vector3(0.5f, 0, 0);
			vertices[face_count * 4 + 2 ] = pos + ltf + new Vector3(0.5f, -0.5f, 0);
			vertices[face_count * 4 + 3 ] = pos + ltf + new Vector3(0, -0.5f, 0);

			add_uvs(face * 2);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + rtf + new Vector3(-0.5f, 0, 0);
			vertices[face_count * 4 + 1 ] = pos + rtf;
			vertices[face_count * 4 + 2 ] = pos + rtf + new Vector3(0, -0.5f, 0);
			vertices[face_count * 4 + 3 ] = pos + rtf + new Vector3(-0.5f, -0.5f, 0);

			add_uvs((face * 2) + 1);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + rbf + new Vector3(-0.5f, 0.5f, 0);
			vertices[face_count * 4 + 1 ] = pos + rbf + new Vector3(0, 0.5f, 0);
			vertices[face_count * 4 + 2 ] = pos + rbf;
			vertices[face_count * 4 + 3 ] = pos + rbf + new Vector3(-0.5f, 0, 0);

			add_uvs((face * 2) + 1, 1);
			add_tris();

			vertices[face_count * 4 + 0 ] = pos + lbf + new Vector3(0, 0.5f, 0);
			vertices[face_count * 4 + 1 ] = pos + lbf + new Vector3(0.5f, 0.5f, 0);
			vertices[face_count * 4 + 2 ] = pos + lbf + new Vector3(0.5f, 0, 0);
			vertices[face_count * 4 + 3 ] = pos + lbf;

			add_uvs(face * 2, 1);
			add_tris();
		}
	}

	public void add_uvs (int h_index = 0, int v_index = 0) {
		uvs[face_count * 4 + 0] = new Vector2(h_tex_div * h_index, v_tex_div * v_index);
		uvs[face_count * 4 + 1] = new Vector2(h_tex_div * h_index + h_tex_div, v_tex_div * v_index);
		uvs[face_count * 4 + 2] = new Vector2(h_tex_div * h_index + h_tex_div, v_tex_div * v_index + v_tex_div);
		uvs[face_count * 4 + 3] = new Vector2(h_tex_div * h_index, v_tex_div * v_index + v_tex_div);
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

	public void add_special_voxel () {
		add_special_voxel(null);
	}

	public void add_special_voxel (SpecialVoxelData instance) {

		SpecialVoxelData new_instance = instance;
		if (instance == null) {
			new_instance = new SpecialVoxelData();
			special_voxels.Add(new_instance);
		}

		new_instance.parent = this;

		NotifyPropertyListChanged();
	}

	public void on_special_voxel_array_changed () {
		if (visualiser != null) {
			make_visualiser_mesh();
		}
	}

	public void set_mesh_visibility (bool value) {
		visualiser.Visible = value;
	}

	public void clear_special_visualisers () {
		foreach (MeshInstance3D mesh in special_visualisers) {
			mesh.QueueFree();
		}

		special_visualisers.Clear();
	}

	public Vector3I get_box_from () {
		int x = -(int)Math.Floor(grid_width / 2.0);
		int y = 0;
		int z = -(int)Math.Floor(grid_length / 2.0);
		if (grid_width % 2 == 0) {
			x += 1;
		}
		if (grid_length % 2 == 0) {
			z += 1;
		}
		return new Vector3I(x, y, z);
	}

	public Vector3I get_box_to () {
		int x = (int)Math.Floor(grid_width / 2.0);
		int y = grid_height - 1;
		int z = (int)Math.Floor(grid_length / 2.0);
		return new Vector3I(x, y, z);
	}

	public virtual void on_build () {
		is_built = true;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}

	public virtual void set_collision (bool value) {
		
	}

	public virtual void on_chunk_changed (BuildingGridChunk chunk) {
		GD.Print(chunk + " changed");
	}

}