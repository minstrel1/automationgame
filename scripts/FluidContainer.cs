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

	public Dictionary<FluidContainer, SpecialVoxelFlags> connected_outputs = new Dictionary<FluidContainer, SpecialVoxelFlags>();
	public Dictionary<FluidContainer, SpecialVoxelFlags> connected_inputs = new Dictionary<FluidContainer, SpecialVoxelFlags>();

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
}