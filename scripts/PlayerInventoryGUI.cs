using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class PlayerInventoryGUI : GUI {

	public Player player;

	public Inventory player_inventory;
	public InventoryGUI inventory_gui;

	public Control inventory_gui_parent;

	public ItemRepresentation player_perch_item_slot;

	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/player_inventory_gui.tscn");

	public override void _Ready()
	{
		base._Ready();

		GUIDummyData result = remove_dummy(pop_dummy_singular("InventoryGUI"));

		inventory_gui = InventoryGUI.make(player.inventory, result.parent);

		result = remove_dummy(pop_dummy_singular("ItemRepresentation"));

		player_perch_item_slot = ItemRepresentation.make_item_representation(0, player.player_perch.inventory, result.parent);
		
	}

	public static PlayerInventoryGUI make_player_inventory_gui (Player player, Control gui_parent) {
		PlayerInventoryGUI new_instance = scene.Instantiate<PlayerInventoryGUI>();

		new_instance.player = player;

		new_instance.player_inventory = player.inventory;
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
