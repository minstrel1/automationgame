using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class SubcategoryFilter : FilterBase {

 public Array<string> subcategories = new Array<string>();

	public SubcategoryFilter () {

	}

	public SubcategoryFilter (Array<string> subcategories) {
		this.subcategories = subcategories;

	}

	public override bool match (string test) {
		return subcategories.Contains(test);
	}

}