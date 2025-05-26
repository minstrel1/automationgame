using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class GrowingPlotGUI : GUI {

	public GrowingPlot growing_plot;

	public InventoryGUI player_inventory_gui;
	public ItemRepresentation seed_input_gui;
	public InventoryGUI product_output_gui;

	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/growing_plot_gui.tscn");

	public override void _Ready()
	{
		base._Ready();

		Array<GUIDummy> inventory_result = pop_dummy_type("InventoryGUI");

		GUIDummyData result = remove_dummy(inventory_result[0]);

		player_inventory_gui = InventoryGUI.make(Player.instance.inventory, result.parent);
		result.parent.MoveChild(player_inventory_gui, result.node_index);
		player_inventory_gui.Position = result.pos;
		player_inventory_gui.CustomMinimumSize = result.min_size;
		player_inventory_gui.Size = result.size;

		result = remove_dummy(inventory_result[1]);

		product_output_gui = InventoryGUI.make(growing_plot.get_output_inventory(), result.parent);
		result.parent.MoveChild(product_output_gui, result.node_index);
		product_output_gui.Position = result.pos;
		product_output_gui.CustomMinimumSize = result.min_size;
		product_output_gui.Size = result.size;

		Array<GUIDummy> item_rep_result = pop_dummy_type("ItemRepresentation");

		result = remove_dummy(item_rep_result[0]);

		seed_input_gui = ItemRepresentation.make_item_representation(0, growing_plot.get_input_inventory(), result.parent);

		Player.instance.lock_controls();
		Player.instance.active_inventory = growing_plot.get_input_inventory();

	}

	public static GrowingPlotGUI make (GrowingPlot growing_plot, Control gui_parent) {
		GrowingPlotGUI new_instance = scene.Instantiate<GrowingPlotGUI>();

		new_instance.growing_plot = growing_plot;
		new_instance.gui_parent = gui_parent;

		new_instance.gui_parent.AddChild(new_instance);

		return new_instance;
	}

	public override void release()
	{
		player_inventory_gui.release();
		seed_input_gui.release();
		product_output_gui.release();

		base.release();
	}

	public override void _ExitTree()
	{
		base._ExitTree();

		Player.instance.unlock_controls();
		Player.instance.active_inventory = null;
	}
}