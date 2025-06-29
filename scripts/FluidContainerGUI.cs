using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class FluidContainerGUI : GUI {

	public FluidContainer fluid_container;

	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/fluid_container_gui.tscn");

	public override void _Ready()
	{
		base._Ready();

		// Array<GUIDummy> inventory_result = pop_dummy_type("InventoryGUI");

		// GUIDummyData result = remove_dummy(inventory_result[0]);

		Player.instance.lock_controls();

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