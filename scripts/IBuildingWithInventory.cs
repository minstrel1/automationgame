using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public interface IBuildingWithInventory {
	Inventory get_input_inventory ();

	Inventory get_output_inventory ();
}