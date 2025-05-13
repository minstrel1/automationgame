using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class InventoryGUI : GUI {

	public Inventory inventory;
	public Array<ItemRepresentation> item_reps = new Array<ItemRepresentation>();
	public GridContainer inventory_grid;

	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/inventory_gui.tscn");

	public override void _Ready()
	{
		base._Ready();

		inventory_grid = GetNode<GridContainer>("ScrollContainer/Control/InventoryGrid");

		init_item_reps();

		//GetNode<Control>("ScrollContainer/Control").Size = inventory_grid.Size;
		
	}

	public static InventoryGUI make_inventory_gui (Inventory inventory, Control gui_parent) {
		InventoryGUI new_instance = scene.Instantiate<InventoryGUI>();

		new_instance.inventory = inventory;
		new_instance.gui_parent = gui_parent;

		new_instance.gui_parent.AddChild(new_instance);

		return new_instance;
	}

	public void init_item_reps () {
		for (int i = 0; i < inventory.inventory_size; i++) {
			item_reps.Add(ItemRepresentation.make_item_representation(i, inventory, inventory_grid));
		}
	}
}