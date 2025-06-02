using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class PrototypeBase : GodotObject {
	public string name = "thing";
	public string type = "";

	public string display_name = "Thing";
	public string display_description = "This is a thing.";

	public string category = "";
}