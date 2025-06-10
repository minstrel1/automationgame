using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Prototypes : Node {

	public static Dictionary buildings;

	public static Dictionary building_category_properties = new Dictionary{
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
	};

	public static string building_scene_directory = "res://building_scenes";

	private static string[] desired_building_properties = {
		"display_name",
		"display_description",
		"building_category",
		"unlocked",
		"display_icon",
		"building_time",
		"building_cost",
	};

	public void create_building_dict () {
		buildings = new Dictionary();

		string[] category_names = Enum.GetNames<BuildingCategory>();

		foreach (string name in category_names) {
			buildings[name] = new Godot.Collections.Array();
		}

		string[] building_res_names = ResourceLoader.ListDirectory(building_scene_directory);

		foreach (string building_res_name in building_res_names) {
			Dictionary properties = Tools.packed_scene_to_properties(building_scene_directory + "/" + building_res_name);
			Dictionary result = new Dictionary();

			result["res_path"] = building_scene_directory + "/" + building_res_name;

			BuildingGridPlacable script_instance = ((BuildingGridPlacable)((CSharpScript) properties["script"]).New());
			
			foreach (string property in desired_building_properties) {
				if (properties.ContainsKey(property)) {
					result[property] = properties[property];
				} else {
					result[property] = script_instance.Get(property);
				}
			}
			
			string category = Enum.GetName<BuildingCategory>((BuildingCategory) (int) result["building_category"]);

			((Godot.Collections.Array) buildings[category]).Add(result);

		}

		GD.Print(buildings);
	}

}