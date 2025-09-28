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

	public HummingbotStatus current_status = HummingbotStatus.waking;

	public override void _Ready() {
		base._Ready();
	}

	 Array<BuildingGridPlacable> placables_in_range = null;

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);

		switch (current_status) {
			case HummingbotStatus.perched:
				if (current_perch == null) {
					return;
				}

				break;

			case HummingbotStatus.waking:
				current_status = HummingbotStatus.travelling_to_target;
				
				break;

			case HummingbotStatus.travelling_to_target:
				GlobalPosition = current_target.GlobalPosition;
				break;

			case HummingbotStatus.building:
				
				break;
		}
	}

	public override void release () {
		base.release();
	}
}
