using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class CheatChestGUI : GUI {

	public CheatChest chest;
	public InventoryGUI player_inventory_gui;
	public InventoryGUI chest_inventory_gui;

	public CheckBox void_unfiltered_check;

	public ItemRepresentation item_filter_slot;

	public Control inventory_parent;

	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/cheat_chest_gui.tscn");

	public override void _Ready()
	{
		base._Ready();

		Array<GUIDummy> inventory_result = pop_dummy_type("InventoryGUI");

		GUIDummyData result = remove_dummy(inventory_result[0]);

		player_inventory_gui = InventoryGUI.make(Player.instance.inventory, result.parent);
		player_inventory_gui.Position = result.pos;
		player_inventory_gui.CustomMinimumSize = result.min_size;
		player_inventory_gui.Size = result.size;

		result = remove_dummy(inventory_result[1]);

		chest_inventory_gui = InventoryGUI.make(chest.inventory, result.parent);
		GD.Print(result.min_size);
		chest_inventory_gui.Position = result.pos;
		chest_inventory_gui.CustomMinimumSize = result.min_size;
		chest_inventory_gui.Size = result.size;

		void_unfiltered_check = GetNode<CheckBox>("VBoxContainer/HBoxContainer/Control2/Control/HBoxContainer/CheckBox");
		void_unfiltered_check.ButtonPressed = chest.void_unfiltered;
		void_unfiltered_check.Toggled += on_check_changed;

		Array<GUIDummy> item_slot_result = pop_dummy_type("ItemRepresentation");

		result = remove_dummy(item_slot_result[0]);

		item_filter_slot = ItemRepresentation.make_item_representation(0, chest.filter_inventory, result.parent);
		item_filter_slot.Position = result.pos;

		Player.instance.lock_controls();
		Player.instance.active_inventory = chest.inventory;
	}

	public static CheatChestGUI make_chest_gui (CheatChest chest, Control gui_parent) {
		CheatChestGUI new_instance = scene.Instantiate<CheatChestGUI>();

		new_instance.chest = chest;
		new_instance.gui_parent = gui_parent;

		new_instance.gui_parent.AddChild(new_instance);

		return new_instance;
	}

	public void on_check_changed (bool toggle) {
		if (toggle) {
			chest.on_inventory_changed(chest.inventory);
		}
	}

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);

		chest.void_unfiltered = void_unfiltered_check.ButtonPressed;
	}

	public override void release()
	{
		player_inventory_gui.release();
		chest_inventory_gui.release();

		void_unfiltered_check.Toggled -= on_check_changed;

		item_filter_slot.release();

		Player.instance.unlock_controls();
		Player.instance.active_inventory = null;

		base.release();
	}
}	
