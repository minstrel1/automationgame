using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class FluidContainer : Node {

	public float volume = 100.0f;
	public float current_amount = 0f;

	public string current_fluid = "";
	public string fluid_filter = "";

	public Godot.Collections.Array<FluidSpecialVoxel> connection_points = new Array<FluidSpecialVoxel>();

	public FluidSystem connected_system;

	public bool new_system_this_frame = false;

	public FluidContainer () {
		
	}

	public FluidContainer (float volume) {
		this.volume = volume;
	}

	public bool set_fluid_filter (string fluid_name, bool force = false) {
		if (current_fluid == "" || force) {
			current_fluid = "";
			fluid_filter = fluid_name;
			current_amount = 0f;
			return true;
		}

		return false;
	}

	public float insert (string fluid, float amount) {
		return connected_system.insert(fluid, amount);
	}

	public void release () {
		if (does_removal_split()) {
			GD.Print("this is where we are supposed to split");
			connected_system.split_system(this);
		} else {
			FluidSystem old_system = connected_system;
			old_system.remove_container(this);

			if (current_amount > 0 && current_fluid != "") {
				old_system.insert(current_fluid, current_amount);
			}
		}
	}

	public bool does_removal_split () {
		int connected_ios = 0;

		foreach (FluidSpecialVoxel special_voxel in connection_points) {
			foreach (FluidContainer container in special_voxel.connected_containers.Keys) {
				if (special_voxel.connected_containers[container] == SpecialVoxelFlags.FluidInputOutput) {
					connected_ios += 1;
				}
			}
		}

		return connected_ios > 1;
	}
}