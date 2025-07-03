using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Prototypes : Node {
	public static Dictionary<string, FluidPrototype> fluids = new Dictionary<string, FluidPrototype> {
		{"test_fluid", new FluidPrototype {
			name = "test_fluid",
			category = "miscellaneous",
			display_name = "Test Fluid",
			display_description = "This is a test fluid.",
			icon_texture = "res://item_textures/fluid.png",
		}}, 
		{"water", new FluidPrototype {
			name = "water",
			category = "miscellaneous",
			display_name = "Water",
			display_description = "It's water.",
			icon_texture = "res://item_textures/water.png",
		}}, 
	};
}