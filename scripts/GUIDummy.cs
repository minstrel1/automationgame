using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class GUIDummy : Control {
	
	public virtual string replacable_by {
		get {
			return "GUI";
		}
	}
	
	public override void _Ready()
	{
		base._Ready();
	}
}