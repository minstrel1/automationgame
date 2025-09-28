using System;
using System.Numerics;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Player {
	public Array<Drone> player_drones = new Array<Drone>();

    public DronePerch player_perch;

    public void ready_drone () {
        player_perch = GetNode<DronePerch>("CollisionShape3D/DronePerch");
    }
}