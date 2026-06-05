using System;
using System.Data.SqlTypes;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class DronePerch : DroneHome { 

	public bool closest_first = true;
	public bool round_robin = true;

	public int max_drones_per_building = 5;

	public Inventory inventory;
	public int inventory_size = 1;

	public string current_drone_path = "res://drone_scenes/hummingbot.tscn";

	public Array<Hummingbot> working_drones = new Array<Hummingbot>();
	public Array<Hummingbot> available_drones = new Array<Hummingbot>();
	public new int max_drone_count = 20;

    public double range = 10.0f;
    
    public Array<GridEntity> targets = new Array<GridEntity>();
	public Array<HummingbotJobType> target_jobs = new Array<HummingbotJobType>();

    public override void _Ready() {
        base._Ready();

		for (int i = 0; i < max_drone_count; i++) {
			create_drone(current_drone_path);
		}
    }

    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        //GD.Print("bababa");

        calculate_placables_in_range();

		if (targets.Count != 0) {
			if (working_drones.Count < max_drone_count) {
				for (int i = 0; i < targets.Count; i++) {
					GridEntity target = targets[i];
					if (round_robin) {
						if (target.current_building_drones.Count < max_drones_per_building) {
							GD.Print("allocatin drone");
							allocate_drone(target, target_jobs[i]);
						}
					}
				}
			}
		}
    }

	public override void create_drone (String path) {
		PackedScene drone_scene = GD.Load<PackedScene>(path);

		Hummingbot drone = drone_scene.Instantiate<Hummingbot>();

		available_drones.Add(drone);

		drone.current_perch = this;

		AddChild(drone);

		drone.TopLevel = true;
		drone.GlobalPosition = GlobalPosition;
	}

	public void allocate_drone (GridEntity target, HummingbotJobType job_type) {
		if (working_drones.Count < max_drone_count) {
			Hummingbot selected_drone = null;

			foreach (Hummingbot drone in available_drones) {
				if (drone.current_target == null) {
					selected_drone = drone;
					break;
				}
			}

			GD.Print(selected_drone);

			if (selected_drone != null) {
				working_drones.Add(selected_drone);
				selected_drone.current_target = target;
				selected_drone.current_job_type = job_type;
				target.current_building_drones.Add(selected_drone);
			}
		}
	}

	public void gather_drone (Hummingbot drone) {
		if (working_drones.Contains(drone)) {
			if (drone.current_target != null) {
				drone.current_target.current_building_drones.Remove(drone);
			}
			drone.current_target = null;
			drone.current_job_type = HummingbotJobType.none;
			working_drones.Remove(drone);
		}
	}

    public void calculate_placables_in_range () {
        targets.Clear();
		target_jobs.Clear();

        GridEntity target = null;
        foreach (Node node in GetTree().GetNodesInGroup("pre_built_entities")) {
            if (node is GridEntity) {
                target = (GridEntity) node;

				if (target.GlobalPosition.DistanceSquaredTo(GlobalPosition) < (range * range)) {
					if (target.current_building_state == BuildingState.pre_remove) {
						targets.Add(target);
						target_jobs.Add(HummingbotJobType.entity_demolish);
					}
					
				}
            }
        }

		foreach (Node node in GetTree().GetNodesInGroup("pre_built_buildables")) {
			if (node is GridBuildable) {
				target = (GridBuildable) node;

				if (target.GlobalPosition.DistanceSquaredTo(GlobalPosition) < (range * range)) {
					if (target.current_building_state == BuildingState.pre_built) {
						targets.Add(target);
						target_jobs.Add(HummingbotJobType.buildable_build);
					} else if (target.current_building_state == BuildingState.pre_remove) {
						targets.Add(target);
						target_jobs.Add(HummingbotJobType.buildable_demolish);
					}
				}
			}
		}

		targets.OrderBy(thing => thing.GlobalPosition.DistanceTo(GlobalPosition));

		if (!closest_first) {
			targets.Reverse();
		}
    }

    public void register_drone (Hummingbot drone) {
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