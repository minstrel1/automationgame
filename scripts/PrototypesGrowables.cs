using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class GrowablePrototype : PrototypeBase {
	
	public string icon_texture = "res://item_textures/test_item.png";

	public float time_to_grow = 1.0f;

	public Godot.Collections.Array harvest_result = new Godot.Collections.Array();

}

public partial class Prototypes : Node {
	public static Dictionary<string, GrowablePrototype> growables = new Dictionary<string, GrowablePrototype>{
		{"test_plant", new GrowablePrototype{
			time_to_grow = 0.01f,
			harvest_result = new Godot.Collections.Array{
				new Dictionary{
					{"type", "item"},
					{"name", "test_item"},
					{"amount", 3},
				}
			},
		}},
	};
}