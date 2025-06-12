using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class ItemFilter : FilterBase {

	public string name;
	public bool invert;

	public ItemFilter () {
		this.name = "";
		this.invert = false;
	}

	public ItemFilter (string name, bool invert = false) {
		this.name = name;
		this.invert = invert;

		icon_path = Prototypes.items[name].icon_texture;
	}

	public override bool match (string test) {
		return (test == name) && !invert;
	}

}