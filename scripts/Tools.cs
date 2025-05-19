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

	public static BuildDirection normal_to_enum (Vector3 normal) {

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

	public static bool enum_in_flags (BuildDirection value, BuildDirectionFlags flags) {
		Array<BuildDirection> result = flags_to_enum(flags);
		return result.Contains(value);
	}

}