using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class ItemPrototype : PrototypeBase {
	public new string category = "miscellaneous";
	public string icon_texture = "res://item_textures/test_item.png";

	public int stack_size = 50;

	public bool plantable = false;
	public string plant_result = "";
}