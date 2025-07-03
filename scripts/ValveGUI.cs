using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class ValveGUI : GUI {

	public Valve valve;

	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/valve_gui.tscn");

	public Label flow_label;
	public TextureProgressBar flow_bar;

	public Label system_label;
	public TextureRect system_icon;

	public override void _Ready()
	{
		base._Ready();

		// Array<GUIDummy> inventory_result = pop_dummy_type("InventoryGUI");

		// GUIDummyData result = remove_dummy(inventory_result[0]);

		flow_label = GetNode<Label>("VBoxContainer/VBoxContainer/HBoxContainer/Control2/TextureProgressBar/Label");
		flow_bar = GetNode<TextureProgressBar>("VBoxContainer/VBoxContainer/HBoxContainer/Control2/TextureProgressBar");

		system_label = GetNode<Label>("VBoxContainer/VBoxContainer/HBoxContainer/VBoxContainer/Container/TextureRect/Label");
		system_icon = GetNode<TextureRect>("VBoxContainer/VBoxContainer/HBoxContainer/VBoxContainer/Container/TextureRect");

		flow_bar.MinValue = 0;
		flow_bar.MaxValue = valve.flow_rate;
		flow_bar.Value = valve.container.connected_system.flow_rate_this_frame * 60;

		Player.instance.lock_controls();

	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		
		if (valve.container.connected_system != null) {
			system_label.Text = String.Format("{0:F4} / {1:F4}", valve.container.connected_system.total_amount, valve.container.connected_system.total_volume);
			
			flow_bar.Value = valve.container.connected_system.flow_rate_this_frame * 60;
			flow_label.Text = Math.Round(valve.container.connected_system.flow_rate_this_frame * 60).ToString() + " L/s";
		} 
	}

	public static ValveGUI make (Valve valve, Control gui_parent) {
		ValveGUI new_instance = scene.Instantiate<ValveGUI>();

		new_instance.valve = valve;
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