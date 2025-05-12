using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Globals : Node {
	public static Globals Instance {get; private set;}

	public static int seconds_per_hour = 90;
	public static int ticks_per_second = 60;

	public static int hours_to_ticks (float input) {
		return (int)Math.Round(input * seconds_per_hour * ticks_per_second);
	}

	public override void _Ready()
	{
		Instance = this;
	}
}