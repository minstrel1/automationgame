using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Tools {

	public static Vector3 v3I_to_v3 (Vector3I input) {
		return new Vector3(input.X, input.Y, input.Z);
	}

	public static Vector3I v3_to_v3I (Vector3 input) {
		return new Vector3I((int) Math.Round(input.X), (int) Math.Round(input.Y), (int) Math.Round(input.Z));
	}

	public static Vector3 rot_to_up (Vector3 point, Vector3 normal) {

		if (normal == Vector3.Up) {
			// no rotation
		} else if (normal == Vector3.Down) {
			point = point.Rotated(Vector3.Right, (float) Math.PI);
		} else if (normal == Vector3.Left) {
			point = point.Rotated(Vector3.Back, (float) Math.PI / 2);
		} else if (normal == Vector3.Right) {
			point = point.Rotated(Vector3.Forward, (float) Math.PI / 2);
		} else if (normal == Vector3.Forward) {
			point = point.Rotated(Vector3.Left, (float) Math.PI / 2);
		} else if (normal == Vector3.Back) {
			point = point.Rotated(Vector3.Right, (float) Math.PI / 2);
		}

		return point;
	}

	public static Vector3 rot_to_up (Vector3 normal) {
		return rot_to_up(normal, normal);
	}

	public static BuildDirection rot_to_up (BuildDirection direction) {
		Vector3 normal = enum_to_normal(direction);
		return normal_to_enum(rot_to_up(normal, normal));
	}

	public static Vector3I rot_to_up (Vector3I point, Vector3 normal) {
		return v3_to_v3I(rot_to_up(v3I_to_v3(point), normal));
	}

	public static Vector3I v3I_rot (Vector3I point, Vector3 axis, float angle) {
		return v3_to_v3I(v3I_to_v3(point).Rotated(axis, angle));
	}

	public static Vector3 up_to_rot (Vector3 point, Vector3 normal) {

		if (normal == Vector3.Up) {
			// no rotation
		} else if (normal == Vector3.Down) {
			point = point.Rotated(Vector3.Right, (float) Math.PI);
		} else if (normal == Vector3.Left) {
			point = point.Rotated(Vector3.Forward, -(float) Math.PI / 2);
		} else if (normal == Vector3.Right) {
			point = point.Rotated(Vector3.Back, -(float) Math.PI / 2);
		} else if (normal == Vector3.Forward) {
			point = point.Rotated(Vector3.Right, -(float) Math.PI / 2);
		} else if (normal == Vector3.Back) {
			point = point.Rotated(Vector3.Left, -(float) Math.PI / 2);
		}

		return point;
	}

	
	public static void up_to_rot (Node3D point, Vector3 normal) {

		if (normal == Vector3.Up) {
			// no rotation
		} else if (normal == Vector3.Down) {
			point.Rotate(Vector3.Right, (float) Math.PI);
		} else if (normal == Vector3.Left) {
			point.Rotate(Vector3.Forward, -(float) Math.PI / 2);
		} else if (normal == Vector3.Right) {
			point.Rotate(Vector3.Back, -(float) Math.PI / 2);
		} else if (normal == Vector3.Forward) {
			point.Rotate(Vector3.Right, -(float) Math.PI / 2);
		} else if (normal == Vector3.Back) {
			point.Rotate(Vector3.Left, -(float) Math.PI / 2);
		}

	}

	public static Vector3I up_to_rot (Vector3I point, Vector3 normal) {
		return v3_to_v3I(up_to_rot(v3I_to_v3(point), normal));
	}

	public static BuildDirection up_to_rot (BuildDirection direction, Vector3 normal) {
		Vector3 enum_normal = enum_to_normal(direction);
		return normal_to_enum(up_to_rot(enum_normal, normal));
	}

	public static BuildDirectionFlags up_to_rot (BuildDirectionFlags directions, Vector3 normal) {
		Array<BuildDirection> enums = flags_to_enum(directions);

		for (int i = 0; i < enums.Count; i++) {
			enums[i] = up_to_rot(enums[i], normal);
		}

		return enum_to_flags(enums);
	}

	public static Vector3I apply_building_rotations (Vector3I pos, Vector3 normal, int rotation) {
		pos = Tools.up_to_rot(pos, normal);
		pos = Tools.v3I_rot(pos, normal, (float) (rotation * (Math.PI / 2.0f)));
		return pos;
	}

	public static BuildDirection apply_building_rotations (BuildDirection direction, Vector3 normal, int rotation) {
		Vector3 enum_normal = enum_to_normal(direction);

		enum_normal = up_to_rot(enum_normal, normal);
		enum_normal = enum_normal.Rotated(normal, rotation * (float) ((Math.PI / 2.0f)));

		return normal_to_enum(enum_normal);
	}

	public static BuildDirectionFlags apply_building_rotations (BuildDirectionFlags directions, Vector3 normal, int rotation) {
		Array<BuildDirection> enums = flags_to_enum(directions);

		for (int i = 0; i < enums.Count; i++) {
			enums[i] = apply_building_rotations(enums[i], normal, rotation);
		}

		return enum_to_flags(enums);
	}

	public static BuildDirection normal_to_enum (Vector3 normal) {

		normal = v3I_to_v3(v3_to_v3I(normal));

		if (normal == Vector3.Up) {
			return BuildDirection.Up;
		} else if (normal == Vector3.Down) {
			return BuildDirection.Down;
		} else if (normal == Vector3.Left) {
			return BuildDirection.Left;
		} else if (normal == Vector3.Right) {
			return BuildDirection.Right;
		} else if (normal == Vector3.Forward) {
			return BuildDirection.Forward;
		} else if (normal == Vector3.Back) {
			return BuildDirection.Back;
		} else {
			return BuildDirection.Up;
		}
	}

	public static Vector3 enum_to_normal (BuildDirection value) {
		switch (value) {
			case BuildDirection.Left:
				return Vector3.Left;
			case BuildDirection.Right:
				return Vector3.Right;
			case BuildDirection.Up:
				return Vector3.Up;
			case BuildDirection.Down:
				return Vector3.Down;
			case BuildDirection.Forward:
				return Vector3.Forward;
			case BuildDirection.Back:
				return Vector3.Back;
			default:
				return Vector3.Zero;
		}
	}

	public static Array<BuildDirection> flags_to_enum (BuildDirectionFlags flags) {
		Array<BuildDirection> result = new Array<BuildDirection>();

		if (flags == BuildDirectionFlags.Any || (((int) flags & 2) >> 1) == 1) {
			result.Add(BuildDirection.Left);
		}

		if (flags == BuildDirectionFlags.Any || (((int) flags & 4) >> 2) == 1) {
			result.Add(BuildDirection.Right);
		}

		if (flags == BuildDirectionFlags.Any || (((int) flags & 8) >> 3) == 1) {
			result.Add(BuildDirection.Up);
		}

		if (flags == BuildDirectionFlags.Any || (((int) flags & 16) >> 4) == 1) {
			result.Add(BuildDirection.Down);
		}

		if (flags == BuildDirectionFlags.Any || (((int) flags & 32) >> 5) == 1) {
			result.Add(BuildDirection.Forward);
		}

		if (flags == BuildDirectionFlags.Any || (((int) flags & 64) >> 6) == 1) {
			result.Add(BuildDirection.Back);
		}

		return result;
	}

	public static BuildDirectionFlags enum_to_flags (Array<BuildDirection> enums) {
		BuildDirectionFlags result = 0;

		foreach(BuildDirection direction in enums) {
			switch (direction) {
				case BuildDirection.Left:
					result |= BuildDirectionFlags.Left;
					break;
				case BuildDirection.Right:
					result |= BuildDirectionFlags.Right;
					break;
				case BuildDirection.Up:
					result |= BuildDirectionFlags.Up;
					break;
				case BuildDirection.Down:
					result |= BuildDirectionFlags.Down;
					break;
				case BuildDirection.Forward:
					result |= BuildDirectionFlags.Forward;
					break;
				case BuildDirection.Back:
					result |= BuildDirectionFlags.Back;
					break;
			}
		}

		return result;
	}

	public static BuildDirectionFlags enum_to_flags (BuildDirection enums) {
		BuildDirectionFlags result = 0;
		
		switch (enums) {
			case BuildDirection.Left:
				result |= BuildDirectionFlags.Left;
				break;
			case BuildDirection.Right:
				result |= BuildDirectionFlags.Right;
				break;
			case BuildDirection.Up:
				result |= BuildDirectionFlags.Up;
				break;
			case BuildDirection.Down:
				result |= BuildDirectionFlags.Down;
				break;
			case BuildDirection.Forward:
				result |= BuildDirectionFlags.Forward;
				break;
			case BuildDirection.Back:
				result |= BuildDirectionFlags.Back;
				break;
		}

		return result;
	}

	public static BuildDirection get_enum_opposite (BuildDirection direction) {
		switch (direction) {
			case BuildDirection.Left:
				return BuildDirection.Right;
			case BuildDirection.Right:
				return BuildDirection.Left;
			case BuildDirection.Up:
				return BuildDirection.Down;
			case BuildDirection.Down:
				return BuildDirection.Up;
			case BuildDirection.Forward:
				return BuildDirection.Back;
			case BuildDirection.Back:
				return BuildDirection.Forward;
		}
		return 0;
	}

	public static bool enum_in_flags (BuildDirection value, BuildDirectionFlags flags) {
		Array<BuildDirection> result = flags_to_enum(flags);
		return result.Contains(value);
	}

	public static Dictionary packed_scene_to_properties (string res_path) {
		PackedScene packed_scene = (PackedScene) GD.Load(res_path);
		SceneState scene_state = packed_scene.GetState();

		Dictionary result = new Dictionary();

		for (int i = 0; i < scene_state.GetNodePropertyCount(0); i++) {
			result[scene_state.GetNodePropertyName(0, i)] = scene_state.GetNodePropertyValue(0, i);
		}

		return result;
	}

	public static bool is_special_compatible (SpecialVoxelFlags from, SpecialVoxelFlags to) {
		switch (from) {
			case SpecialVoxelFlags.None:
				return false;
			
			case SpecialVoxelFlags.ItemInput:
				return to == SpecialVoxelFlags.ItemOutput || to == SpecialVoxelFlags.ItemInputOutput;

			case SpecialVoxelFlags.ItemOutput:
				return to == SpecialVoxelFlags.ItemInput || to == SpecialVoxelFlags.ItemInputOutput;

			case SpecialVoxelFlags.ItemInputOutput:
				return to == SpecialVoxelFlags.ItemInput || to == SpecialVoxelFlags.ItemOutput;

			case SpecialVoxelFlags.FluidInput:
				return to == SpecialVoxelFlags.FluidOutput || to == SpecialVoxelFlags.FluidInputOutput;

			case SpecialVoxelFlags.FluidOutput:
				return to == SpecialVoxelFlags.FluidInput || to == SpecialVoxelFlags.FluidInputOutput;

			case SpecialVoxelFlags.FluidInputOutput:
				return to == SpecialVoxelFlags.FluidInput || to == SpecialVoxelFlags.FluidOutput || to == SpecialVoxelFlags.FluidInputOutput;
		}
		return false;
	}

}