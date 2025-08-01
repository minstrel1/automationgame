using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class FluidSystem : Node {
	public Godot.Collections.Array<FluidContainer> connected_containers = new Array<FluidContainer>();

	public Godot.Collections.Array<FluidContainer> filtered_containers = new Array<FluidContainer>();

	public Array<FluidContainer> connected_outputs = new Array<FluidContainer>();
	public Array<float> connected_outputs_flow_rates = new Array<float>();

	public string current_fluid = "";
	public string fluid_filter = "";

	public float total_volume = 0;
	public float total_amount = 0;

	public float flow_rate = 600.0f;
	public static float min_flow_rate = 5.0f * (1.0f / 60.0f);

	public float flow_rate_this_frame = 0.0f;

	public float percent_full = 0f;

	public bool outputs_recalculated_this_frame = false;
	public bool outputs_recalculated_queued_this_frame = false;

	public FluidSystem () {
		FluidSystemManager.Instance.add_system(this);
	}

	public float amount_to_remove = 0f;

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);

		outputs_recalculated_this_frame = false;
		outputs_recalculated_queued_this_frame = false;

		// for (int i = 0; i < connected_containers.Count; i++) {
		// 	connected_containers[i].new_system_this_frame = false;
		// }

		flow_rate_this_frame = 0;

		if (connected_outputs.Count > 0) {
			amount_to_remove = 0;

			for (int i = 0; i < connected_outputs.Count; i++) {
				FluidContainer container = connected_outputs[i];
				float connection_flow_rate = connected_outputs_flow_rates[i];
				if (container == null) {
					GD.Print("we got a null container");
					GD.Print(connected_outputs);
					queue_recalculate_outputs();
					continue;
				}
				
				if (container.connected_system == null) {
					GD.Print("we got a container with no system??");
					queue_recalculate_outputs();
					continue;
				}

				FluidSystem system = container.connected_system;

				amount_to_remove = Math.Min(Math.Max(min_flow_rate, (1 - (system.total_amount / system.total_volume)) * connection_flow_rate * (float) delta), total_amount);
				if (system != null) {
					amount_to_remove = system.insert(current_fluid, amount_to_remove);
					flow_rate_this_frame += amount_to_remove;
					total_amount -= amount_to_remove;
				} else {
					GD.Print("WHAT???");
				}
				
			}

			if (total_amount < 0.000000) {
				total_amount = 0;
				current_fluid = "";
			}

			update_amounts();
		}
		//GD.Print("this is a system?");
	}

	public void add_container (FluidContainer container) {
		if (!connected_containers.Contains(container)) {
			if (is_container_compatible(container)) {
				if (container.fluid_filter != "") {
					if (fluid_filter == "") {
						fluid_filter = container.fluid_filter;
					}

					filtered_containers.Add(container);
				}

				if (current_fluid == "" && container.current_fluid != "") {
					current_fluid = container.current_fluid;
				}

				connected_containers.Add(container);
				container.connected_system = this;
				queue_recalculate_outputs();
				calculate_volume();
			} else {
				GD.Print("INCOMPATIBLE");
			}
		}
	}

	public void remove_container (FluidContainer container) {
		if (connected_containers.Contains(container)) {
			connected_containers.Remove(container);
			container.connected_system = null;

			//GD.Print("removing container");

			if (filtered_containers.Contains(container)) {
				filtered_containers.Remove(container);

				fluid_filter = null;

				foreach (FluidContainer filtered_container in filtered_containers) {
					if (container.fluid_filter != null) {
						fluid_filter = container.fluid_filter;
					}
				}
			}

			if (connected_containers.Count == 0) {
				this.release();
			} else {
				queue_recalculate_outputs();
				calculate_volume();
			}
		}

		//GD.Print(Name + " has " + connected_containers.Count + " containers left");
	}

	public void queue_recalculate_outputs () {
		if (!outputs_recalculated_queued_this_frame) {
			CallDeferred(MethodName.recalculate_outputs);
			outputs_recalculated_queued_this_frame = true;
		}
	}


	public void recalculate_outputs () {
		if (outputs_recalculated_this_frame) {
			return;
		}

		outputs_recalculated_this_frame = true;

		connected_outputs.Clear();
		connected_outputs_flow_rates.Clear();

		foreach (FluidContainer container in connected_containers) {
			foreach (FluidSpecialVoxel special_voxel in container.connection_points) {
				foreach (FluidContainer connected_container in special_voxel.connected_containers.Keys) {
					if (connected_container != null) {
						if (special_voxel.connected_containers[connected_container] == SpecialVoxelFlags.FluidOutput) {
							if (connected_container.connected_system != null && is_system_compatible(connected_container.connected_system)) {
								connected_outputs.Add(connected_container);
								connected_outputs_flow_rates.Add(special_voxel.flow_rate);
							}
						} else if (special_voxel.connected_containers[connected_container] == SpecialVoxelFlags.FluidInput) {
							if (connected_container.connected_system != null) {
								connected_container.connected_system.queue_recalculate_outputs();
							}
						}
					}
				}
			}
		}
	}

	public bool is_container_compatible (FluidContainer container) {
		if (container.fluid_filter != "") {
			if (fluid_filter == "") {
				if (container.current_fluid == "") {
					GD.Print("what");
					return true;
				} else {
					GD.Print("huh");
					return current_fluid == "";
				}
			} else {
				GD.Print("who");
				return container.fluid_filter == fluid_filter;
			} 
		} else {
			if (container.current_fluid != "") {
				if (fluid_filter != "") {
					GD.Print("whence");
					return fluid_filter == container.current_fluid;
				} else if (current_fluid == "") {
					return true;
				} else {
					GD.Print("wot");
					return current_fluid == container.current_fluid;
				}
			} else {
				return true;
			}
		}
	}

	public bool is_system_compatible (FluidSystem system) {
		if (system == this) {
			return false;
		}

		if (system.fluid_filter != "") {
			if (fluid_filter == "") {
				if (system.current_fluid == "") {
					GD.Print("what");
					return true;
				} else {
					GD.Print("huh");
					return current_fluid == "";
				}
			} else {
				GD.Print("who");
				return system.fluid_filter == fluid_filter;
			} 
		} else {
			if (system.current_fluid != "") {
				if (fluid_filter != "") {
					GD.Print("whence");
					return fluid_filter == system.current_fluid;
				} else if (current_fluid == "") {
					return true;
				} else {
					GD.Print("wot");
					return current_fluid == system.current_fluid;
				}
			} else {
				return true;
			}
		}
	}

	
	public void split_system (FluidContainer container) {
		if (connected_containers.Contains(container)) {
			GD.Print("SPLITTING SYSTEM!");

			Array<FluidSpecialVoxel> visited_voxels = new Array<FluidSpecialVoxel>();

			System.Collections.Generic.Queue<FluidContainer> search_queue = new System.Collections.Generic.Queue<FluidContainer>();
			Dictionary<FluidContainer, int> fluid_indices = new Dictionary<FluidContainer, int>();
			Dictionary<int, Array<FluidContainer>> new_systems = new Dictionary<int, Array<FluidContainer>>();
			
			int search_index = 0;

			GD.Print("starting_voxels " + container.connection_points.Count);

			foreach (FluidSpecialVoxel starting_voxel in container.connection_points) {
				starting_voxel.force_update = true;
				visited_voxels.Add(starting_voxel);
				GD.Print("connected_containers " + starting_voxel.connected_containers.Keys.Count);
				foreach (FluidContainer key_container in starting_voxel.connected_containers.Keys) {
					switch (starting_voxel.connected_containers[key_container]) {
						case SpecialVoxelFlags.FluidInputOutput:
							if (!fluid_indices.ContainsKey(key_container)) {
								fluid_indices[key_container] = search_index;
								new_systems[search_index] = new Array<FluidContainer>{key_container};
								search_queue.Enqueue(key_container);

								search_index += 1;
							}
							break;
					} 
				}
			}

			GD.Print("new systems");
			GD.Print(new_systems);

			remove_container(container);

			int iteration_count = 0;

			int current_index;
			FluidContainer current;

			int containers_searched = 0;

			while (search_queue.Count > 0 && iteration_count < 10000) {
				current = search_queue.Dequeue();
				current_index = fluid_indices[current];
				containers_searched += 1;

				foreach (FluidSpecialVoxel starting_voxel in current.connection_points) {
					//starting_voxel.force_update = true;
					visited_voxels.Add(starting_voxel);
					foreach (FluidContainer key_container in starting_voxel.connected_containers.Keys) {
						if (key_container == container) {
							continue;
						}
						switch (starting_voxel.connected_containers[key_container]) {
							case SpecialVoxelFlags.FluidInputOutput:
								if (fluid_indices.ContainsKey(key_container)) {
									if (fluid_indices[key_container] != current_index) {
										int old_index = fluid_indices[key_container];

										foreach (FluidContainer old_container in new_systems[old_index]) {
											fluid_indices[old_container] = current_index;
											new_systems[current_index].Add(old_container);
										}

										new_systems.Remove(old_index);
									}

								} else {
									fluid_indices[key_container] = current_index;
									new_systems[current_index].Add(key_container);
									search_queue.Enqueue(key_container);
								}
								break;
						} 
					}
				}
			}

			GD.Print("new systems post search");
			GD.Print(new_systems);
			
			int largest_key = -1;
			int largest_count = -99999;

			foreach (int key in new_systems.Keys) {
				if (new_systems[key].Count > largest_count) {
					largest_key = key;
					largest_count = new_systems[largest_key].Count;
				}
			}
			
			new_systems.Remove(largest_key);

			GD.Print("new systems post largest");
			GD.Print(new_systems);

			foreach (int key in new_systems.Keys) {
				FluidSystem new_system = new FluidSystem();

				foreach(FluidContainer transplant_container in new_systems[key]) {
					remove_container(transplant_container);

					new_system.add_container(transplant_container);
					transplant_container.connected_system = new_system;
				}

				new_system.calculate_volume();
			}
			
			calculate_volume();

			// foreach (FluidSpecialVoxel visited_voxel in visited_voxels) {
			// 	visited_voxel.update_voxel_connections();
			// }
		}

		if (connected_containers.Count == 0) {
			this.release();
		}
	}

	public void calculate_volume () {
		float new_volume = 0;
		float new_amount = 0;

		foreach (FluidContainer container in connected_containers) {
			new_volume += container.volume;
			new_amount += container.current_amount;
		}

		total_volume = new_volume;
		total_amount = Math.Min(total_volume, new_amount);
		update_amounts();

		GD.Print(String.Format("{0} has volume {1} and amount {2}", Name, total_volume, total_amount));
	}

	public void update_amounts () {
		percent_full = total_amount / total_volume;

		foreach (FluidContainer container in connected_containers) {
			container.current_amount = container.volume * percent_full;
			container.current_fluid = current_fluid;
		}
	}

	public float insert_difference;

	public float insert (string fluid, float amount) {
		if (current_fluid == "") {
			current_fluid = fluid;
			insert_difference = Math.Min(total_volume - total_amount, amount);
			total_amount += insert_difference;
			update_amounts();
			return insert_difference;
		} else if (current_fluid == fluid) {
			insert_difference = Math.Min(total_volume - total_amount, amount);
			total_amount += insert_difference;
			update_amounts();
			return insert_difference;
		}

		return 0;
	}

	public float remove (string fluid, float amount) {
		if (current_fluid == fluid) {
			float amount_to_remove = Math.Min(amount, total_amount);
			total_amount -= amount_to_remove;
			update_amounts();
			return amount_to_remove;
		}

		return 0;
	}

	public void merge_system (FluidSystem system) {
		if (system == this) {
			GD.PrintErr("tried merging a system with itself.");
			return;
		}
		GD.Print("merging like a mansion?");

		Array<FluidSpecialVoxel> visited_voxels = new Array<FluidSpecialVoxel>();

		FluidContainer container;
		while (system.connected_containers.Count > 0) {
			container = system.connected_containers[0];
			system.remove_container(container);

			add_container(container);
			container.connected_system = this;

			foreach (FluidSpecialVoxel starting_voxel in container.connection_points) {
				starting_voxel.force_update = true;
				visited_voxels.Add(starting_voxel);
			}
		}

		calculate_volume();

		foreach (FluidSpecialVoxel visited_voxel in visited_voxels) {
			visited_voxel.update_voxel_connections();
		}

		queue_recalculate_outputs();

		system.release();
	} 

	public void release () {
		FluidSystemManager.Instance.remove_system(this);

		QueueFree();
	}
}