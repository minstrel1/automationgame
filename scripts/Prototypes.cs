using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Prototypes : Node {
	public static Prototypes Instance {get; private set;}
	public static Dictionary growables = new Dictionary{
		{"test_plant", new Dictionary{
			{"time_to_grow", 0.09},
			{"harvest_result", new Godot.Collections.Array{
				new Dictionary{
					{"type", "item"},
					{"name", "test_item"},
					{"amount", 3},
				}
			}},
		}},
	};

	public static Dictionary items = new Dictionary{
		{"test_item", new Dictionary{
			{"name", "test_item"},
			{"icon_texture", "res://item_textures/test_item.png"},
			{"stack_size", 50},
			{"plant_result", ""},
		}},
		{"test_cock", new Dictionary{
			{"name", "test_cock"},
			{"icon_texture", "res://item_textures/test_cock.png"},
			{"stack_size", 25},
			{"plant_result", ""},
		}},
		{"test_seeds", new Dictionary{
			{"name", "test_seeds"},
			{"icon_texture", "res://item_textures/test_seeds.png"},
			{"stack_size", 50},
			{"plant_result", "test_plant"},
		}},
	};

	public override void _Ready()
	{
		Instance = this;
	}
}