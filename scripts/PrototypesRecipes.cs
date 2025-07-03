using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public enum RecipeCategory {
	components,
}



public partial class Prototypes : Node {
	public static int max_recipe_ingredients = 8;

	public static Dictionary recipe_category_properties = new Dictionary{
		{"components", new Dictionary {
			{"display_name", "Components"},
			{"icon_texture", "res://item_textures/test_item.png"}
		}},

	};

	public static Dictionary<string, RecipePrototype> recipes = new Dictionary<string, RecipePrototype> {
		{"test_product", new RecipePrototype{
			name = "test_product",
			display_name = "Test Product",
			category = "components",
			subcategory = "basic_crafting",
			time_to_craft = 0.3f,
			ingredients = new Array<IngredientPrototype>{
				new ItemIngredientPrototype{
					name = "test_item",
					amount = 2,
				},
			},
			products = new Array<ProductPrototype>{
				new ItemProductPrototype{
					name = "test_item",
					amount = 1,
				}
			},
		}},
	};
}