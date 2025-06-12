using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class GrowablePrototype : PrototypeBase {
	
	public string icon_texture = "res://item_textures/test_item.png";

	public float time_to_grow = 1.0f;

	public Godot.Collections.Array<ProductPrototype> harvest_result = new Godot.Collections.Array<ProductPrototype>();

	public (Godot.Collections.Array<InventoryItem>, int) get_products () { // replace with fluids
		Godot.Collections.Array<InventoryItem> items = new Godot.Collections.Array<InventoryItem>();

		items.Resize(harvest_result.Count);

		int index = 0;
		foreach (ProductPrototype product in harvest_result) {
			if (product.type == "item") {
				items[index] = InventoryItem.new_item(product.name, product.amount);
			}

			index += 1;
		}

		return (items, 0);
	}

}