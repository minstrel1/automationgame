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

	public BuildingGridPlacable target;

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

                break;

            case HummingbotStatus.waking:
                
                
                break;
        }
    }

    public override void release () {
        base.release();
    }
}