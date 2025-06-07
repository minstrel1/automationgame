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
	public new string type = "item";
}

public partial class RecipePrototype : PrototypeBase {
	public string icon_texture = "res://item_textures/test_item.png";

	public new string category = "basic_crafting";

	public Godot.Collections.Array<IngredientPrototype> ingredients = new Godot.Collections.Array<IngredientPrototype>();
	public Godot.Collections.Array<ProductPrototype> products = new Godot.Collections.Array<ProductPrototype>();

	public float time_to_craft = 4.0f;

	public bool unlocked = false;

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
	
	public Godot.Collections.Array get_ingredients () {
		Godot.Collections.Array result = new Godot.Collections.Array();

		foreach (IngredientPrototype ingredient in ingredients) {
			if (ingredient.type == "item") {
				result.Add(InventoryItem.new_item(ingredient.name, ingredient.amount));
			}
		}

		return result;
	}

}

public partial class Prototypes : Node {
	public static Dictionary<string, RecipePrototype> recipes = new Dictionary<string, RecipePrototype> {
		{"test_product", new RecipePrototype{
			name = "test_product",
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