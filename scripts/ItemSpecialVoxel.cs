using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class ItemSpecialVoxel : SpecialVoxel {

	public Inventory inventory;
	public int inventory_count;

	public Inventory target_inventory;
	public int target_count;

	public bool is_target = false;

	public bool input_should_check;
	public int  input_index;

	public bool output_should_check;
	public int  output_index;

	public bool auto_output = false;
	public bool auto_input = false;

	public FilterBase any_filter;

	public override void update_voxel_connections() {
		base.update_voxel_connections();

		if (target_inventory != null) {
			target_inventory.OnInventoryChanged -= on_inventory_changed;
		}

		target_inventory = null;
		is_target = false;

		int index = 0;
		foreach (SpecialVoxel voxel in connected_voxels) {
			if (voxel is ItemSpecialVoxel) {
				if (((ItemSpecialVoxel) voxel).inventory != null) {
					Inventory inventory = ((ItemSpecialVoxel) voxel).inventory;
					GD.Print(inventory);
					target_inventory = inventory;

					inventory.OnInventoryChanged += on_inventory_changed;

					target_count = target_inventory.inventory_size;

					is_target = true;

					index += 1;
				}
			}
		}

		inventory_count = inventory.inventory_size;

		input_index = 0;
		input_should_check = true;

		output_index = 0;
		output_should_check = true;

		any_filter = new FilterBase();
	}

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);

		if (is_target) {
			if (auto_input) {
				if (input_should_check) {
					if (input_index >= target_count) {
						input_index = 0;
						input_should_check = false;
						goto SkipInput;
					}

					int result = target_inventory.get_first_item(any_filter, input_index);

					if (result != -1) {
						input_index = result;

						inventory.insert(target_inventory.contents[input_index]);

						input_index += 1;
					} else {
						input_index = target_count;
					}

					// for (int index = 0; index < input_robin_count; index++) {
					// 	foreach (InventoryItem item in target_inventories[index].contents) {
					// 		if (item != null) {
					// 			inventory.insert(item);
					// 		}
					// 	}
					// }
				}
			}

			SkipInput:

			if (auto_output) {
				if (output_should_check) {
					if (output_index >= inventory_count) {
						output_index = 0;
						output_should_check = false;
						goto SkipOutput;
					}

					int result = inventory.get_first_item(any_filter, output_index);

					if (result != -1) {
						output_index = result;

						target_inventory.insert(inventory.contents[output_index]);

						output_index += 1;
					} else {
						output_index = inventory_count;
					}
				}
				// for (int index = 0; index < output_robin_count; index++) {
				// 	foreach (InventoryItem item in inventory.contents) {
				// 		if (item != null) {
				// 			target_inventories[index].insert(item);
				// 		}
				// 	}
				// }
			}

			SkipOutput:

			return;
		}
		
	}

	public void on_inventory_changed (Inventory inventory) {
		input_should_check = true;
		output_should_check = true;
	}

	public void set_inventory (Inventory inventory) {
		if (inventory != null) {
			inventory.OnInventoryChanged -= on_inventory_changed;
		}

		this.inventory = inventory;
		inventory.OnInventoryChanged += on_inventory_changed;
	}

	public void clear_inventory () {
		if (inventory != null) {
			inventory.OnInventoryChanged -= on_inventory_changed;
		}
	}

	public override void on_build()
	{
		base.on_build();

		switch (voxel_flags) {
			case SpecialVoxelFlags.ItemInput:
				auto_input = true;
				break;
			case SpecialVoxelFlags.ItemOutput:
				auto_output = true;
				break;
		}
	}

}