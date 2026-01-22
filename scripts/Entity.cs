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
	public string category = "nature";

	[Export]
	public Texture2D display_icon = GD.Load<Texture2D>("res://item_textures/test_item.png");

	[Export]
	public bool secret = false;

	public Array<Inventory> display_inventories;
	public Array<FluidContainer> display_fluid_containers;

	public virtual void release () {
		QueueFree();
	}

}