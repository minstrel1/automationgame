using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class FluidFilter : FilterBase {

	public string name;
	public bool invert;

	public FluidFilter () {
		this.name = "";
		this.invert = false;
	}

	public FluidFilter (string name, bool invert = false) {
		this.name = name;
		this.invert = invert;

		icon_path = Prototypes.fluids[name].icon_texture;
	}

	public override bool match (string test) {
		return (test == name) && !invert;
	}

}