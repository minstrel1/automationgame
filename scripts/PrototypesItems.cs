using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Prototypes : Node {
	public static Dictionary<string, ItemPrototype> items = new Dictionary<string, ItemPrototype>{
		{"test_thing", new ItemPrototype{

		}},
		{"test_item", new ItemPrototype{
			name = "test_item",
			category = "miscellaneous",
			display_name = "Test Item",
			display_description = "This is a test item.",
			icon_texture = "res://item_textures/test_item.png",
			stack_size = 50,
			plant_result = "",
		}},
		{"test_cock", new ItemPrototype{
			name = "test_cock",
			category = "miscellaneous",
			display_name = "Test Cock",
			display_description = "This is a test item.",
			icon_texture = "res://item_textures/test_cock.png",
			stack_size = 25,
			plant_result = "",
		}},
		{"test_seeds", new ItemPrototype{
			name = "test_seeds",
			category = "seed",
			display_name = "Test Seeds",
			display_description = "This is a test item.",
			icon_texture = "res://item_textures/test_seeds.png",
			stack_size = 50,
			plant_result = "test_plant",
		}},
	};
}