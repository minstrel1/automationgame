using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Prototypes : Node {
	public static Dictionary<string, GrowablePrototype> growables = new Dictionary<string, GrowablePrototype>{
		{"test_plant", new GrowablePrototype{
			time_to_grow = 0.01f,
			harvest_result = new Godot.Collections.Array<ProductPrototype>{
				new ItemProductPrototype {
					name = "test_item",
					amount = 3,
				}
			},
		}},
	};
}