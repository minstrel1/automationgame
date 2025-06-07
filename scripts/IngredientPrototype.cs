using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class IngredientPrototype : GodotObject {
	public string type = "none";
	public string name = "test_item";
	public int amount = 1;
}