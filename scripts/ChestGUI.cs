using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class ChestGUI : GUI {

	public Chest chest;
	public InventoryGUI player_inventory_gui;
	public InventoryGUI chest_inventory_gui;

	public Control inventory_parent;

	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/chest_gui.tscn");

	public override void _Ready()
	{
		base._Ready();

		inventory_parent = GetNode<Control>("VBoxContainer/HBoxContainer");

		player_inventory_gui = InventoryGUI.make_inventory_gui(Player.instance.inventory, inventory_parent);
		chest_inventory_gui = InventoryGUI.make_inventory_gui(chest.inventory, inventory_parent);
	}

	public static ChestGUI make_chest_gui (Chest chest, Control gui_parent) {
		ChestGUI new_instance = scene.Instantiate<ChestGUI>();

		new_instance.chest = chest;
		new_instance.gui_parent = gui_parent;

		new_instance.gui_parent.AddChild(new_instance);

		return new_instance;
	}
}	