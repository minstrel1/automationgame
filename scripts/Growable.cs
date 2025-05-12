using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
public partial class Growable : Node3D {
	
	public string current_plant_name = "";
	public int growth_time = 0;
	public int current_growth_time = 0;
	public bool plant_set = false;

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
		AddChild(model);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		//ulong start = Time.GetTicksUsec();
		
		if (current_growth_time < growth_time) {
			current_growth_time += 1;
			float scale = (float) current_growth_time / growth_time;
			model.Scale = Vector3.One * scale;

			if (current_growth_time == growth_time) {

			}
		}

		//ulong time = Time.GetTicksUsec() - start;
		//GD.Print("GROWABLE" + Name + "PHYSICS PROCESS TIME:" + time.ToString());
	}

	public void set_plant (String new_plant_name) {
		if (Prototypes.growables.ContainsKey(new_plant_name)) {
			Dictionary plant_data = (Dictionary)Prototypes.growables[new_plant_name];
			current_plant_name = new_plant_name;
			current_growth_time = 0;
			growth_time = Globals.hours_to_ticks((float) plant_data["time_to_grow"]);
			plant_set = true;

			model.Visible = true;
			model.Scale = Vector3.One * 0.000000001f;
		} else {
			GD.Print("Invalid Plant Name: " + new_plant_name);
		}
	}

}