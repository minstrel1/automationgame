using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class DronePerch : Node3D {

    public Array<Drone> drones = new Array<Drone>();

    public double range = 10.0f;
    
    public Array<BuildingGridPlacable> placables_to_build = new Array<BuildingGridPlacable>();
    public bool placables_calculated_this_frame = false;

    public override void _Ready() {
        base._Ready();
    }

    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);

        placables_calculated_this_frame = false;
    }

    public Array<BuildingGridPlacable> get_placables () {
        if (!placables_calculated_this_frame) {
            calculate_placables_in_range();
        }

        return placables_to_build;
    }

    public void calculate_placables_in_range () {
        placables_calculated_this_frame = true;

        placables_to_build.Clear();

        BuildingGridPlacable placable = null;
        foreach (Node node in GetTree().GetNodesInGroup("pre_built_entities")) {
            if (node is BuildingGridPlacable) {
                placable = (BuildingGridPlacable) node;

                placables_to_build.Add(placable);
            }
        }
    }

    public void register_drone (Drone drone) {
        if (!drones.Contains(drone)) {
            drones.Add(drone);
            drone.current_perch = this;
        }
    }

    public virtual void release () {
        base.QueueFree();
    }
}