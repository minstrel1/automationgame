using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class FilterBase : Node {

	public FilterBase () {

	}

	public virtual bool match (string test) {
		return true;
	}
}