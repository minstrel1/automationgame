using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class MultiFilterRepresentation : GUI {

	public static Array<MultiFilterRepresentation> instances = new Array<MultiFilterRepresentation>();
	public static Stack<MultiFilterRepresentation> available_instances = new Stack<MultiFilterRepresentation>();

	public MultiFilter multi_filter;
	public int current_index;
	
	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/multi_filter_representation.tscn");

	public TextureRect background;
	public TextureRect filter_texture;
	public TextureRect item_texture;
	public Label item_count;

	public bool interactable = true;

	public static MultiFilterRepresentation make (int index, MultiFilter filter, Control parent) {
		MultiFilterRepresentation new_rep = get_first_available_instance();

		if (new_rep == null) {
			new_rep = scene.Instantiate<MultiFilterRepresentation>();
			instances.Add(new_rep);
		} else {
			if (new_rep.GetParent() != null) {
				GD.Print("WHATT HE FUCK>?>???");
			}
		}

		new_rep.gui_parent = parent;
		
		new_rep.current_index = index;
		new_rep.multi_filter= filter;
		new_rep.multi_filter.OnFiltersChanged += new_rep.on_filters_changed;

		new_rep.in_use = true;

		if (new_rep.readied) {
			new_rep.RequestReady();
		}
		
		parent.AddChild(new_rep);

		return new_rep;
	}

	public override void _Ready()
	{
		base._Ready();

		background = GetNode<TextureRect>("Background");
		item_texture = GetNode<TextureRect>("ItemTexture");
		filter_texture = GetNode<TextureRect>("FilterTexture");
		item_count = GetNode<Label>("ItemCount");

		update_visualization();
	
	}

	public void clear () {
		gui_parent = null;

		current_index = 0;
		multi_filter = null;

		Position = Vector2.Zero;
	}

	public void on_filters_changed (Array<String> filters) {
		GD.Print(multi_filter.filters);

		update_visualization();
	}

	public void update_visualization () {
		if (multi_filter.filters[current_index] != "") {
			
			if (multi_filter.prototype_type == "items") {
				if (Prototypes.items.ContainsKey(multi_filter.filters[current_index])) {
					item_texture.Texture = GD.Load<Texture2D>(Prototypes.items[multi_filter.filters[current_index]].icon_texture);
				}
			}
			
		} else {
			item_texture.Texture = null;
		}
		// if (current_item != null) {
		// 	filter_texture.Texture = null;

		// 	string texture_path = current_item.prototype.icon_texture;
		// 	item_texture.Texture = GD.Load<Texture2D>(texture_path);

		// 	item_count.Text = current_item.count.ToString();
		// } else {
		// 	if (parent_inventory != null) {
		// 		if (parent_inventory.filters[current_index] != null) {
		// 			if (parent_inventory.filters[current_index].icon_path != "") {
		// 				filter_texture.Texture = GD.Load<Texture2D>(parent_inventory.filters[current_index].icon_path);
		// 			}
		// 		}
		// 	}

		// 	item_count.Text = "";
		// 	item_texture.Texture = null;
		// }
	}

	public override void _GuiInput(InputEvent @event)
	{
		base._GuiInput(@event);

		if (@event is InputEventMouseButton && interactable) {
			InputEventMouseButton mouse_event = @event as InputEventMouseButton;

			if (mouse_event.ButtonIndex == MouseButton.Left && mouse_event.Pressed) {
				if (Player.hand_item.has_item()) {
					multi_filter.filters[current_index] = Player.hand_item.current_item.name;
					multi_filter.on_filters_changed();
				}
			}

			if (mouse_event.ButtonIndex == MouseButton.Right && mouse_event.Pressed) {
				if (multi_filter.filters[current_index] != "") {
					multi_filter.filters[current_index] = "";
					multi_filter.on_filters_changed();
				}
			}
		}
	}

	public static MultiFilterRepresentation get_first_available_instance () {
		if (available_instances.Count > 0) {
			MultiFilterRepresentation result = available_instances.Pop();
			if (result.GetParent() != null) {
				GD.Print("Item rep pushed with parent");
			}
			return result;
		}

		return null;
	}

	public override void release () {
		in_use = false;
		
		Node parent = gui_parent;
		if (parent != null) {
			parent.RemoveChild(this);
		}

		clear();
		
		available_instances.Push(this);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
	}
}
