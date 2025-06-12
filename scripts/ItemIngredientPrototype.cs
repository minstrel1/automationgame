using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class ItemIngredientPrototype : IngredientPrototype {
	public override string type {set; get;} = "item";
	public override string name {set; get;} = "test_item";
	public override int amount {set; get;}= 1;
}