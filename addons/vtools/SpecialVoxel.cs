#if TOOLS
using Godot;
using System;

[Tool]
public partial class SpecialVoxel : EditorInspectorPlugin {

	public PackedScene test_scene = GD.Load<PackedScene>("res://addons/vtools/special_voxel_editor_node.tscn");
	public PackedScene editor = GD.Load<PackedScene>("res://addons/vtools/special_voxel_editor.tscn");
	public override bool _CanHandle(GodotObject @object)
    {
        return @object is Node3D;
    }

    public override bool _ParseProperty(GodotObject @object, Variant.Type type, string name, PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
    {
		//GD.Print(name);
        // We handle properties of type integer.
        if (type == Variant.Type.Int)
        {
            // Create an instance of the custom property editor and register
            // it to a specific property path.
            // Inform the editor to remove the default property editor for
            // this property type.
        }

        return false;
    }

	public override void _ParseCategory(GodotObject @object, String category) {

		//GD.Print(category);

		if (category == "BuildingGridPlacable" && @object is BuildingGridPlacable) {
			Control new_editor = editor.Instantiate<Control>();
			AddCustomControl(new_editor);

			Control new_label = test_scene.Instantiate<Control>();
			AddCustomControl(new_label);
			
		}
		
	}
}
#endif
