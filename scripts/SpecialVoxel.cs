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
public partial class SpecialVoxel : Node3D {
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
		
		update_flags();

		//GD.Print(parent);

		if (parent != null) {
			parent.make_visualiser_mesh();
		}
	}

	public void update_flags () {
		flag_directions = (BuildDirectionFlags) FlagDirections;
		support_directions = (BuildDirectionFlags) SupportDirections;
	}

	public virtual void update_voxel_connections () {

		if (connections_updated_this_frame) {
			return;
		}

		connected_voxels = new Array<SpecialVoxel>();

		Array<BuildDirection> directions = Tools.flags_to_enum(placed_voxel_data.special_directions);

		foreach (BuildDirection direction in directions) {
			Vector3 normal = Tools.enum_to_normal(direction);
			Vector3I test_pos = Tools.v3_to_v3I(placed_voxel_pos + normal);

			BuildDirection opposite = Tools.get_enum_opposite(direction);

			if (parent_grid.is_position_valid(test_pos)) {
				VoxelData test_data = parent_grid.get_block(test_pos);

				if (test_data.is_special_voxel) {
					if (Tools.is_special_compatible(voxel_flags, test_data.voxel_flags)) {
						if (Tools.flags_to_enum(test_data.special_voxel.flag_directions).Contains(opposite)) {
							connected_voxels.Add(test_data.special_voxel);
						}
					}
				}
			}
		}

		GD.Print(name + " connected");
		GD.Print(connected_voxels);
		connections_updated_this_frame = true;
	}

	public virtual void update () {
		connections_updated_this_frame = false;
	}

	public virtual void on_build () {
		
	}
}