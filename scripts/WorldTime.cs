using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class WorldTime : Node {
	public static WorldTime Instance {get; private set;}

	public int ticks = 0;
	public int seconds = 0;
	public int minutes = 10;
	public bool pm = false;
	public bool pm_changed = false;

	public override void _Ready()
	{
		Instance = this;
	}

	public string get_time () {

		return String.Format("{0:D2}:{1:D2} {2}", minutes + 1, seconds, pm ? "PM" : "AM");
	}

	public override void _PhysicsProcess(double delta)
	{
		
		ticks += 1;

		if (ticks >= Globals.seconds_per_hour) {
			ticks = 0;
			seconds += 1;
		}

		if (seconds >= 60) {
			seconds = 0;
			minutes += 1;
		}

		if (minutes == 11 && !pm_changed) {
			pm = !pm;
			pm_changed = true;
		}

		if (minutes >= 12) {
			minutes = 0;
			pm_changed = false;
		}
	}
}