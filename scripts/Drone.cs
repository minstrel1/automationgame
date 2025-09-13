using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Drone : CharacterBody3D {
    
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