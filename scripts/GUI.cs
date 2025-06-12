using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public struct GUIDummyData {
	public Control parent;
	public int node_index;
	public Vector2 pos;
	public Vector2 global_pos;
	public Vector2 size;
	public Vector2 min_size;

	public GUIDummyData (Control parent, int node_index, Vector2 pos, Vector2 global_pos, Vector2 size, Vector2 min_size ) {
		this.parent = parent;
		this.node_index = node_index;
		this.pos = pos;
		this.global_pos = pos;
		this.size = size;
		this.min_size = min_size;
	}
}

public partial class GUI : Control {
	public Control gui_parent;
	public Godot.Collections.Dictionary<string, Array<GUIDummy>> dummies = new Godot.Collections.Dictionary<string, Array<GUIDummy>>();

	public bool in_use = false;
	public bool readied = false;
	public bool released = false;
	
	public override void _Ready () {
		int count = 0;
		foreach (Node child in get_all_children(this)) {
			if (child is GUIDummy) {
				string key = (child as GUIDummy).replacable_by;
				if (!dummies.ContainsKey(key)) {
					dummies[key] = new Array<GUIDummy>();
				}

				dummies[key].Add(child as GUIDummy);
				count += 1;
			}
		}

		if (count > 0) {
			//GD.Print(String.Format("GUI {0} found {1} dummies.", this.ToString(), count));
			//GD.Print(dummies);
		}

		readied = true;
	}

	public Array<GUIDummy> pop_dummy_type (string type) {
		Array<GUIDummy> result = dummies[type];
		dummies.Remove(type);
		return result;
	}

	public GUIDummy pop_dummy_singular (string type) {
		Array<GUIDummy> result = dummies[type];
		dummies.Remove(type);
		if (result.Count > 0) {
			return result[0];
		}
		return null;
	}

	public GUIDummyData remove_dummy (GUIDummy dummy) {
		Control parent = dummy.GetParent<Control>();

		GUIDummyData result = new GUIDummyData (
			parent,
			dummy.GetIndex(),
			dummy.Position,
			dummy.GlobalPosition,
			dummy.Size,
			dummy.GetCombinedMinimumSize()
		);

		dummy.QueueFree();

		return result;
	}

	private Array<Node> get_all_children(Node node) {
		Array<Node> result = new Array<Node>();

		foreach (Node child in node.GetChildren()) {
			if (child.GetChildCount() > 0) {
				result.Add(child);
				result.AddRange(get_all_children(child));
			} else {
				result.Add(child);
			}
		}

		return result;
	}

	public virtual void release () {
		released = true;
		QueueFree();
	}
}