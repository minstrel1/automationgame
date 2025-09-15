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

    public override void _Ready() {
        base._Ready();
    }

     Array<BuildingGridPlacable> placables_in_range = null;

    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);

        switch (current_status) {
            // case HummingbotStatus.perched:
            //     if (current_perch == null) {
            //         return;
            //     }

            //     placables_in_range = current_perch.get_placables();

            //     foreach (BuildingGridPlacable placable in placables_in_range) {
            //         if (placable.current_building_drones.Count == 0) {
            //             current_target = placable;

            //             current_status = HummingbotStatus.waking;

            //             current_unperch_time = 0f;
            //         }
            //     }

            //     break;

            // case HummingbotStatus.waking:
                
                
            //     break;
        }
    }

    public override void release () {
        base.release();
    }
}