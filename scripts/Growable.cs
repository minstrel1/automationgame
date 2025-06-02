using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
public partial class Growable : Node3D {
	
	public string current_plant_name = "";
	public GrowablePrototype prototype;
	public int growth_time = 0;
	public int current_growth_time = 0;
	public bool plant_set = false;
	public bool done_growing = false;

	public bool waiting_on_insert = false;
	public Array<InventoryItem> to_insert = new Array<InventoryItem>();
	public bool try_insert = true;

	MeshInstance3D model;
	SphereMesh sphere_mesh;

	public override void _Ready()
	{
		model = new MeshInstance3D();
		sphere_mesh = new SphereMesh();
		sphere_mesh.Radius = 0.5f;
		sphere_mesh.Height = 1f;
		model.Mesh = sphere_mesh;
		model.Position = Vector3.Zero;
		model.Scale = Vector3.One * 0.0000001f;
		model.Visible = false;
		model.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;
		AddChild(model);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		//ulong start = Time.GetTicksUsec();
		
		if (current_growth_time < growth_time && !done_growing) {
			current_growth_time += 1;
			float scale = (float) current_growth_time / growth_time;
			model.Scale = Vector3.One * scale;

			if (current_growth_time == growth_time) {
				done_growing = true;
			}
		}

		//ulong time = Time.GetTicksUsec() - start;
		//GD.Print("GROWABLE" + Name + "PHYSICS PROCESS TIME:" + time.ToString());
	}

	public void set_plant (String new_plant_name) {
		if (Prototypes.growables.ContainsKey(new_plant_name)) {
			prototype = Prototypes.growables[new_plant_name];
			current_plant_name = new_plant_name;
			current_growth_time = 0;
			growth_time = Globals.ighours_to_ticks((float) prototype.time_to_grow);
			plant_set = true;
			done_growing = false;

			model.Visible = true;
			model.Scale = Vector3.One * 0.000000001f;
		} else {
			GD.Print("Invalid Plant Name: " + new_plant_name);
		}
	}

	public void clear_plant () {
		current_plant_name = "";
		prototype = null;

		plant_set = false;
		done_growing = false;

		model.Visible = false;
		model.Scale = Vector3.One * 0.000000001f;

		waiting_on_insert = false;
		try_insert = true;
	}

	public void on_output_inventory_changed (Inventory inventory) {
		try_insert = true;
	}

}