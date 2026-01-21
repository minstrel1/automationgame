using System;
using System.Data.SqlTypes;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class DronePerch : Node3D { 

	public bool closest_first = true;
	public bool round_robin = true;

	public int max_drones_per_building = 5;

	public Inventory inventory;
	public int inventory_size = 1;

	public string current_drone_path = "res://drone_scenes/hummingbot.tscn";

	public Array<Drone> working_drones = new Array<Drone>();
	public Array<Drone> available_drones = new Array<Drone>();
	public int max_drone_count = 20;

    public double range = 10.0f;
    
    public Array<BuildingGridPlacable> placables_to_build = new Array<BuildingGridPlacable>();

    public override void _Ready() {
        base._Ready();

		inventory = new Inventory(inventory_size);
		inventory.set_filter(new ItemCategoryFilter("drone"), 0);

		for (int i = 0; i < max_drone_count; i++) {
			create_drone(current_drone_path);
		}
    }

    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        //GD.Print("bababa");

        calculate_placables_in_range();

		if (placables_to_build.Count != 0) {
			if (working_drones.Count < max_drone_count) {
				foreach (BuildingGridPlacable placable in placables_to_build) {
					if (round_robin) {
						if (placable.current_building_drones.Count < max_drones_per_building) {
							GD.Print("allocatin drone");
							allocate_drone(placable);
						}
					}
				}
			}
		}
    }

	public void create_drone (String path) {
		PackedScene drone_scene = GD.Load<PackedScene>(path);

		Drone drone = drone_scene.Instantiate<Drone>();

		available_drones.Add(drone);

		drone.current_perch = this;

		AddChild(drone);

		drone.TopLevel = true;
		drone.GlobalPosition = GlobalPosition;
	}

	public void allocate_drone (BuildingGridPlacable target) {
		if (working_drones.Count < max_drone_count) {
			Drone selected_drone = null;

			foreach (Drone drone in available_drones) {
				if (drone.current_target == null) {
					selected_drone = drone;
					break;
				}
			}

			GD.Print(selected_drone);

			if (selected_drone != null) {
				working_drones.Add(selected_drone);
				selected_drone.current_target = target;
				target.current_building_drones.Add(selected_drone);
			}
		}
	}

	public void gather_drone (Drone drone) {
		if (working_drones.Contains(drone)) {
			if (drone.current_target != null) {
				drone.current_target.current_building_drones.Remove(drone);
			}
			drone.current_target = null;
			working_drones.Remove(drone);
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