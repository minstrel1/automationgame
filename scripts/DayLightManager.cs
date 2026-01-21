using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class DaylightManager : Node3D {

	private DirectionalLight3D light;

	public override void _Ready () {
		base._Ready();

		light = GetNode<DirectionalLight3D>("DirectionalLight3D");
	}

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);

		float degrees = 360 * WorldTime.Instance.get_day_percentage();

		light.LightEnergy = (float) Mathf.Clamp(Math.Sin((degrees / 360) * 2 * Math.PI + (Math.PI / 2) - 0.1) * 2 + 0.7, -0.5, 0.5) + 0.5f; 

		if (light.LightEnergy < 0.0005) {
			light.Visible = false;
		} else {
			light.Visible = true;
		}

		RotationDegrees = new Vector3(0, 0, degrees);
	}
}