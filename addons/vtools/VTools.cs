#if TOOLS
using Godot;
using System;

[Tool]
public partial class VTools : EditorPlugin
{

	public SpecialVoxel special_voxel_instance;

	public override void _EnterTree()
	{
		special_voxel_instance = new SpecialVoxel ();
		AddInspectorPlugin(special_voxel_instance);
	}

	public override void _ExitTree()
	{
		RemoveInspectorPlugin(special_voxel_instance);
	}
}
#endif
