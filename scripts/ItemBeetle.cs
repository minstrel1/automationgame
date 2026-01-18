using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
public partial class ItemBeetle : CharacterBody3D {

    public BuildingGrid parent_grid;

    public ManhattanAStar3D grid_astar;

    
}