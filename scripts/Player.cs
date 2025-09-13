using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using Limbo.Console.Sharp;

[GlobalClass]
public partial class Player : CharacterBody3D {
	[ExportGroup("Player")]
	[Export(PropertyHint.Range, "1, 35, 1")]
	public float speed = 10;
	[Export(PropertyHint.Range, "1, 35, 1")]
	public float sprint_speed = 20;
	[Export(PropertyHint.Range, "10, 400, 1")]
	public float acceleration = 100;
	[Export(PropertyHint.Range, "10, 400, 1")]
	public float deceleration = 100;

	[Export]
	public bool can_jump = false;
	[Export(PropertyHint.Range, "0.1, 3.0, 0.1")]
	public float jump_height = 1;
	[Export(PropertyHint.Range, "0.1, 3.0, 0.1, or_greater")]
	public float camera_sens = 1;

	[ExportGroup("Camera")]
	[Export]
	bool head_bob = true;
	[Export]
	float side_amplitude = 0.08f;
	[Export]
	float side_frequency = 3.3f;
	[Export]
	float vertical_amplitude = 0.05f;
	[Export]
	float vertical_frequency = 6.6f;

	bool jumping = false;
	bool sprinting = false;
	bool mouse_captured = false;
	bool controls_locked = false;

	float gravity = (float) ProjectSettings.GetSetting("physics/3d/default_gravity");

	Vector2 move_dir;
	Vector2 look_dir;

	Vector3 walk_vel;
	Vector3 grav_vel;
	Vector3 jump_vel;

	Node3D camera;
	Camera3D actual_renderer;

	float walk_time = 0;
	bool stepped = false;
	
	bool scrolled_this_frame = false;

	public Inventory inventory = new Inventory(100);

	public RayCast3DExtendable interaction_raycaster;
	public Vector3 last_interact_cast_position;
	public GodotObject interact_cast_result;

	public CollisionShape3D collider;

	public static Player instance;
	public static HandItemRepresentation hand_item;

	public bool mouse_clicked = false;
	public bool right_mouse_clicked = false;

	public override void _Ready()
	{
		base._Ready();

		Player.instance = this;
		Player.hand_item = GetNode<HandItemRepresentation>("HandItemRepresentation");

		camera = GetNode<Node3D>("Camera");
		actual_renderer = GetNode<Camera3D>("Camera/Camera3D");

		collider = GetNode<CollisionShape3D>("CollisionShape3D");

		interaction_raycaster = GetNode<RayCast3DExtendable>("Camera/Camera3D/InteractionRaycaster");
		building_raycaster = GetNode<RayCast3DExtendable>("Camera/Camera3D/BuildingRaycaster");

		gui_parent = GetNode<Control>("PlayerGUI");

		ready_hud();

		ready_drone();

		init_build_mode();

		capture_mouse();

		RegisterConsoleCommands();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		scrolled_this_frame = false;
		

		interaction_cast();
		building_cast(delta);

		walk_vel = do_walk((float) delta);
		grav_vel = do_gravity((float) delta);
		jump_vel = do_jump((float) delta);

		Velocity = walk_vel + grav_vel + jump_vel;
		MoveAndSlide();

		if (head_bob && delta > 0) {
			handle_bob(walk_vel, (float) delta);
		}

		update_stats();
		update_hud();

		mouse_clicked = false;
		right_mouse_clicked = false;
		
	}

	private Vector3 do_walk (float delta) {

		if (!controls_locked) {
			move_dir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
		} else {
			move_dir = Vector2.Zero;
		}

		Vector3 forward = camera.GlobalTransform.Basis * new Vector3(move_dir.X, 0, move_dir.Y);
		Vector3 walk_dir = new Vector3(forward.X, 0, forward.Z).Normalized();

		bool can_sprint = false;
		float final_speed = speed;

		if (move_dir.Dot(Vector2.Up) > 0.5) {
			can_sprint = true;
		};

		if (can_sprint && sprinting) {
			final_speed = sprint_speed;
		}

		if (walk_dir == Vector3.Zero) {
			if (IsOnFloor()) {
				walk_vel = walk_vel.MoveToward(walk_dir * final_speed * move_dir.Length(), deceleration * delta);
			} 
		} else {
			if (IsOnFloor()) {
				walk_vel = walk_vel.MoveToward(walk_dir * final_speed * move_dir.Length(), acceleration * delta);
			} else {
				float move_dir_x = move_dir.X;
				float move_dir_y = move_dir.Y;


				walk_vel = walk_vel.MoveToward(walk_dir * final_speed * move_dir.Length(), acceleration * delta * 0.25f);
			}
		}

		return walk_vel;
	}

	private Vector3 do_gravity (float delta) {
		grav_vel = IsOnFloor() ? Vector3.Zero : grav_vel.MoveToward(new Vector3(0, Velocity.Y - gravity, 0), gravity * delta);
		return grav_vel;
	}

	private Vector3 do_jump (float delta) {
		if (! can_jump) {
			return Vector3.Zero;
		}

		if (jumping) {
			if (IsOnFloor()) {
				jump_vel = new Vector3(0, (float) Math.Sqrt(4 * jump_height * gravity), 0);
			}
			jumping = false;
			return jump_vel;
		}

		jump_vel = IsOnFloor() ? Vector3.Zero : jump_vel.MoveToward(Vector3.Zero, gravity * delta);
		return jump_vel;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion) {
			look_dir = (@event as InputEventMouseMotion).Relative * 0.001f;
			if (mouse_captured) {
				if (! controls_locked) {
					rotate_camera();
				}
			}
		}

		if (Input.IsActionJustPressed("jump")) {
			jumping = true;
		}

		if (Input.IsActionJustPressed("open_inventory")) {

			if (is_in_gui() && active_gui is PlayerInventoryGUI) {
				clear_active_gui();
			} else {
				set_active_gui(PlayerInventoryGUI.make_player_inventory_gui(inventory, gui_parent));
			}
		}

		if (Input.IsActionJustPressed("close")) {
			if (is_in_gui()) {
				clear_active_gui();
			} else {
				if (mouse_captured) {
					release_mouse();
				} else {
					capture_mouse();
				}
			}
		}

		if (Input.IsActionJustPressed("interact")) {
			if (is_in_gui()) {
				clear_active_gui();
			} else if (is_interact_valid() && current_build_mode == BuildingMode.disabled) {
				GD.Print("Interacting with " + interact_cast_result.ToString());
				(interact_cast_result as IInteractable).on_interact();
			}
		}

		if (Input.IsActionJustPressed("test")) {
			inventory.insert(new SimpleItem{name = "test_item", count = 27});
		}

		if (Input.IsActionJustPressed("test2")) {
			set_active_gui(InventoryGUI.make(inventory, gui_parent));
		}

		if (Input.IsActionJustPressed("build_mode")) {
			if (current_build_mode == BuildingMode.building) {
				foreach (BuildingGrid grid in BuildingGrid.grids) {
					grid.set_mesh_visibility(false);
				}

				current_build_mode = BuildingMode.disabled;
				clear_active_gui();
				clear_building_instance();
				current_building_scene = null;
			} else if (current_build_mode == BuildingMode.demolishing) {
				clear_demolish_targets();

				current_build_mode = BuildingMode.building;
				if (current_building_instance == null) {
					CategoryList new_instance = CategoryList.make(CategoryListMode.Buildings, Prototypes.buildings, gui_parent);
					new_instance.OnChoiceSelected += on_building_choice_selected;
					set_active_gui(new_instance);
				}
			} else {
				foreach (BuildingGrid grid in BuildingGrid.grids) {
					grid.set_mesh_visibility(true);
				}

				current_build_mode = BuildingMode.building;
				if (current_building_instance == null) {
					CategoryList new_instance = CategoryList.make(CategoryListMode.Buildings, Prototypes.buildings, gui_parent);
					new_instance.OnChoiceSelected += on_building_choice_selected;
					set_active_gui(new_instance);
				}
			}
		}

		if (Input.IsActionJustPressed("demolish_mode")) {
			if (current_build_mode == BuildingMode.demolishing) {
				foreach (BuildingGrid grid in BuildingGrid.grids) {
					grid.set_mesh_visibility(false);
				}

				current_build_mode = BuildingMode.disabled;
				clear_active_gui();
				clear_demolish_targets();
			} else if (current_build_mode == BuildingMode.building) {
				current_build_mode = BuildingMode.demolishing;
				clear_active_gui();
				clear_building_instance();
				current_building_scene = null;
			} else {
				foreach (BuildingGrid grid in BuildingGrid.grids) {
					grid.set_mesh_visibility(true);
				}

				current_build_mode = BuildingMode.demolishing;
			}
		}

		if (current_build_mode == BuildingMode.building && !scrolled_this_frame) {
			if (Input.IsActionJustPressed("rotate_up")) {
				GD.Print("scroll up");
				current_building_rotation += 1;
				if (current_building_rotation > 3) {
					current_building_rotation = 0;
				}
				scrolled_this_frame = true;
			} else if (Input.IsActionJustPressed("rotate_down")) {
				current_building_rotation -= 1;
				if (current_building_rotation < 0) {
					current_building_rotation = 3;
				}
				scrolled_this_frame = true;
			}
		}

		sprinting = Input.IsActionPressed("shift_modifier");

		if (@event is InputEventMouseButton) {
			InputEventMouseButton new_event = @event as InputEventMouseButton;
			if (new_event.ButtonIndex == MouseButton.Left && new_event.Pressed) {
				if (hand_item.has_item()) {
					hand_item.clear_item();
				}

				mouse_clicked = true;
			}

			if (new_event.ButtonIndex == MouseButton.Right && new_event.Pressed) {
				right_mouse_clicked = true;
			}
		}
	}

	private void handle_bob (Vector3 walk_vel, float delta) {
		Vector3 offset = actual_renderer.Position;
		if (walk_vel.Length() > 0.5f) {
			walk_time += delta;
			float x = (float) Math.Cos(walk_time * side_frequency) * side_amplitude;
			float y = (float) Math.Sin(walk_time * vertical_frequency) * vertical_amplitude;

			if (y < -0.03) {
				if (! stepped) {
					stepped = true;
				}
			} else {
				stepped = false;
			}

			Vector3 new_offset = new Vector3(x, y, 0);
			offset = offset.MoveToward(new_offset, (walk_vel.Length() / speed) * delta);

		} else {
			walk_time = 0;
			offset = offset.MoveToward(Vector3.Zero, delta);
		}
	}

	private void rotate_camera (float sens_mod = 1.0f) {
		Vector3 new_rotation = camera.Rotation;
		new_rotation.Y -= look_dir.X * camera_sens * sens_mod;
		new_rotation.X = Math.Clamp(new_rotation.X - look_dir.Y * camera_sens * sens_mod, -1.5f, 1.5f);
		camera.Rotation = new_rotation;

		collider.Rotation = camera.Rotation * new Vector3(0, 1, 0);
	}

	public void capture_mouse () {
		Input.MouseMode = Input.MouseModeEnum.Captured;
		mouse_captured = true;
	}

	public void release_mouse () {
		Input.MouseMode = Input.MouseModeEnum.Visible;
		mouse_captured = false;
	}

	public void lock_controls () {
		controls_locked = true;
	}

	public void unlock_controls () {
		controls_locked = false;
	}

	public void interaction_cast () {
		GodotObject current_cast_result = interaction_raycaster.GetCollider();
		Vector3 current_cast_position = interaction_raycaster.GetCollisionPoint();

		if (interaction_raycaster.IsColliding()) {
			last_interact_cast_position = current_cast_position;
		}

		if (!is_in_gui() && current_build_mode == BuildingMode.disabled) {
			if (current_cast_result != interact_cast_result) {
				if (is_interact_valid()) {
					(interact_cast_result as IInteractable).on_hover_unfocus();
				}

				interact_cast_result = current_cast_result;

				if (is_interact_valid()) {
					(interact_cast_result as IInteractable).on_hover_focus();
				}
			}
		}

	}

	public bool is_interact_valid () {
		return interact_cast_result != null && interact_cast_result is IInteractable;
	}

	public void on_interact_target_changed () {

	}

	
}