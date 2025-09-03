using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class DummyMultiFilterRepresentation : GUIDummy {
	
	public override string replacable_by {
		get {
			return "MultiFilterRepresentation";
		}
	}

}
