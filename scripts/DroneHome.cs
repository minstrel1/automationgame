using System;
using System.Data.SqlTypes;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class DroneHome : Node3D {

    public Array<Drone> available_drones = new Array<Drone>(); 
    public Array<Drone> working_drones = new Array<Drone>();

    public int max_drone_count = 10;

    public Inventory drone_inventory = new Inventory(1);
    public int drone_inventory_size = 1;

    public override void _Ready() {
        base._Ready();

        drone_inventory = new Inventory(drone_inventory_size);
    }

    public virtual void create_drones () {
        
    }

    public virtual void release () {
        base.QueueFree();
    }
   
}