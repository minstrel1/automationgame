using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class FluidRepresentation : GUI {

	public static Array<FluidRepresentation> instances = new Array<FluidRepresentation>();
	public static Stack<FluidRepresentation> available_instances = new Stack<FluidRepresentation>();

	public FluidContainer container;
	
	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/fluid_representation.tscn");

	public TextureRect background;
	public TextureRect filter_texture;
	public TextureRect fluid_texture;
	public Label fluid_count;

	public bool interactable = true;

	public static FluidRepresentation make (FluidContainer container, Control gui_parent) {
		FluidRepresentation new_rep = get_first_available_instance();

		if (new_rep == null) {
			new_rep = scene.Instantiate<FluidRepresentation>();
			instances.Add(new_rep);
		} else {
			if (new_rep.GetParent() != null) {
				GD.Print("WHATT HE FUCK>?>???");
			}
		}

		if (new_rep.readied) {
			new_rep.RequestReady();
		}

		new_rep.container = container;
		new_rep.gui_parent = gui_parent;
		
		gui_parent.AddChild(new_rep);

		return new_rep;
	}

	public override void _Ready()
	{
		base._Ready();

		background = GetNode<TextureRect>("Background");
		fluid_texture = GetNode<TextureRect>("FluidTexture");
		filter_texture = GetNode<TextureRect>("FilterTexture");
		fluid_count = GetNode<Label>("FluidCount");

		update_visualization();
	
	}

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);

		update_visualization();
	}

	public void clear () {
		gui_parent = null;

		container = null;

		Position = Vector2.Zero;
	}

	public void update_visualization () {
		if (container != null) {

			if (container.current_fluid != "") {
				fluid_texture.Texture = GD.Load<Texture2D>(Prototypes.fluids[container.current_fluid].icon_texture);
				fluid_count.Text = ((int) container.current_amount).ToString();
				fluid_count.Visible = true;
			} else {
				if (container.fluid_filter != "") {
					filter_texture.Texture = GD.Load<Texture2D>(Prototypes.fluids[container.fluid_filter].icon_texture);
				} else {
					filter_texture.Texture = null;
				}

				fluid_texture.Texture = null;
				fluid_count.Text = "";
				fluid_count.Visible = false;
			}
			
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
				// if (Input.IsActionPressed("shift_modifier")) {
				// 	if (current_item != null) {

				// 		int result;
				// 		if (current_item.parent_inventory == Player.instance.inventory) {
				// 			if (Player.instance.active_inventory != null) {
				// 				result = Player.instance.active_inventory.insert(current_item);
				// 			}
				// 		} else {
				// 			result = Player.instance.inventory.insert(current_item);
				// 		}

				// 	}
				// } else if (Player.hand_item.has_item()) {
				// 	if (current_item != null && Player.hand_item.current_item.name == current_item.name) {
				// 		int count = Player.hand_item.current_item.count;
				// 		int result = current_item.add(Player.hand_item.current_item);

				// 		if (count == result) {
				// 			Player.hand_item.clear_item();
				// 		} else {
				// 			Player.hand_item.update_visualization();
				// 		}
				// 	} else {
				// 		InventoryItem temp_item = current_item;
				// 		int temp_index = Player.hand_item.current_index;
				// 		Inventory temp_inventory = Player.hand_item.current_inventory;

				// 		current_item = Player.hand_item.current_item;
				// 		parent_inventory.set_item(current_item, current_index);

				// 		if (temp_inventory != null) {
				// 			temp_inventory.set_item(temp_item, temp_index);
				// 			//Player.hand_item.set_item(temp_item, temp_inventory);
				// 			Player.hand_item.clear_item();
				// 		} else {
				// 			Player.hand_item.set_item(null, null);
				// 		}
				// 	}
				// } else {
				// 	Player.hand_item.set_item(current_item, parent_inventory);
				// }
			}

			if (mouse_event.ButtonIndex == MouseButton.Right && mouse_event.Pressed) {
				// if (Player.hand_item.has_item()) {

				// 	int count = Player.hand_item.current_item.count;
				// 	int result = 0;

				// 	if (current_item != null && Player.hand_item.current_item.name == current_item.name) {
				// 		result = current_item.add(InventoryItem.new_item(Player.hand_item.current_item.name, 1));
				// 	} else if (current_item == null) {
				// 		parent_inventory.set_item(InventoryItem.new_item(Player.hand_item.current_item.name, 1), current_index);
				// 		result = 1;
				// 	}

				// 	Player.hand_item.current_item.count -= result;

				// 	if (count == result) {
				// 		Player.hand_item.clear_item();
				// 	} else {
				// 		Player.hand_item.current_item.emit_update();
				// 		Player.hand_item.update_visualization();
				// 	}

				// } else {
				// 	if (current_item != null) {
				// 		int previous_count = current_item.count;
				// 		int count = (int) Math.Ceiling(current_item.count / 2.0);
				// 		string name = current_item.name;
				// 		current_item.count -= count;

				// 		GD.Print(previous_count - count);
						
				// 		if (previous_count - count > 0) {
				// 			GD.Print("emitting update???");
				// 			current_item.emit_update();
				// 		}

				// 		Player.hand_item.set_item(InventoryItem.new_item(name, count), null);
				// 	}
				// }
			}
		}
	}

	public static FluidRepresentation get_first_available_instance () {
		if (available_instances.Count > 0) {
			FluidRepresentation result = available_instances.Pop();
			if (result.GetParent() != null) {
				GD.Print("Fluid rep pushed with parent");
			}
			return result;
		}

		return null;
	}

	public override void release () {
		GD.Print("Fluid Rep getting released?");

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