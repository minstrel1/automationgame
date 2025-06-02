using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Prototypes : Node {
	public static Prototypes Instance {get; private set;}

	public override void _Ready()
	{
		Instance = this;

		create_building_dict();
	}
}