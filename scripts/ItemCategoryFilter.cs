using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class ItemCategoryFilter : ItemFilter {

	public ItemCategoryFilter () {
		this.name = "";
		this.invert = false;
	}

	public ItemCategoryFilter (string name, bool invert = false) {
		this.name = name;
		this.invert = invert;
	}

	public override bool match (InventoryItem test) {
		// performance could be fucking terrible on this
		return (Prototypes.items[test.name].category == name) && !invert; 
	}
}