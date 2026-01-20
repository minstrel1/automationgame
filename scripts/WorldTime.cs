using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class WorldTime : Node {
	public static WorldTime Instance {get; private set;}

	public long ticks_since_start = 0;

	public int ticks = 0;
	public int seconds = 0;
	public int minutes = 11;
	public bool pm = false;
	public bool pm_changed = false;

	public override void _Ready()
	{
		Instance = this;
	}

	public string get_time () {

		return String.Format("{0:D2}:{1:D2} {2}", minutes + 1, seconds, pm ? "PM" : "AM");
	}

	public float get_day_percentage () {
		return ((float) (ticks_since_start % Globals.get_day_length_ticks())) / Globals.get_day_length_ticks();
	}

	public override void _PhysicsProcess(double delta)
	{
		
		ticks += 1;
		ticks_since_start += 1;

		if (ticks >= Globals.ticks_per_second) {
			ticks = 0;
			seconds += 1;
		}

		if (seconds >= Globals.seconds_per_hour) {
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