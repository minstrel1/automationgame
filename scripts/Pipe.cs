using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
#if TOOLS
[Tool]
#endif
public partial class Pipe : BuildingGridPlacable, IInteractable {

	[ExportCategory("Pipe Properties")]
	[Export]
	public string interact_name = "Pipe";
	[Export]
	public float volume = 100.0f;

	public FluidContainer container;

	public override void _Ready() {
		base._Ready();

		container = new FluidContainer(volume);
		container.set_fluid_filter("test_fluid");
		container.current_amount = new RandomNumberGenerator().RandfRange(0.1f, 99.9f);

		((FluidSpecialVoxel) special_voxels["voxel"]).set_container(container);
	}

	public override void on_build() {
		base.on_build();
	}

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);
	}

	public void on_hover_focus () {
		//set_mesh_visibility(true);
	}

	public void on_hover_unfocus () {
		//set_mesh_visibility(false);
	}

	public void on_interact () {

	}

	public string get_interact_text () {
		return "Configure " + interact_name;
	}
}