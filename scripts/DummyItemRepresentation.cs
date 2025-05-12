using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class DummyItemRepresentation : GUIDummy {
	
	public override string replacable_by {
		get {
			return "ItemRepresentation";
		}
	}

}