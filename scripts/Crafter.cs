using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
#if TOOLS
[Tool]
#endif
public partial class Crafter : BuildingGridPlacable, IBuildingWithInventory, IInteractable {

	[ExportCategory("Crafter Properties")]
	[Export]
	public string interact_name = "Crafter";
	[Export]
	public float crafting_speed = 1.0f;

	Inventory input_inventory;
	Inventory output_inventory;



	CsgShape3D collider;

	public override void _Ready() {
		base._Ready();

		input_inventory = new Inventory(5);
		output_inventory = new Inventory(5);

		collider = GetNode<CsgBox3D>("CSGBox3D");
	}

	public override void on_build()
	{
		base.on_build();

		
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);


	}

	public void on_input_inventory_changed () {

	}

	public void on_output_inventory_changed () {
		
	}


	public Inventory get_input_inventory () {
		return input_inventory;
	}

	public Inventory get_output_inventory () {
		return output_inventory;
	}

	public override void set_collision(bool value)
	{
		collider.UseCollision = value;
	}

	public void on_hover_focus () {
		set_mesh_visibility(true);
	}

	public void on_hover_unfocus () {
		set_mesh_visibility(false);
	}

	public void on_interact () {
		if (is_built) {
			// if (Player.instance.active_gui is GrowingPlotGUI) {
			// 	Player.instance.clear_active_gui();
			// } else {
			// 	//Player.set_active_gui(GrowingPlotGUI.make(this, Player.instance.gui_parent));
			// }
		}
	}

	public string get_interact_text() {
		return "Configure " + interact_name;
	}
}