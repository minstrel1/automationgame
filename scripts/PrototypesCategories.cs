using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Prototypes : Node {

	public static Dictionary<string, Dictionary> thing_categories = new Dictionary<string, Dictionary> {
		{"logistics", new Dictionary {
			{"display_name", "Logistics"},
			{"icon_texture", "res://item_textures/test_item.png"}
		}},
		{"agriculture", new Dictionary {
			{"display_name", "Agriculture"},
			{"icon_texture", "res://item_textures/test_item.png"}
		}},
		{"production", new Dictionary {
			{"display_name", "Production"},
			{"icon_texture", "res://item_textures/test_item.png"}
		}},
		{"structures", new Dictionary {
			{"display_name", "Structures"},
			{"icon_texture", "res://item_textures/test_item.png"}
		}},
		{"decorations", new Dictionary {
			{"display_name", "Decorations"},
			{"icon_texture", "res://item_textures/test_item.png"}
		}},
		{"miscellaneous", new Dictionary {
			{"display_name", "Miscellaneous"},
			{"icon_texture", "res://item_textures/test_item.png"}
		}},
		{"developer", new Dictionary{
			{"display_name", "Developer"},
			{"icon_texture", "res://item_textures/test_item.png"},
		}},
	};

	public static Dictionary item_categories = new Dictionary {
		{"miscellaneous", new Dictionary {
			{"display_name", "Miscellaneous"},
			{"icon_texture", "res://item_textures/test_item.png"}
		}},
		{"seed", new Dictionary {
			{"display_name", "Seed"},
			{"icon_texture", "res://item_textures/test_item.png"}
		}},
		{"drone", new Dictionary {
			{"display_name", "Drone"},
			{"icon_texture", "res://item_textures/test_item.png"}
		}},
	};
	
}