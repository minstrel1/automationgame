using System;
using Godot;

[Tool]
public partial class SpecialVoxelEditor : Control {

	public PackedScene voxel_node = GD.Load<PackedScene>("res://addons/vtools/special_voxel_editor_node.tscn");

}