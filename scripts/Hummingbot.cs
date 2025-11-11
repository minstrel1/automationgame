using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public enum HummingbotStatus {
	perched,
	waking,
	travelling_to_target,
	building,
	travelling_to_home,
	docking,
}

public partial class Hummingbot : Drone {

	public HummingbotStatus current_status = HummingbotStatus.perched;

	private double wake_time = 0.0f;
	private double target_wake_time = 2.0f;
	private float wake_rise_distance = 2.0f;
	private Vector3 wake_start_pos = Vector3.Zero;

	

	public override void _Ready() {
		base._Ready();
	}

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);

		switch (current_status) {
			case HummingbotStatus.perched:
				if (current_perch == null) {
					return;
				}

				GlobalPosition = current_perch.GlobalPosition;
				GlobalBasis = current_perch.GlobalBasis;

				if (current_target != null) {
					current_status = HummingbotStatus.waking;
					GD.Print("got a target");
				}

				break;

			case HummingbotStatus.waking:
				if (current_perch == null) {
					return;
				}

				wake_time += delta;

				if (wake_time < (target_wake_time / 2)) {
					GlobalPosition = current_perch.GlobalPosition + (new Vector3(0,wake_rise_distance / 2f,0) * (float) wake_time);
					GlobalBasis = current_perch.GlobalBasis;
					wake_start_pos = current_perch.GlobalPosition;
				} else {
					GlobalPosition = wake_start_pos + (new Vector3(0,wake_rise_distance / 2f,0) * (float) wake_time);
				}

				if (wake_time >= target_wake_time) {
					current_status = HummingbotStatus.travelling_to_target;
				}
				
				break;

			case HummingbotStatus.travelling_to_target:
				break;
		}
	}

	public override void release () {
		base.release();
	}
}
