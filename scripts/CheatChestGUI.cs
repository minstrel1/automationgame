using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class CheatChestGUI : GUI {

	public CheatChest chest;
	public InventoryGUI player_inventory_gui;
	public InventoryGUI chest_inventory_gui;

	public Control inventory_parent;

	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/cheat_chest_gui.tscn");

	public override void _Ready()
	{
		base._Ready();

		Array<GUIDummy> inventory_result = pop_dummy_type("InventoryGUI");

		GUIDummyData result = remove_dummy(inventory_result[0]);

		player_inventory_gui = InventoryGUI.make(Player.instance.inventory, result.parent);
		player_inventory_gui.Position = result.pos;

		result = remove_dummy(inventory_result[1]);

		chest_inventory_gui = InventoryGUI.make(chest.inventory, result.parent);
		chest_inventory_gui.Position = result.pos;
        chest_inventory_gui.CustomMinimumSize = result.min_size;
        chest_inventory_gui.Size = result.size;

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

	public override void release()
	{
		player_inventory_gui.release();
		chest_inventory_gui.release();

		Player.instance.unlock_controls();
		Player.instance.active_inventory = null;

		base.release();
	}
}	
