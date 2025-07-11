using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public enum BuildingMode {
	disabled,
	building,
	demolishing,
}

public partial class Player {

	public RayCast3DExtendable building_raycaster;
	public Vector3 last_building_cast_position;
	public GodotObject building_cast_result;
	public BuildingGrid last_known_grid;
	public int current_building_rotation = 0;

	public Array<BuildingGridPlacable> demolish_targets = new Array<BuildingGridPlacable>();
	public int max_demolish_targets = 100;

	public CsgSphere3D building_cursor;

	public PackedScene current_building_scene = null; //GD.Load<PackedScene>("res://building_scenes/ExampleGrowingPlot.tscn");
	public BuildingGridPlacable current_building_instance;

	public BuildingMode current_build_mode = BuildingMode.disabled;

	public float current_demolish_time = 0.0f;

	public void init_build_mode () {
		if (current_building_scene != null) {
			make_building_instance();
		}
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
		if (current_building_instance != null) {
			current_building_instance.QueueFree();
		}

		current_building_instance = null;
	}

	public void clear_demolish_targets () {
		foreach (BuildingGridPlacable target in demolish_targets) {
			target.set_mesh_visibility(false);
		}

		demolish_targets.Clear();
	}

	public void on_building_choice_selected (Variant data) {
		Dictionary new_data = (Dictionary) data;
		string res_path = (string) new_data["res_path"];
		if (res_path != null) {
			try {
				current_building_scene = GD.Load<PackedScene>(res_path);
				make_building_instance();
			} catch {
				GD.Print("we done fucked up loading shit!");
			}
		}

		((CategoryList) active_gui).OnChoiceSelected -= on_building_choice_selected;
		clear_active_gui();
	}

	public void building_cast (double delta) {
		GodotObject current_cast_result = building_raycaster.GetCollider();
		Vector3 current_cast_position = building_raycaster.GetCollisionPoint();

		if (building_raycaster.IsColliding()) {
			last_building_cast_position = current_cast_position;
		}

		if (current_cast_result != building_cast_result) {
			building_cast_result = current_cast_result;
		}

		if (building_cast_result != null && building_cast_result is BuildingGridChunk) {

			last_known_grid = (building_cast_result as BuildingGridChunk).parent_grid;

			Vector3 collision_normal = building_raycaster.GetCollisionNormal();

			Vector3I hit_pos = last_known_grid.position_to_voxel(last_building_cast_position - (collision_normal * 0.5f));
			Vector3I hit_chunk = last_known_grid.position_to_chunk(last_building_cast_position - (collision_normal * 0.5f));
			Vector3I hit_chunk_voxel = last_known_grid.position_to_chunk_voxel(last_building_cast_position - (collision_normal * 0.5f));

			Vector3I projected_pos = hit_pos + new Vector3I((int) collision_normal.X, (int) collision_normal.Y, (int) collision_normal.Z);

			if (current_build_mode == BuildingMode.building && current_building_instance != null) {

				current_building_instance.Visible = true;

				building_hit_label.Text = "Hit Pos: " + hit_pos.ToString();

				if (last_known_grid.is_position_valid(hit_pos)) {
					voxel_data_label.Text = last_known_grid.get_block(hit_pos).ToString();
				}
				

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

				if (mouse_clicked && !is_in_gui() && !controls_locked) {
					
					Godot.Collections.Array test_result = last_known_grid.is_area_free(corner_1, corner_2);

					if ((bool) test_result[0]) {
						
						BuildingGridPlacable new_instance = current_building_scene.Instantiate<BuildingGridPlacable>();
						
						last_known_grid.place(new_instance, projected_pos, build_normal, current_building_rotation);

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
				if (current_building_instance != null) {
					current_building_instance.Visible = false;
				}
			}

			if (current_build_mode == BuildingMode.demolishing) {
				if (last_known_grid.is_position_valid(hit_pos)) {
					VoxelData result = last_known_grid.get_block(hit_pos);

					if (result.placable_index != -1) {
						BuildingGridPlacable placable = last_known_grid.placables[result.placable_index];
						//GD.Print(placable);

						if (mouse_clicked) {
							if (!demolish_targets.Contains(placable)) {
								demolish_targets.Add(placable);
								placable.set_mesh_visibility(true);
							} else {
								demolish_targets.Remove(placable);
								placable.set_mesh_visibility(false);
							}
						} 
					}
				}

				if (Input.IsMouseButtonPressed(MouseButton.Right)) {
					current_demolish_time += (float) delta;

					if (current_demolish_time > 1.0f) {
						GD.Print("it's demolishin time");
						current_demolish_time = 0.0f;

						foreach (BuildingGrid grid in BuildingGrid.grids) {
							grid.set_mesh_visibility(false);
						}
						
						foreach (BuildingGridPlacable target in demolish_targets) {
							target.mark_for_demolishing();
						}

						current_build_mode = BuildingMode.disabled;
						clear_active_gui();
						clear_demolish_targets();
					}
				} else {
					current_demolish_time -= (float) delta;
					current_demolish_time = Math.Clamp(current_demolish_time, 0, 1);
				}
			}

		} else {
			if (current_building_instance != null) {
				current_building_instance.Visible = false;
			}
		}

		
		
		
	}
}