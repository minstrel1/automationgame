using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Player {

	public RayCast3DExtendable building_raycaster;
	public Vector3 last_building_cast_position;
	public GodotObject building_cast_result;
	public BuildingGrid last_known_grid;
	public int current_building_rotation = 0;

	public CsgSphere3D building_cursor;

	public PackedScene current_building_scene = GD.Load<PackedScene>("res://building_scenes/ExampleGrowingPlot.tscn");
	public BuildingGridPlacable current_building_instance;
	public bool build_mode = false;

	public void init_build_mode () {
		
	}
	
	public void make_building_instance () {
		current_building_instance = current_building_scene.Instantiate<BuildingGridPlacable>();

		current_building_instance.Position = Vector3.Zero;
		current_building_instance.Visible = false;
		current_building_instance.TopLevel = true;
		current_building_instance.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;

		AddChild(current_building_instance);

		current_building_instance.set_collision(false);
	}

	public void clear_building_instance () {
		current_building_instance.QueueFree();

		current_building_instance = null;
	}

	public void building_cast () {
		GodotObject current_cast_result = building_raycaster.GetCollider();
		Vector3 current_cast_position = building_raycaster.GetCollisionPoint();

		if (building_raycaster.IsColliding()) {
			last_building_cast_position = current_cast_position;
		}

		if (current_cast_result != building_cast_result) {
			building_cast_result = current_cast_result;
		}

		if (build_mode && current_building_instance != null) {
			if (building_cast_result != null && building_cast_result is BuildingGridChunk) {

				last_known_grid = (building_cast_result as BuildingGridChunk).parent_grid;

				Vector3 collision_normal = building_raycaster.GetCollisionNormal();

				Vector3I hit_pos = last_known_grid.position_to_voxel(last_building_cast_position - (collision_normal * 0.5f));
				Vector3I hit_chunk = last_known_grid.position_to_chunk(last_building_cast_position - (collision_normal * 0.5f));
				Vector3I hit_chunk_voxel = last_known_grid.position_to_chunk_voxel(last_building_cast_position - (collision_normal * 0.5f));

				Vector3I projected_pos = hit_pos + new Vector3I((int) collision_normal.X, (int) collision_normal.Y, (int) collision_normal.Z);
				
				current_building_instance.Visible = true;

				building_hit_label.Text = "Hit Pos: " + hit_pos.ToString();

				BuildDirection build_direction = Tools.normal_to_enum(collision_normal);
				
				Vector3 build_normal = Vector3.Up;

				if (Tools.enum_in_flags(build_direction, current_building_instance.placable_directions)) {
					build_normal = Tools.enum_to_normal(build_direction);
				} else {
					build_normal = Tools.enum_to_normal(current_building_instance.default_direction);
				}

				current_building_instance.Position = new Vector3(projected_pos.X, projected_pos.Y, projected_pos.Z) + last_known_grid.GlobalPosition + new Vector3(0.5f, 0.5f, 0.5f);
				current_building_instance.Rotation = new Vector3(0, 0, 0);
				Tools.up_to_rot(current_building_instance, build_normal);
				current_building_instance.Rotate(build_normal, (float) (current_building_rotation * (Math.PI / 2.0f)));

				Vector3I corner_1 = current_building_instance.get_box_from();
				Vector3I corner_2 = current_building_instance.get_box_to();

				corner_1 = Tools.v3_to_v3I(Tools.up_to_rot(Tools.v3I_to_v3(corner_1), build_normal));
				corner_2 = Tools.v3_to_v3I(Tools.up_to_rot(Tools.v3I_to_v3(corner_2), build_normal));

				corner_1 = Tools.v3_to_v3I(Tools.v3I_to_v3(corner_1).Rotated(build_normal, (float) (current_building_rotation * (Math.PI / 2.0f))));
				corner_2 = Tools.v3_to_v3I(Tools.v3I_to_v3(corner_2).Rotated(build_normal, (float) (current_building_rotation * (Math.PI / 2.0f))));

				corner_1 += projected_pos;
				corner_2 += projected_pos;

				if (Input.IsActionJustPressed("click") && !controls_locked) {
					
					Godot.Collections.Array test_result = last_known_grid.is_area_free(corner_1, corner_2);

					if ((bool) test_result[0]) {
						int index = last_known_grid.get_free_index();

						if (index != -1) {

							BuildingGridPlacable new_instance = current_building_scene.Instantiate<BuildingGridPlacable>();

							last_known_grid.AddChild(new_instance);

							new_instance.GlobalTransform = current_building_instance.GlobalTransform;

							Godot.Collections.Array<BuildingGridChunk> affected_chunks = last_known_grid.set_area(corner_1, corner_2, index);

							foreach (BuildingGridChunk chunk in affected_chunks) {
								new_instance.occupied_chunks.Add(chunk);
								chunk.OnChunkChanged += new_instance.on_chunk_changed;
								chunk.on_chunk_changed();
							}

							new_instance.parent_grid = last_known_grid;
							last_known_grid.set_placable(index, new_instance);

							new_instance.set_mesh_visibility(false);

							new_instance.on_build();
						}

					} else {
						GD.Print(test_result);
						GD.Print(test_result.Count - 1);
						for (int i = 1; i < test_result.Count; i++) {
							CsgSphere3D ding = new CsgSphere3D();

							ding.Radius = 0.5f;
							ding.Position = (Vector3) test_result[i] + last_known_grid.GlobalPosition + new Vector3(0.5f, 0.5f, 0.5f);
							ding.UseCollision = false;
							ding.Visible = true;
							ding.TopLevel = true;
							ding.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;

							AddChild(ding);
						}
						//new_instance.QueueFree();
					}
				}
			} else {
				current_building_instance.Visible = false;
			}
		} else {
			current_building_instance.Visible = false;
		}
	}
}