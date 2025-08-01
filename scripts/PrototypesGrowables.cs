using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Prototypes : Node {
	public static Dictionary<string, GrowablePrototype> growables = new Dictionary<string, GrowablePrototype>{
		{"test_plant", new GrowablePrototype{
			time_to_grow = 5.0f,
			harvest_result = new Godot.Collections.Array<ProductPrototype>{
				new ItemProductPrototype {
					name = "test_item",
					amount = 3,
				}
			},
			growing_fluid = "water",
			fluid_to_grow = 50.0f,
		}},
	};
}