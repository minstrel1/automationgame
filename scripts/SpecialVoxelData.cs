using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public enum BuildDirection {
	Left,
	Right,
	Up,
	Down,
	Forward,
	Back,
}

[Flags]
public enum BuildDirectionFlags {
	Any = 1 << 0,
	Left = 1 << 1,
	Right = 1 << 2,
	Up = 1 << 3,
	Down = 1 << 4,
	Forward = 1 << 5,
	Back = 1 << 6,
}

public enum SpecialVoxelFlags {
	None,
	ItemInput,
	ItemOutput,
	ItemInputOutput,
	FluidInput,
	FluidOutput,
	FluidInputOutput,
}

[Tool]
[GlobalClass]
public partial class SpecialVoxelData : Resource {
	[Export]
	public string name = "voxel";
	[Export]
	public Vector3I voxel_position {get; set {field = value; on_property_changed();}} 
	[Export]
	public SpecialVoxelFlags voxel_flags;
	
	[Export(PropertyHint.Layers2DPhysics)]
	public int FlagDirections {get; set {field = value; on_property_changed();}} = 1 << 0;
	public BuildDirectionFlags flag_directions  = BuildDirectionFlags.Any;

	[Export(PropertyHint.Layers2DPhysics)]
	public int SupportDirections {get; set {field = value; on_property_changed();}} = 0;
	public BuildDirectionFlags support_directions = 0;

	public Vector3 parent_center {get; set {field = value; on_property_changed();}} 
	public BuildingGridPlacable parent;

	public void on_property_changed () {
		
		flag_directions = (BuildDirectionFlags) FlagDirections;
		support_directions = (BuildDirectionFlags) SupportDirections;

		//GD.Print(parent);

		if (parent != null) {
			parent.make_visualiser_mesh();
		}
	}
}