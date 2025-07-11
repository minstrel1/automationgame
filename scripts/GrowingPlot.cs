using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
#if TOOLS
[Tool]
#endif
public partial class GrowingPlot : BuildingGridPlacable, IBuildingWithInventory, IInteractable {

	[ExportCategory("Growing Plot Properties")]
	[Export]
	public string interact_name = "Growing Plot";
	[Export]
	public int plot_width = 2;
	[Export]
	public int plot_length = 2;
	[Export]
	public double border = 0.25;

	[Export]
	public bool auto_harvest = true;
	[Export]
	public float harvest_time = 5.0f;
	[Export]
	public bool auto_plant = true;
	[Export]
	public float plant_time = 5.0f;

	int max_grow_slots;
	
	Array<Growable> grow_slots_physical = new Array<Growable>();
	Inventory input_inventory;
	Inventory output_inventory;
	[Export]
	CsgBox3D grow_area;
	CsgShape3D collider;

	public override void _Ready()
	{
		base._Ready();

		input_inventory = new Inventory (1);
		input_inventory.set_filter(new ItemCategoryFilter("seed"), 0);

		output_inventory = new Inventory (4);

		((ItemSpecialVoxel) special_voxels["seed_input"]).set_inventory(input_inventory);
		((ItemSpecialVoxel) special_voxels["crop_output"]).set_inventory(output_inventory);

		collider = GetNode<CsgBox3D>("CSGBox3D2");

		adjust_box();
	}

	public override void on_build () {

		base.on_build();

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
				output_inventory.OnInventoryChanged += new_node.on_output_inventory_changed;
			}
		}

		//GD.Print(grow_slots_physical);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (Engine.IsEditorHint() || !(current_building_state == BuildingState.built)) {
			return;
		}

		InventoryItem current_seed = input_inventory.get_item_at_index(0);
		string plant_result = "";
		if (current_seed != null) {
			plant_result = current_seed.prototype.plant_result;
		}
		
		bool plant_set_this_frame = false;
		//GD.Print("doing physics");
		foreach (Growable growable in grow_slots_physical) {
			if (auto_harvest) {
				if (growable.done_growing) {
					if (!growable.waiting_on_insert) {
						Godot.Collections.Array<ProductPrototype> harvest_results = growable.prototype.harvest_result;
					
						growable.to_insert = growable.prototype.get_products().Item1;

						growable.waiting_on_insert = true;
					}
					
					if (growable.try_insert) {
						if (output_inventory.can_insert(growable.to_insert)) {
							output_inventory.insert(growable.to_insert);

							growable.clear_plant();
						} else {
							growable.try_insert = false;
						}
					}
					
				}
			}

			if (auto_plant) {
				if (! growable.plant_set) {
					if (current_seed != null && plant_result != "") {
						if (current_seed.count > 0) {
							current_seed.count -= 1;
							current_seed.emit_update();
							growable.set_plant(plant_result);
							plant_set_this_frame = true;
						}
					}
				}
			}
		}

		if (current_seed != null && plant_set_this_frame) {
			current_seed.emit_update();
		}
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
		if (current_building_state == BuildingState.built) {
			if (Player.instance.active_gui is GrowingPlotGUI) {
				Player.instance.clear_active_gui();
			} else {
				Player.set_active_gui(GrowingPlotGUI.make(this, Player.instance.gui_parent));
			}
		}
	}

	public string get_interact_text() {
		return "Configure " + interact_name;
	}
}