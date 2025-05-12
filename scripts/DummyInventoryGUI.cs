using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
public partial class DummyInventoryGUI : GUIDummy {
	
	public override string replacable_by {
		get {
			return "InventoryGUI";
		}
	}

	[Export]
	public int inventory_size {
		get;
		set {
			field = value;
			if (Engine.IsEditorHint()) {
				init_item_reps();
			}
		}
	} = 50;

	public Array<Control> item_reps = new Array<Control>();
	public GridContainer inventory_grid;

	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/dummy_item_representation.tscn");

	public override void _Ready () {
		base._Ready();

		inventory_grid = GetNode<GridContainer>("ScrollContainer/InventoryGrid");

		if (Engine.IsEditorHint()) {
			init_item_reps();
		}
		
	}
	
	public void init_item_reps () {
		foreach (Control dummy in item_reps) {
			if (dummy != null) {
				dummy.QueueFree();
			}
			
		}

		item_reps.Clear();

		for (int i = 0; i < inventory_size; i++) {
			Control new_instance = scene.Instantiate<Control>();
			if (new_instance != null && inventory_grid != null) {
				inventory_grid.AddChild(new_instance);
				item_reps.Add(new_instance);
			}
		}
	}
}