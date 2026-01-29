using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public enum DroneStatus {
    resting,
    waking,
    moving_to_target,
    working,
    returning,
    docking,
}

public partial class Drone : CharacterBody3D {
	public GridEntity current_target;

    public double max_flying_speed = 5.0f;
    public double flying_accel = 5.0f;
    public double flying_decel = 10.0f;

    public double time_to_unperch = 2.0f;
    public double unperch_speed = 1.0f;
    public double current_unperch_time = 0.0f;

    public double current_flying_speed = 0f;
    
    public DronePerch current_perch;

    public override void _Ready() {
        base._Ready();
    }

    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
    }

    public virtual void release () {
        base.QueueFree();
    }
}