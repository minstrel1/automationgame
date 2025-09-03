using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
[Tool]
public partial class CheatChest : Chest {
	
	public bool void_unfiltered = false;

	public MultiFilter multi_filter;

	public string filter_item_name = "";
	public int filter_item_count = 0;
	public bool filter_set = false;

	public override void _Ready()
	{
		base._Ready();

		inventory = new Inventory(10);

		inventory.OnInventoryChanged += on_inventory_changed;

		//filter_inventory = new Inventory(1);

		//filter_inventory.OnInventoryChanged += on_filter_inventory_changed;

		multi_filter = new MultiFilter(new Array<string>(), "items", 1);

		multi_filter.OnFiltersChanged += on_filters_changed;

		((ItemSpecialVoxel) special_voxels["voxel"]).inventory = inventory;

		adjust_box();
	}

	public override void on_build () {
		base.on_build();
	}

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);
	}

	public void on_filters_changed (Array<String> filters) {
		if (multi_filter.filters[0] != "") {
			filter_set = true;
			filter_item_name = multi_filter.filters[0];
			filter_item_count = Prototypes.items[multi_filter.filters[0]].stack_size;

			refill();
		} else {
			filter_set = false;
			filter_item_name = "";
			filter_item_count = 0;
		}
	}

	public void on_inventory_changed (Inventory inventory) {
		if (void_unfiltered) {
			InventoryItem item;
			for (int i = 0; i < inventory.contents.Count; i++) {
				item = inventory.contents[i];
				if (item != null) {
					if (filter_item_name == "") {
						inventory.remove_item(item);
					} else {
						if (filter_item_name != item.name) {
							inventory.remove_item(item);
						}
					}
				}
			}
		}

		if (filter_item_name != "") {
			refill();
		}
	}

	public void refill () {
		int item_count = inventory.get_item_count(filter_item_name);
		if (filter_item_count - item_count > 0) {
			inventory.insert(new SimpleItem{name = filter_item_name, count = filter_item_count - item_count});
		}
		
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

	public override void release() {
		inventory.OnInventoryChanged -= on_inventory_changed;
		inventory.release();

		multi_filter.OnFiltersChanged -= on_filters_changed;
		multi_filter.QueueFree();

		base.release();
	}
}
