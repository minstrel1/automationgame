using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
[Tool]
public partial class CheatChest : Chest {
	
	public Inventory filter_inventory;
	public bool void_unfiltered = false;

	public override void _Ready()
	{
		base._Ready();

		inventory = new Inventory(10);

		filter_inventory = new Inventory(1);

		((ItemSpecialVoxel) special_voxels["voxel"]).inventory = inventory;

		adjust_box();
	}

	public override void on_build () {
		base.on_build();
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}

	public void on_hover_focus () {

	}

	public void on_hover_unfocus () {

	}

	public override void set_collision(bool value) {
		
	}

	public override void on_interact () {
		if (current_building_state == BuildingState.built) {
			if (Player.instance.active_gui is CheatChestGUI) {
				Player.instance.clear_active_gui();
			} else {
				Player.set_active_gui(CheatChestGUI.make_chest_gui(this, Player.instance.gui_parent));
			}
		}
	}

	public string get_interact_text () {
		if (current_building_state == BuildingState.built) {
			return "Open " + interact_name;
		}
		return null;
	}
}
