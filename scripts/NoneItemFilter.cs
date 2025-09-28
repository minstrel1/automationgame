using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class NoneItemFilter : ItemFilter {
	public override bool match (InventoryItem test) {
		return false;
	}
}

