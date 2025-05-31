using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
[Tool]
public partial class Chest : BuildingGridPlacable, IBuildingWithInventory, IInteractable {
	
	public Inventory inventory;

	[Export]
	public string interact_name = "Chest";

	public override void _Ready()
	{
		base._Ready();

		inventory = new Inventory(10);

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

	public Inventory get_input_inventory () {
		return inventory;
	}

	public Inventory get_output_inventory () {
		return inventory;
	}

	public void on_hover_focus () {

	}

	public void on_hover_unfocus () {

	}

	public override void set_collision(bool value)
	{
		
	}

	public void on_interact () {
		if (is_built) {
			if (Player.instance.active_gui is ChestGUI) {
				Player.instance.clear_active_gui();
			} else {
				Player.set_active_gui(ChestGUI.make_chest_gui(this, Player.instance.gui_parent));
			}
		}
	}

	public string get_interact_text () {
		if (is_built) {
			return "Open " + interact_name;
		}
		return null;
	}
}