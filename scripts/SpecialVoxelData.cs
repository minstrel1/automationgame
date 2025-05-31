using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

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

	public VoxelData placed_voxel_data;
	public Vector3I placed_voxel_pos;
	public BuildingGrid parent_grid;

	public Array<SpecialVoxel> connected_voxels;

	public bool connections_updated_this_frame = false;

	public void on_property_changed () {
		
		flag_directions = (BuildDirectionFlags) FlagDirections;
		support_directions = (BuildDirectionFlags) SupportDirections;

		//GD.Print(parent);

		if (parent != null) {
			parent.make_visualiser_mesh();
		}
	}

	
}