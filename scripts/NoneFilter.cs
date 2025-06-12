using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class NoneFilter : FilterBase {
	public override bool match (string test) {
		return false;
	}
}

