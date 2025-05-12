using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
[Tool]
public partial class GrowingPlot : BuildingGridPlacable, IBuildingWithInventory, IInteractable {
	int plot_width = 2;
	int plot_length = 2;
	double border = 0.25;

	int max_grow_slots;
	Array<Growable> grow_slots_physical = new Array<Growable>();
	
	Inventory input_inventory;
	Inventory output_inventory;
	[Export]
	CsgBox3D grow_area;

	public override void _Ready()
	{
		base._Ready();

		input_inventory = new Inventory (1);
		output_inventory = new Inventory (4);

		adjust_box();
	}

	public override void on_build () {
		float grow_area_width  = (float)(grow_area.Size.X - (border * 2));
		float grow_area_height = (float)(grow_area.Size.Y);
		float grow_area_length = (float)(grow_area.Size.Z - (border * 2));

		for (int x = 0; x < plot_width; x++) {
			for (int z = 0; z < plot_length; z++) {
				Vector3 offset = new Vector3((grow_area_width / plot_width) * x, -grow_area_height / 2, (grow_area_length / plot_length) * z);
				Vector3 coordinate = grow_area.GlobalPosition + offset - new Vector3(grow_area_width / 4, 0, grow_area_length / 4);
				
				Growable new_node = new Growable();
				AddChild(new_node); // add to scene tree first so it does the ready shit

				new_node.GlobalPosition = coordinate;
				new_node.Name = String.Format("{0}, {1}", x, z);
				grow_slots_physical.Add(new_node);
			}
		}

		GD.Print(grow_slots_physical);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		InventoryItem current_seed = input_inventory.get_item_at_index(0);
		string plant_result = "";
		if (current_seed != null) {
			plant_result = (string) current_seed.prototype["plant_result"];
		}
		
		//GD.Print("doing physics");
		foreach (Growable growable in grow_slots_physical) {
			if (! growable.plant_set) {
				if (current_seed != null && plant_result != "") {
					if (current_seed.count > 0) {
						current_seed.count -= 1;
						growable.set_plant(plant_result);
					}
				}
			}
		}

		if (current_seed != null) {
			current_seed.emit_update();
		}
	}

	public Inventory get_input_inventory () {
		return input_inventory;
	}

	public Inventory get_output_inventory () {
		return output_inventory;
	}

	public void on_hover_focus () {

	}

	public void on_hover_unfocus () {

	}

	public void on_interact () {
		if (Player.instance.active_gui is GrowingPlotGUI) {
			Player.instance.clear_active_gui();
		} else {
			Player.set_active_gui(GrowingPlotGUI.make_growing_plot_gui(this, Player.instance.gui_parent));
		}
	}
}