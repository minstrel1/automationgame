using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class FluidSystem : Node {
	public Godot.Collections.Array<FluidContainer> connected_containers = new Array<FluidContainer>();

	public Godot.Collections.Array<FluidContainer> filtered_containers = new Array<FluidContainer>();

	public Dictionary<FluidSystem, int> connected_inputs = new Dictionary<FluidSystem, int>(); 

	public Dictionary<FluidSystem, int> connected_outputs = new Dictionary<FluidSystem, int>(); 

	public string current_fluid = "";
	public string fluid_filter = "";

	public float total_volume = 0;
	public float total_amount = 0;

	public float flow_rate = 600.0f;
	public static float min_flow_rate = 5.0f * (1.0f / 60.0f);

	public float flow_rate_this_frame = 0.0f;

	public float percent_full = 0f;

	public FluidSystem () {
		FluidSystemManager.Instance.add_system(this);
	}

	public float amount_to_remove = 0f;

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);

		for (int i = 0; i < connected_containers.Count; i++) {
			connected_containers[i].new_system_this_frame = false;
		}

		flow_rate_this_frame = 0;

		if (connected_outputs.Count > 0) {
			amount_to_remove = 0;

			foreach (FluidSystem system in connected_outputs.Keys) {
				amount_to_remove = Math.Min(Math.Max(min_flow_rate, (1 - (system.total_amount / system.total_volume)) * flow_rate * (float) delta), total_amount);
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
				calculate_volume();
			} else {
				GD.Print("INCOMPATIBLE");
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
				} else {
					GD.Print("wot");
					return current_fluid == system.current_fluid;
				}
			} else {
				return true;
			}
		}
	}

	public void remove_container (FluidContainer container) {
		if (connected_containers.Contains(container)) {
			connected_containers.Remove(container);
			container.connected_system = null;

			if (filtered_containers.Contains(container)) {
				filtered_containers.Remove(container);

				fluid_filter = null;

				foreach (FluidContainer filtered_container in filtered_containers) {
					if (container.fluid_filter != null) {
						fluid_filter = container.fluid_filter;
					}
				}
			}

			foreach (FluidSpecialVoxel special_voxel in container.connection_points) {
				foreach (FluidContainer target_container in special_voxel.connected_containers.Keys) {
					switch (special_voxel.connected_containers[target_container]) {
						case SpecialVoxelFlags.FluidInput:
							target_container.connected_system.remove_output(this);
							break;

						case SpecialVoxelFlags.FluidOutput:
							remove_output(target_container.connected_system);
							break;
					}
				}
			}

			calculate_volume();
		}
		connected_containers.Remove(container);
	}

	public void split_system (FluidContainer container) {
		if (connected_containers.Contains(container)) {
			connected_containers.Remove(container);
			container.connected_system = null;

			System.Collections.Generic.Queue<FluidContainer> search_queue = new System.Collections.Generic.Queue<FluidContainer>();
			Dictionary<FluidContainer, int> fluid_indices = new Dictionary<FluidContainer, int>();
			Dictionary<int, Array<FluidContainer>> new_systems = new Dictionary<int, Array<FluidContainer>>();
			
			int search_index = 0;

			foreach (FluidSpecialVoxel starting_voxel in container.connection_points) {
				foreach (FluidContainer key_container in starting_voxel.connected_containers.Keys) {
					switch (starting_voxel.connected_containers[key_container]) {
						case SpecialVoxelFlags.FluidInputOutput:
							if (!fluid_indices.ContainsKey(key_container)) {
								fluid_indices[key_container] = search_index;
								new_systems[search_index] = new Array<FluidContainer>{key_container};
								search_queue.Enqueue(key_container);
							}
							break;

						case SpecialVoxelFlags.FluidOutput:
							remove_output(key_container.connected_system);
							break;

					} 
				}
			}

			int iteration_count = 0;

			int current_index;
			FluidContainer current;

			int containers_searched = 0;

			while (search_queue.Count > 0 && iteration_count < 10000) {
				current = search_queue.Dequeue();
				current_index = fluid_indices[current];
				containers_searched += 1;

				foreach (FluidSpecialVoxel starting_voxel in current.connection_points) {
					foreach (FluidContainer key_container in starting_voxel.connected_containers.Keys) {
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

							case SpecialVoxelFlags.FluidOutput:
								break;

						} 
					}
				}
			}
			
			int largest_key = new_systems.Keys.GetEnumerator().Current;
			int largest_count = new_systems[largest_key].Count;

			foreach (int key in new_systems.Keys) {
				if (new_systems[key].Count > largest_count) {
					largest_key = key;
					largest_count = new_systems[largest_key].Count;
				}
			}
			
			new_systems.Remove(largest_key);
			
			calculate_volume();
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

	public void add_output (FluidSystem system) {
		if (system == this) {
			GD.PrintErr("tried outputting to a system with itself.");
			return;
		}

		if (connected_outputs.ContainsKey(system)) {
			connected_outputs[system] += 1;
		} else {
			connected_outputs[system] = 1;
		}

		if (system.connected_inputs.ContainsKey(this)) {
			system.connected_inputs[this] += 1;
		} else {
			system.connected_inputs[this] = 1;
		}

	}

	public void remove_output (FluidSystem system) {
		if (system == this) {
			return;
		}

		if (connected_outputs.ContainsKey(system)) {
			connected_outputs[system] -= 1;
			if (connected_outputs[system] <= 0) {
				connected_outputs.Remove(system);
			}
		}

		if (system.connected_inputs.ContainsKey(this)) {
			system.connected_inputs[this] -= 1;
			if (system.connected_inputs[this] <= 0) {
				system.connected_inputs.Remove(system);
			}
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

	public void merge_system (FluidSystem system) {
		if (system == this) {
			GD.PrintErr("tried merging a system with itself.");
			return;
		}

		if (system.current_fluid != null) {
			current_fluid = system.current_fluid;
		}

		if (system.fluid_filter != null) {
			fluid_filter = system.fluid_filter;

			foreach (FluidContainer container in system.filtered_containers) {
				if (!filtered_containers.Contains(container)) {
					filtered_containers.Add(container);
				}
			}
		}

		foreach (FluidContainer container in system.connected_containers) {
			if (!connected_containers.Contains(container)) {
				connected_containers.Add(container);
				container.connected_system = this;
			}
		}

		foreach (FluidSystem output in system.connected_outputs.Keys) {
			if (output != this) {
				if (connected_outputs.ContainsKey(output)) {
					connected_outputs[output] += system.connected_outputs[output];
				} else {
					connected_outputs[output] = system.connected_outputs[output];
				}
			}
		}

		foreach (FluidSystem input in system.connected_inputs.Keys) {
			if (input != this) {
				if (input.connected_outputs.ContainsKey(system)) {
					if (input.connected_outputs.ContainsKey(this)) {
						input.connected_outputs[this] += input.connected_outputs[system];
					} else {
						input.connected_outputs[this] = input.connected_outputs[system];
					}
					input.connected_outputs.Remove(system);
				}
			}
		}

		calculate_volume();

		system.release();
	} 

	public void release () {
		FluidSystemManager.Instance.remove_system(this);

		QueueFree();
	}
}