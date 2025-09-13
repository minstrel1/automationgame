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
    public BuildingGridPlacable current_target;

    public double max_flying_speed = 5.0f;
    public double flying_accel = 5.0f;
    public double flying_decel = 10.0f;

    public double time_to_unperch = 2.0f;
    public double unperch_speed = 1.0f;
    public double current_unperch_time = 0.0f;

    public double current_flying_speed = 0f;

    public HummingbotStatus current_status = HummingbotStatus.perched;

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

                placables_in_range = current_perch.get_placables();

                foreach (BuildingGridPlacable placable in placables_in_range) {
                    if (placable.current_building_drones.Count == 0) {
                        current_target = placable;

                        current_status = HummingbotStatus.waking;

                        current_unperch_time = 0f;
                    }
                }

                break;

            case HummingbotStatus.waking:
                
                
                break;
        }
    }

    public override void release () {
        base.release();
    }
}