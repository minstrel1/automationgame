using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
	
	private MeshInstance3D visualiser;
	public BuildingGrid parent_grid;

	public override void _Ready()
	{
		if (Godot.Engine.IsEditorHint()) {
			visualiser = new MeshInstance3D();
			visualiser.Mesh = new BoxMesh();
			StandardMaterial3D material = GD.Load<StandardMaterial3D>("res://selection_box_texture.tres");
			visualiser.SetSurfaceOverrideMaterial(0, material);
			AddChild(visualiser);
			adjust_box();
		}
	}

	public void adjust_box () {
		if (visualiser != null) {
			(visualiser.Mesh as BoxMesh).Size = new Vector3(grid_width, grid_height, grid_length);
			(visualiser.GetSurfaceOverrideMaterial(0) as StandardMaterial3D).Uv1Scale = new Vector3(3 * grid_width, 2 * grid_height, 1 * grid_length);
			visualiser.GlobalPosition = grid_offset;
		}
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

	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}

}