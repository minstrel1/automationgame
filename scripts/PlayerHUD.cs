using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Player {

	public Control player_hud;
	public Label clock_label;
	public Label oxygen_label;
	public Label building_hit_label;
	public TextureProgressBar oxygen_meter;

	public void update_hud () {

		clock_label.Text = WorldTime.Instance.get_time();

		int hours = oxygen_remaining / (Globals.ticks_per_second * Globals.seconds_per_hour);

		int second_ticks = oxygen_remaining % (Globals.ticks_per_second * Globals.seconds_per_hour);
		int seconds = second_ticks / (Globals.seconds_per_hour);

		oxygen_label.Text = String.Format("{0:D2}:{1:D2}", hours, seconds);
		oxygen_meter.Value = (float) oxygen_remaining / (float) max_oxygen;

	}
}