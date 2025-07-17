using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
#if TOOLS
[Tool]
#endif
public partial class WaterCollector : BuildingGridPlacable, IInteractable {

	[ExportCategory("Water Collector Properties")]
	[Export]
	public string interact_name = "Water Collector";
	[Export]
	public float volume = 100.0f;
	[Export]
	public float max_water_per_second = 50.0f;

	public float water_made_this_frame;

	public FluidContainer container;

	public override void _Ready() {
		base._Ready();

		container = new FluidContainer(volume);

		((FluidSpecialVoxel) special_voxels["WATEROUTPUT"]).set_container(container);
	}

	public override void on_build() {
		base.on_build();
	}

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);

		if (current_building_state == BuildingState.built) {
			water_made_this_frame = container.insert("water", max_water_per_second * (float) delta);
		}
	}

	public void on_hover_focus () {
		//set_mesh_visibility(true);
	}

	public void on_hover_unfocus () {
		//set_mesh_visibility(false);
	}

	public void on_interact () {
		if (current_building_state == BuildingState.built) {
			if (Player.instance.active_gui is FluidContainerGUI) {
				Player.instance.clear_active_gui();
			} else {
				Player.set_active_gui(FluidContainerGUI.make(container, Player.instance.gui_parent));
			}
		}
	}

	public string get_interact_text () {
		return "Configure " + interact_name;
	}
}