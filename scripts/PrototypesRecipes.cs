using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class ItemProductPrototype : ProductPrototype {
	public new string type = "item";
	public string name = "test_item";
	public int amount = 1;

	
}

public partial class RecipePrototype : PrototypeBase {
	public string icon_texture = "res://item_textures/test_item.png";

	public Godot.Collections.Array<ProductPrototype> ingredients = new Godot.Collections.Array<ProductPrototype>();
	public Godot.Collections.Array<ProductPrototype> products = new Godot.Collections.Array<ProductPrototype>();

	public float time_to_craft = 4.0f;

	public bool unlocked = false;

}

public partial class Prototypes : Node {
	public static Dictionary<string, RecipePrototype> recipes = new Dictionary<string, RecipePrototype> {
		
	};
}