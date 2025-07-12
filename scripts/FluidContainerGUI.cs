using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class FluidContainerGUI : GUI {

	public FluidContainer fluid_container;

	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/fluid_container_gui.tscn");

	public Label container_label;
	public TextureRect container_icon;

	public Label system_label;
	public TextureRect system_icon;

	public string current_fluid = "";

	public override void _Ready()
	{
		base._Ready();

		// Array<GUIDummy> inventory_result = pop_dummy_type("InventoryGUI");

		// GUIDummyData result = remove_dummy(inventory_result[0]);

		container_label = GetNode<Label>("VBoxContainer/Container/TextureRect/Label");
		container_icon = GetNode<TextureRect>("VBoxContainer/Container/TextureRect");

		system_label = GetNode<Label>("VBoxContainer/System/TextureRect/Label");
		system_icon = GetNode<TextureRect>("VBoxContainer/System/TextureRect");

		current_fluid = fluid_container.connected_system.current_fluid;

		if (current_fluid != "") {
			system_icon.Texture = GD.Load<Texture2D>(Prototypes.fluids[current_fluid].icon_texture);
			container_icon.Texture = GD.Load<Texture2D>(Prototypes.fluids[current_fluid].icon_texture);
		}

		

		Player.instance.lock_controls();

	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		container_label.Text = String.Format("{0:F4} / {1:F4}", fluid_container.current_amount, fluid_container.volume);

		if (fluid_container.connected_system != null) {
			system_label.Text = String.Format("{0:F4} / {1:F4}", fluid_container.connected_system.total_amount, fluid_container.connected_system.total_volume);

			if (current_fluid != fluid_container.connected_system.current_fluid) {
				current_fluid = fluid_container.connected_system.current_fluid;

				if (current_fluid != "") {
					system_icon.Texture = GD.Load<Texture2D>(Prototypes.fluids[current_fluid].icon_texture);
					container_icon.Texture = GD.Load<Texture2D>(Prototypes.fluids[current_fluid].icon_texture);
				}
			}
		} 
	}

	public static FluidContainerGUI make (FluidContainer fluid_container, Control gui_parent) {
		FluidContainerGUI new_instance = scene.Instantiate<FluidContainerGUI>();

		new_instance.fluid_container = fluid_container;
		new_instance.gui_parent = gui_parent;

		new_instance.gui_parent.AddChild(new_instance);

		return new_instance;
	}

	public override void release()
	{

		Player.instance.unlock_controls();
		Player.instance.active_inventory = null;

		base.release();
	}

	public override void _ExitTree()
	{
		base._ExitTree();

	}
}
