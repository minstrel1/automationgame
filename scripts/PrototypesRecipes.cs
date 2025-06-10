using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class ItemProductPrototype : ProductPrototype {
	public override string type {set; get;} = "item";
	public override string name {set; get;} = "test_item";
	public override int amount {set; get;}= 1;

	
}

public partial class ItemIngredientPrototype : IngredientPrototype {
	public override string type {set; get;} = "item";
	public override string name {set; get;} = "test_item";
	public override int amount {set; get;}= 1;
}

public enum RecipeCategory {
	basic_crafting,
}

public partial class RecipePrototype : PrototypeBase {
	public string icon_texture = "res://item_textures/test_item.png";

	public new string category = "basic_crafting";

	public Godot.Collections.Array<IngredientPrototype> ingredients = new Godot.Collections.Array<IngredientPrototype>();
	public Godot.Collections.Array<ProductPrototype> products = new Godot.Collections.Array<ProductPrototype>();

	public float time_to_craft = 4.0f;

	public bool unlocked = false;

	public override string ToString()
	{
		return "RecipePrototype " + name;
	}

	public (Godot.Collections.Array<InventoryItem>, int) get_products () { // replace with fluids
		Godot.Collections.Array<InventoryItem> items = new Godot.Collections.Array<InventoryItem>();

		items.Resize(products.Count);

		int index = 0;
		foreach (ProductPrototype product in products) {
			if (product.type == "item") {
				items[index] = InventoryItem.new_item(product.name, product.amount);
			}

			index += 1;
		}

		return (items, 0);
	}
	
	public (Godot.Collections.Array<InventoryItem>, int) get_ingredients () {
		Godot.Collections.Array<InventoryItem> items = new Godot.Collections.Array<InventoryItem>();

		items.Resize(ingredients.Count);
		
		int index = 0;
		foreach (IngredientPrototype ingredient in ingredients) {
			if (ingredient.type == "item") {
				items[index] = InventoryItem.new_item(ingredient.name, ingredient.amount);
			}

			index += 1;
		}

		return (items, 0);
	}

}

public partial class Prototypes : Node {
	public static int max_recipe_ingredients = 8;

	public static Dictionary recipe_category_properties = new Dictionary{
		{"basic_crafting", new Dictionary {
			{"display_name", "Basic Crafting"},
			{"icon_texture", "res://item_textures/test_item.png"}
		}},

	};

	public static Dictionary<string, RecipePrototype> recipes = new Dictionary<string, RecipePrototype> {
		{"test_product", new RecipePrototype{
			name = "test_product",
			display_name = "Test Product",
			category = "basic_crafting",
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