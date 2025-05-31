// using System;
// using System.ComponentModel;
// using System.Diagnostics.CodeAnalysis;
// using System.Drawing;
// using Godot;
// using Godot.Collections;
// using Godot.NativeInterop;

// public partial class VoxelInterface : Node {

// 	public BuildingGridPlacable parent_placable;
// 	public BuildingGrid parent_grid;

// 	public SpecialVoxel special_voxel;

// 	public static VoxelInterface make (SpecialVoxel special_voxel) {
// 		VoxelInterface new_instance = new VoxelInterface();

// 		new_instance.special_voxel = special_voxel;
		

// 		return new_instance;
// 	}

// 	public virtual void update_connections () {

// 	}
// }