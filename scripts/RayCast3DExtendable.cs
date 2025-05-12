using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class RayCast3DExtendable : RayCast3D {
	
	[Export]
	public float interact_distance = 4.0f;

	public override void _Ready()
	{
		TargetPosition = TargetPosition.Normalized() * interact_distance;
	}

}