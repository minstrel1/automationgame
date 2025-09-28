using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class DronePerch : Node3D {

    public Array<Drone> working_drones = new Array<Drone>();

	public bool closest_first = true;
	public bool round_robin = true;

	public int max_drones_per_building = 3;

	public Inventory inventory;
	public int inventory_size = 1;

    public double range = 10.0f;
    
    public Array<BuildingGridPlacable> placables_to_build = new Array<BuildingGridPlacable>();

    public override void _Ready() {
        base._Ready();

		inventory = new Inventory(inventory_size);
		inventory.set_filter(new ItemCategoryFilter("drone"), 0);

        GD.Print("DINGALING");
        GD.Print(inventory.get_filter(0).GetType().Name);
    }

    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        //GD.Print("bababa");

        calculate_placables_in_range();

		if (placables_to_build.Count != 0) {
            GD.Print("found placable to build");
            foreach (BuildingGridPlacable placable in placables_to_build) {
                if (placable.current_building_drones.Count < max_drones_per_building) {
                    if (inventory.contents[0] != null) {
                        if (inventory.contents[0].prototype.drone_result != null) {

                        } 
                    }
                }
                
            }
		}

		
    }

    public void calculate_placables_in_range () {
        placables_to_build.Clear();

        BuildingGridPlacable placable = null;
        foreach (Node node in GetTree().GetNodesInGroup("pre_built_entities")) {
            if (node is BuildingGridPlacable) {
                placable = (BuildingGridPlacable) node;

				if (placable.GlobalPosition.DistanceSquaredTo(GlobalPosition) < (range * range)) {
                    placables_to_build.Add(placable);
				}
            }
        }

		placables_to_build.OrderBy(thing => thing.GlobalPosition.DistanceTo(GlobalPosition));

		if (!closest_first) {
			placables_to_build.Reverse();
		}
    }

    public void register_drone (Drone drone) {
        if (!working_drones.Contains(drone)) {
            working_drones.Add(drone);
            drone.current_perch = this;
        }
    }

    public virtual void release () {
		inventory.release();

        base.QueueFree();
    }
}