using System;
using System.Collections;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Entity : Node3D {
	[ExportCategory("Basic Properties")]

	[Export]
	public string internal_name = "example_entity";

	[Export]
	public string display_name = "Example Entity";

	[Export]
	public string display_description = "A base entity.";

	[Export]
	public string long_description = "A simple example entity. You should not be seeing this.";

	[Export]
	public string category = "miscellaneous";

	[Export]
	public string sub_category = "";

	[Export]
	public Texture2D display_icon = GD.Load<Texture2D>("res://item_textures/test_item.png");

	[Export]
	public bool unlocked = true;

	[Export]
	public bool secret = false;

	[Export]
	public StaticBody3D collider;

	public Array<Inventory> display_inventories;
	public Array<FluidContainer> display_fluid_containers;

	public virtual void set_collision (bool value) {
		if (collider != null) {
			foreach (uint shape_id in collider.GetShapeOwners()) {
				collider.ShapeOwnerSetDisabled(shape_id, !value);
			}
		}
	}

	public virtual void release () {
		QueueFree();
	}

}