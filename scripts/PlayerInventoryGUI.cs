using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class PlayerInventoryGUI : GUI {

	public Inventory inventory;
	public InventoryGUI inventory_gui;

	public Control inventory_gui_parent;

	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/player_inventory_gui.tscn");

	public override void _Ready()
	{
		base._Ready();

		GUIDummyData result = remove_dummy(pop_dummy_singular("InventoryGUI"));

		inventory_gui_parent = result.parent;
		
		inventory_gui = InventoryGUI.make(inventory, inventory_gui_parent);
	}

	public static PlayerInventoryGUI make_player_inventory_gui (Inventory inventory, Control gui_parent) {
		PlayerInventoryGUI new_instance = scene.Instantiate<PlayerInventoryGUI>();

		new_instance.inventory = inventory;
		new_instance.gui_parent = gui_parent;

		new_instance.gui_parent.AddChild(new_instance);

		return new_instance;
	}

	public override void release()
	{
		inventory_gui.release();

		base.release();
	}

}