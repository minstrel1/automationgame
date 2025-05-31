using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class ItemSpecialVoxel : SpecialVoxel {

	public Inventory inventory;

	public Array<Inventory> target_inventories = new Array<Inventory>();
	public int[] target_indices = {};
	public int robin_index;
	public int robin_count;

	public bool auto_output = false;
	public bool auto_input = false;

	public override void update_voxel_connections() {
		base.update_voxel_connections();

		target_inventories.Clear();

		foreach (SpecialVoxel voxel in connected_voxels) {
			if (voxel is ItemSpecialVoxel) {
				if (((ItemSpecialVoxel) voxel).inventory != null) {
					GD.Print(((ItemSpecialVoxel) voxel).inventory);
					target_inventories.Add(((ItemSpecialVoxel) voxel).inventory);
				}
			}
		}

		robin_index = 0;
		robin_count = target_inventories.Count;
		target_indices = new int[robin_count];
	}

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);

		if (auto_input) {
			for (int index = 0; index < robin_count; index++) {
				foreach (InventoryItem item in target_inventories[index].contents) {
					if (item != null) {
						inventory.insert(item);
					}
				}
			}
		}

		if (auto_output) {
			for (int index = 0; index < robin_count; index++) {
				foreach (InventoryItem item in inventory.contents) {
					if (item != null) {
						target_inventories[index].insert(item);
					}
				}
			}
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