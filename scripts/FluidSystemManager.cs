using System;
using System.Security.Cryptography.X509Certificates;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class FluidSystemManager : Node {
	public Godot.Collections.Array<FluidSystem> systems = new Array<FluidSystem>();

	public static FluidSystemManager Instance {get; private set;}

	public override void _Ready()
	{
		Instance = this;
	}

	public void add_system (FluidSystem system) {
		if (!systems.Contains(system)) {
			systems.Add(system);
			AddChild(system);
		}
	}

	public void remove_system (FluidSystem system) {
		if (systems.Contains(system)) {
			systems.Remove(system);
			RemoveChild(system);
		}
	}
} 

	