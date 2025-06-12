using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class CrafterGUI : GUI {

	public Crafter crafter;

	public InventoryGUI player_inventory_gui;
	public InventoryGUI input_inventory_gui;
	public InventoryGUI output_inventory_gui;

	public TextureRect recipe_icon;
	public Label recipe_label;

	public TextureRect change_recipe_button;

	public ProgressBar crafting_progress_bar;

	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/crafter_gui.tscn");

	public override void _Ready() {
		base._Ready();

		Array<GUIDummy> dummy_result = pop_dummy_type("InventoryGUI");

		GUIDummyData result = remove_dummy(dummy_result[0]);

		player_inventory_gui = InventoryGUI.make(Player.instance.inventory, result.parent);
		result.parent.MoveChild(player_inventory_gui, result.node_index);
		player_inventory_gui.Position = result.pos;
		player_inventory_gui.CustomMinimumSize = result.min_size;
		player_inventory_gui.Size = result.size;

		result = remove_dummy(dummy_result[1]);

		input_inventory_gui = InventoryGUI.make(crafter.input_inventory, result.parent);
		result.parent.MoveChild(input_inventory_gui, result.node_index);
		input_inventory_gui.Position = result.pos;
		input_inventory_gui.CustomMinimumSize = result.min_size;
		input_inventory_gui.Size = result.size;

		result = remove_dummy(dummy_result[2]);

		output_inventory_gui = InventoryGUI.make(crafter.output_inventory, result.parent);
		result.parent.MoveChild(output_inventory_gui, result.node_index);
		output_inventory_gui.Position = result.pos;
		output_inventory_gui.CustomMinimumSize = result.min_size;
		output_inventory_gui.Size = result.size;

		recipe_icon = GetNode<TextureRect>("VBoxContainer/HBoxContainer/VBoxContainer/Control2/PanelContainer/Control/TextureRect");
		recipe_label = GetNode<Label>("VBoxContainer/HBoxContainer/VBoxContainer/Control2/PanelContainer/Control/TextureRect/Label");

		recipe_icon.Texture = GD.Load<Texture2D>(crafter.current_recipe.icon_texture);
		recipe_label.Text = crafter.current_recipe.display_name;

		change_recipe_button = GetNode<TextureRect>("VBoxContainer/HBoxContainer/VBoxContainer/Control2/PanelContainer/Control/PanelContainer/Button");

		change_recipe_button.GuiInput += on_recipe_change_clicked;

		crafting_progress_bar = GetNode<ProgressBar>("VBoxContainer/HBoxContainer/VBoxContainer/VBoxContainer/Control/PanelContainer/Control/HBoxContainer/VBoxContainer/ProgressBar");

		Player.instance.active_inventory = crafter.input_inventory;
		Player.instance.lock_controls();
	}

	public static CrafterGUI make (Crafter crafter, Control gui_parent) {
		CrafterGUI new_instance = scene.Instantiate<CrafterGUI>();

		new_instance.crafter = crafter;
		new_instance.gui_parent = gui_parent;

		new_instance.gui_parent.AddChild(new_instance);

		return new_instance;
	}

	public void on_recipe_change_clicked (InputEvent @event) {
		if (@event is InputEventMouseButton) {
			InputEventMouseButton mouse_event = (InputEventMouseButton) @event;
			if (mouse_event.ButtonIndex == MouseButton.Left && mouse_event.Pressed) {
				crafter.clear_recipe(true);
				crafter.make_recipe_gui();
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (released) { return; }

		crafting_progress_bar.Value = crafter.current_crafting_time / crafter.current_recipe.time_to_craft;
	}

	public override void release () {
		Player.instance.unlock_controls();
		Player.instance.active_inventory = null;

		player_inventory_gui.release();
		input_inventory_gui.release();
		output_inventory_gui.release();

		base.release();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
	}
}