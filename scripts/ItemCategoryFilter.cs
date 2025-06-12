using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class ItemCategoryFilter : FilterBase {

	public string name;
	public bool invert;

	public ItemCategoryFilter () {
		this.name = "";
		this.invert = false;
	}

	public ItemCategoryFilter (string name, bool invert = false) {
		this.name = name;
		this.invert = invert;
	}

	public override bool match (string test) {
		// performance could be fucking terrible on this
		return (Prototypes.items[test].category == name) && !invert; 
	}
}