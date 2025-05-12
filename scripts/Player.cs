using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
public partial class Player : CharacterBody3D {
	[ExportGroup("Player")]
	[Export(PropertyHint.Range, "1, 35, 1")]
	public float speed = 10;
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

	public Inventory inventory = new Inventory(250);

	public RayCast3DExtendable interaction_raycaster;
	public Vector3 last_cast_position;
	public GodotObject interact_cast_result;

	public static Player instance;
	public static HandItemRepresentation hand_item;

	public override void _Ready()
	{
		base._Ready();

		Player.instance = this;
		Player.hand_item = GetNode<HandItemRepresentation>("HandItemRepresentation");

		camera = GetNode<Node3D>("Camera");
		actual_renderer = GetNode<Camera3D>("Camera/Camera3D");

		interaction_raycaster = GetNode<RayCast3DExtendable>("Camera/Camera3D/InteractionRaycaster");

		gui_parent = GetNode<Control>("PlayerGUI");

		capture_mouse();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		interaction_cast();

		if (! controls_locked) {
			walk_vel = do_walk((float) delta);
			grav_vel = do_gravity((float) delta);
			jump_vel = do_jump((float) delta);
		} else {
			walk_vel = Vector3.Zero;
			grav_vel = do_gravity((float) delta);
			jump_vel = Vector3.Zero;
		}

		Velocity = walk_vel + grav_vel + jump_vel;
		MoveAndSlide();

		if (head_bob && delta > 0) {
			handle_bob(walk_vel, (float) delta);
		}
	}

	private Vector3 do_walk (float delta) {
		move_dir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
		Vector3 forward = camera.GlobalTransform.Basis * new Vector3(move_dir.X, 0, move_dir.Y);
		Vector3 walk_dir = new Vector3(forward.X, 0, forward.Z).Normalized();

		if (walk_dir == Vector3.Zero) {
			if (IsOnFloor()) {
				walk_vel = walk_vel.MoveToward(walk_dir * speed * move_dir.Length(), deceleration * delta);
			} else {
				walk_vel = walk_vel.MoveToward(walk_dir * speed * move_dir.Length(), deceleration * delta * 0.25f);
			}
			
		} else {
			walk_vel = walk_vel.MoveToward(walk_dir * speed * move_dir.Length(), acceleration * delta);
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
			if (is_interact_valid()) {
				GD.Print("Interacting with " + interact_cast_result.ToString());
				(interact_cast_result as IInteractable).on_interact();
			}
		}

		if (Input.IsActionJustPressed("test")) {
			inventory.insert(new InventoryItem("test_seeds", 27));
		}

		if (Input.IsActionJustPressed("test2")) {
			inventory.insert(new InventoryItem("test_cock", 39));
		}

		if (@event is InputEventMouseButton) {
			InputEventMouseButton new_event = @event as InputEventMouseButton;
			if (new_event.Pressed) {
				if (hand_item.has_item()) {
					hand_item.clear_item();
				}
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
			last_cast_position = current_cast_position;
		}

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

	public bool is_interact_valid () {
		return interact_cast_result != null && interact_cast_result is IInteractable;
	}

	public void on_interact_target_changed () {

	}
}