using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Player {

	public Control player_hud;
	public Label clock_label;
	public Label oxygen_label;
	public Label building_hit_label;
	public Label fps_label;
	public TextureProgressBar oxygen_meter;

	public PanelContainer interact_label_container;
	public Label interact_label;

	public void ready_hud () {
		player_hud = GetNode<Control>("PlayerHUD");
		clock_label = GetNode<Label>("PlayerHUD/Control/Label");
		oxygen_label = GetNode<Label>("PlayerHUD/Control2/TextureProgressBar/Label");
		oxygen_meter = GetNode<TextureProgressBar>("PlayerHUD/Control2/TextureProgressBar");
		building_hit_label = GetNode<Label>("PlayerHUD/Control/Label2");
		fps_label = GetNode<Label>("PlayerHUD/Control/Label3");

		interact_label_container = GetNode<PanelContainer>("PlayerHUD/Control3/PanelContainer");
		interact_label = GetNode<Label>("PlayerHUD/Control3/PanelContainer/Label");
	}

	public void update_hud () {

		clock_label.Text = WorldTime.Instance.get_time();

		int hours = oxygen_remaining / (Globals.ticks_per_second * Globals.seconds_per_hour);

		int second_ticks = oxygen_remaining % (Globals.ticks_per_second * Globals.seconds_per_hour);
		int seconds = second_ticks / (Globals.seconds_per_hour);

		oxygen_label.Text = String.Format("{0:D2}:{1:D2}", hours, seconds);
		oxygen_meter.Value = (float) oxygen_remaining / (float) max_oxygen;

		fps_label.Text = "FPS: " + Engine.GetFramesPerSecond().ToString();

		if (is_interact_valid() && !is_in_gui()) {
			interact_label_container.Visible = true;
			interact_label.Text = "Press E to " + (interact_cast_result as IInteractable).get_interact_text();
		} else {
			interact_label_container.Visible = false;
		}

	}
}