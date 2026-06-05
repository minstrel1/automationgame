using System;
using System.Data.SqlTypes;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public abstract partial class DroneHome : Node3D {

    public int max_drone_count = 10;

    public Inventory drone_inventory = new Inventory(1);
    public int drone_inventory_size = 1;

    public override void _Ready() {
        base._Ready();

        drone_inventory = new Inventory(drone_inventory_size);
        drone_inventory.set_filter(new ItemCategoryFilter("drone"), 0);
    }

    public virtual void create_drone (string path) {
        
    }

    public virtual void release () {
        base.QueueFree();
    }
   
}