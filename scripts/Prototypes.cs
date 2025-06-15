using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Prototypes : Node {
	public static Prototypes Instance {get; private set;}

	public override void _Ready()
	{
		Instance = this;

		create_building_dict();
	}

	public static Dictionary<string, Godot.Collections.Array<RecipePrototype>> get_recipes_categorized (SubcategoryFilter filter) {
		Dictionary<string, Godot.Collections.Array<RecipePrototype>> result = new Dictionary<string, Godot.Collections.Array<RecipePrototype>>();

		foreach (string key in recipes.Keys) {
			string category = recipes[key].category;

			if (filter.match(recipes[key].subcategory)) {
				if (!result.ContainsKey(category)) {
					result[category] = new Array<RecipePrototype>();
				}

				result[category].Add(recipes[key]);
			}
		}

		return result;
	}
}