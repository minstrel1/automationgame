using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class InventoryGUI : GUI {

	public static Array<InventoryGUI> instances = new Array<InventoryGUI>();
	public static System.Collections.Generic.Stack<InventoryGUI> available_instances = new System.Collections.Generic.Stack<InventoryGUI>();

	public Inventory inventory;
	public Array<ItemRepresentation> item_reps = new Array<ItemRepresentation>();

	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/inventory_gui.tscn");

	public GridContainer inventory_grid;

	public override void _Ready()
	{
		base._Ready();

		inventory_grid = GetNode<GridContainer>("ScrollContainer/Control/InventoryGrid");

		init_item_reps();

	}

	public void clear () {
		gui_parent = null;

		foreach (ItemRepresentation rep in item_reps) {
			rep.release();
		}
		item_reps.Clear();

		inventory = null;
	}

	public static InventoryGUI make_inventory_gui (Inventory inventory, Control gui_parent) {
		InventoryGUI new_instance = get_first_available_instance();

		if (new_instance == null) {
			new_instance = scene.Instantiate<InventoryGUI>();
			instances.Add(new_instance);
		}

		new_instance.inventory = inventory;
		new_instance.gui_parent = gui_parent;

		if (new_instance.readied) {
			new_instance.RequestReady();
		}
			
		new_instance.gui_parent.AddChild(new_instance);

		return new_instance;
	}

	public void init_item_reps () {
		for (int i = 0; i < inventory.inventory_size; i++) {
			item_reps.Add(ItemRepresentation.make_item_representation(i, inventory, inventory_grid));
		}
	}

	public static InventoryGUI get_first_available_instance () {
		if (available_instances.Count > 0) {
			return available_instances.Pop();
		}

		return null;
	}

	public override void release()
	{
		in_use = false;

		Node parent = GetParent();
		if (parent != null) {
			parent.RemoveChild(this);
		}

		clear();

		available_instances.Push(this);
	}

	
}