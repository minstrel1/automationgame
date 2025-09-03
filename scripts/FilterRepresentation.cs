// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Godot;
// using Godot.Collections;
// using Godot.NativeInterop;

// public partial class FilterRepresentation : GUI {

// 	public static Array<FilterRepresentation> instances = new Array<FilterRepresentation>();
// 	public static Stack<FilterRepresentation> available_instances = new Stack<FilterRepresentation>();

// 	public MultiFilter multi_filter;
// 	public int current_index;
	
// 	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/filter_representation.tscn");

// 	public TextureRect background;
// 	public TextureRect filter_texture;
// 	public TextureRect item_texture;
// 	public Label item_count;

// 	public bool interactable = true;

// 	public static FilterRepresentation make (int current_index, MultiFilter filter, Control parent) {
// 		FilterRepresentation new_rep = get_first_available_instance();

// 		if (new_rep == null) {
// 			new_rep = scene.Instantiate<FilterRepresentation>();
// 			instances.Add(new_rep);
// 		} else {
// 			if (new_rep.GetParent() != null) {
// 				GD.Print("WHATT HE FUCK>?>???");
// 			}
// 		}

// 		new_rep.gui_parent = parent;
// 		new_rep.current_index = current_index;
// 		new_rep.in_use = true;

// 		if (new_rep.readied) {
// 			new_rep.RequestReady();
// 		}
		
// 		parent.AddChild(new_rep);

// 		return new_rep;
// 	}

// 	public override void _Ready()
// 	{
// 		base._Ready();

// 		background = GetNode<TextureRect>("Background");
// 		item_texture = GetNode<TextureRect>("ItemTexture");
// 		filter_texture = GetNode<TextureRect>("FilterTexture");
// 		item_count = GetNode<Label>("ItemCount");

// 		update_visualization();
	
// 	}

// 	public void clear () {
// 		gui_parent = null;

// 		current_index = 0;

// 		Position = Vector2.Zero;
// 	}

// 	public void on_inventory_slot_changed (int index, InventoryItem item) {
// 		if (index == current_index) {

// 			update_visualization();
// 		}
// 	}

// 	public void update_visualization () {
// 		// if (current_item != null) {
// 		// 	filter_texture.Texture = null;

// 		// 	string texture_path = current_item.prototype.icon_texture;
// 		// 	item_texture.Texture = GD.Load<Texture2D>(texture_path);

// 		// 	item_count.Text = current_item.count.ToString();
// 		// } else {
// 		// 	if (parent_inventory != null) {
// 		// 		if (parent_inventory.filters[current_index] != null) {
// 		// 			if (parent_inventory.filters[current_index].icon_path != "") {
// 		// 				filter_texture.Texture = GD.Load<Texture2D>(parent_inventory.filters[current_index].icon_path);
// 		// 			}
// 		// 		}
// 		// 	}

// 		// 	item_count.Text = "";
// 		// 	item_texture.Texture = null;
// 		// }
// 	}

// 	public override void _GuiInput(InputEvent @event)
// 	{
// 		base._GuiInput(@event);

// 		if (@event is InputEventMouseButton && interactable) {
// 			InputEventMouseButton mouse_event = @event as InputEventMouseButton;

// 			if (mouse_event.ButtonIndex == MouseButton.Left && mouse_event.Pressed) {
				
// 			}

// 			if (mouse_event.ButtonIndex == MouseButton.Right && mouse_event.Pressed) {
				
// 			}
// 		}
// 	}

// 	public static FilterRepresentation get_first_available_instance () {
// 		if (available_instances.Count > 0) {
// 			FilterRepresentation result = available_instances.Pop();
// 			if (result.GetParent() != null) {
// 				GD.Print("Item rep pushed with parent");
// 			}
// 			return result;
// 		}

// 		return null;
// 	}

// 	public override void release () {
// 		in_use = false;
		
// 		Node parent = gui_parent;
// 		if (parent != null) {
// 			parent.RemoveChild(this);
// 		}

// 		clear();
		
// 		available_instances.Push(this);
// 	}

// 	public override void _ExitTree()
// 	{
// 		base._ExitTree();
// 	}
// }