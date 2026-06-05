using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
[Tool]
public partial class GridBuildable : GridEntity {

	[ExportCategory("Grid Buildable")]
	[Export]
	public float target_building_time = 5.0f;
	public double current_building_time = 0.0f;

	[Export]
	public Godot.Collections.Array building_cost = new Godot.Collections.Array{
		new Dictionary{
			{"type", "item"},
			{"name", "test_item"},
			{"amount", 5},
		}
	};

	public override void on_place(bool update = true) {
		current_building_state = BuildingState.pre_built;

		calculate_open_adjacent_cells();
		
		if (update) {
			foreach (BuildingGridChunk chunk in occupied_chunks) {
				chunk.on_chunk_changed();
			}
		}

		AddToGroup("pre_built_buildables");
	}

	public virtual void on_build () {
		current_building_state = BuildingState.built;

		RemoveFromGroup("pre_built_buildables");

		foreach (string name in special_voxels.Keys) {
			special_voxels[name].on_build();
		}

		foreach (BuildingGridChunk chunk in occupied_chunks) {
			chunk.on_chunk_changed();
		}
	}

	public override void mark_for_demolishing() {
		current_building_state = BuildingState.pre_remove;

		AddToGroup("pre_remove_buildables");
	}

}
