using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Player {

	public int oxygen_remaining = Globals.ighours_to_ticks(6.0f);

	public int max_oxygen = Globals.ighours_to_ticks(6.0f);

	public void update_stats () {

		oxygen_remaining -= 1;

		oxygen_remaining = Math.Clamp(oxygen_remaining, 0, max_oxygen);

	}

}