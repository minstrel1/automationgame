using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class FluidSystem : Node {
	public Godot.Collections.Array<FluidContainer> connected_containers = new Array<FluidContainer>();

	public Godot.Collections.Array<FluidContainer> filtered_containers = new Array<FluidContainer>();

	public Godot.Collections.Array<FluidSystem> connected_outputs = new Array<FluidSystem>(); // EXCLUSIVELY FOR OUTPUTTING

	public string current_fluid = "";
	public string fluid_filter = "";

	public float total_volume = 0;
	public float total_amount = 0;

	public FluidSystem () {
		FluidSystemManager.Instance.add_system(this);
	}

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);

		foreach (FluidContainer container in connected_containers) {
			container.new_system_this_frame = false;
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
		float percent_full = total_amount / total_volume;
		
		foreach (FluidContainer container in connected_containers) {
			container.current_amount = container.volume * percent_full;
		}

		GD.Print(String.Format("{0} has volume {1} and amount {2}", Name, total_volume, total_amount));
	}

	public void update_amounts () {
		float percent_full = total_amount / total_volume;

		foreach (FluidContainer container in connected_containers) {
			container.current_amount = container.volume * percent_full;
		}
	}

	public void add_output (FluidSystem system) {
		if (system == this) {
			GD.PrintErr("tried outputting to a system with itself.");
			return;
		}

		if (!connected_outputs.Contains(system)) {
			connected_outputs.Add(system);
		}
	}

	public void merge_system (FluidSystem system) {
		if (system == this) {
			GD.PrintErr("tried merging a system with itself.");
			return;
		}

		foreach (FluidContainer container in system.connected_containers) {
			if (!connected_containers.Contains(container)) {
				connected_containers.Add(container);
				container.connected_system = this;
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