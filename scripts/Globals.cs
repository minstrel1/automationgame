using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Globals : Node {
	public static Globals Instance {get; private set;}

	public static int seconds_per_hour = 240;
	public static int ticks_per_second = 60;

	public static int ighours_to_ticks (float input) {
		return (int) Math.Round(input * seconds_per_hour * ticks_per_second);
	}

	public static float ticks_to_ighours (int input) {
		return (float) Math.Round((double) input / seconds_per_hour / ticks_per_second, 1);
	}

	public static int seconds_to_ticks (float input) {
		return (int) Math.Round(input * ticks_per_second);
	}

	public static float ticks_to_seconds (int input) {
		return (float) Math.Round((double) input / ticks_per_second);
	}

	public override void _Ready()
	{
		Instance = this;
	}
}