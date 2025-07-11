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
	public Label voxel_data_label;
	public TextureProgressBar oxygen_meter;

	public PanelContainer interact_label_container;
	public Label interact_label;

	public PanelContainer demolish_count_container;
	public Label demolish_count_label;

	public ProgressBar demolish_bar;

	public void ready_hud () {
		player_hud = GetNode<Control>("PlayerHUD");
		clock_label = GetNode<Label>("PlayerHUD/Control/Label");
		oxygen_label = GetNode<Label>("PlayerHUD/Control2/TextureProgressBar/Label");
		oxygen_meter = GetNode<TextureProgressBar>("PlayerHUD/Control2/TextureProgressBar");
		building_hit_label = GetNode<Label>("PlayerHUD/Control/Label2");
		fps_label = GetNode<Label>("PlayerHUD/Control/Label3");
		voxel_data_label = GetNode<Label>("PlayerHUD/Control/Label4");

		interact_label_container = GetNode<PanelContainer>("PlayerHUD/Control3/PanelContainer");
		interact_label = GetNode<Label>("PlayerHUD/Control3/PanelContainer/Label");

		demolish_count_container = GetNode<PanelContainer>("PlayerHUD/Control3/PanelContainer2");
		demolish_count_label = GetNode<Label>("PlayerHUD/Control3/PanelContainer2/Label");

		demolish_bar = GetNode<ProgressBar>("PlayerHUD/Control3/PanelContainer3/DemolishBar");
	}

	public void update_hud () {

		clock_label.Text = WorldTime.Instance.get_time();

		int hours = oxygen_remaining / (Globals.ticks_per_second * Globals.seconds_per_hour);

		int second_ticks = oxygen_remaining % (Globals.ticks_per_second * Globals.seconds_per_hour);
		int seconds = second_ticks / (Globals.seconds_per_hour);

		oxygen_label.Text = String.Format("{0:D2}:{1:D2}", hours, seconds);
		oxygen_meter.Value = (float) oxygen_remaining / (float) max_oxygen;

		fps_label.Text = "FPS: " + Engine.GetFramesPerSecond().ToString();

		if (is_interact_valid() && !is_in_gui() && current_build_mode == BuildingMode.disabled) {
			interact_label_container.Visible = true;
			interact_label.Visible = true;
			interact_label.Text = "Press E to " + (interact_cast_result as IInteractable).get_interact_text();
		} else {
			interact_label_container.Visible = false;
			interact_label.Visible = false;
		}

		if (!is_in_gui() && current_build_mode == BuildingMode.demolishing) {
			demolish_count_container.Visible = true;
			demolish_count_label.Visible = true;
			demolish_count_label.Text = demolish_targets.Count.ToString() + " / " + max_demolish_targets.ToString();
		} else {
			demolish_count_container.Visible = false;
			demolish_count_label.Visible = false;
		}

		if (current_demolish_time > 0.01) {
			demolish_bar.Visible = true;
			demolish_bar.Value = current_demolish_time;
		} else {
			demolish_bar.Visible = false;
		}

	}
}