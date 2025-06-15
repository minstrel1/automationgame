using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class RecipePrototype : PrototypeBase {
	public string icon_texture = "res://item_textures/test_item.png";

	public new string category = "components";
	public new string subcategory = "basic_crafting";

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