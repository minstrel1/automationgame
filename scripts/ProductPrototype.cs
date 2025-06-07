using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class ProductPrototype : GodotObject {
	public virtual string type {set; get;} = "none";
	public virtual string name {set; get;} = "none";
	public virtual int amount {set; get;} = 1;
	public virtual int weight {set; get;} = 1;
}