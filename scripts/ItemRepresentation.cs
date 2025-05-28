using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class ItemRepresentation : GUI {

	public static Array<ItemRepresentation> instances = new Array<ItemRepresentation>();
	public static Stack<ItemRepresentation> available_instances = new Stack<ItemRepresentation>();

	public Inventory parent_inventory;
	public int current_index;
	public InventoryItem current_item;
	
	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/item_representation.tscn");

	public TextureRect background;
	public TextureRect item_texture;
	public Label item_count;

	public bool interactable = true;

	public static ItemRepresentation make_item_representation (int current_index, Inventory parent_inventory, Control parent, bool interactable = true) {
		ItemRepresentation new_rep = get_first_available_instance();

		if (new_rep == null) {
			new_rep = scene.Instantiate<ItemRepresentation>();
			instances.Add(new_rep);
		} else {
			if (new_rep.GetParent() != null) {
				GD.Print("WHATT HE FUCK>?>???");
			}
		}

		new_rep.parent_inventory = parent_inventory;
		new_rep.gui_parent = parent;
		new_rep.current_index = current_index;
		new_rep.in_use = true;
		new_rep.interactable = interactable;

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
		item_count = GetNode<Label>("ItemCount");

		parent_inventory.OnItemSlotChanged += on_inventory_slot_changed;

		if (parent_inventory.get_item_at_index(current_index) != null) {
			this.current_item = parent_inventory.get_item_at_index(current_index);
		}

		update_visualization();
	
	}

	public void clear () {
		gui_parent = null;

		if (parent_inventory != null) {
			parent_inventory.OnItemSlotChanged -= on_inventory_slot_changed;
		}

		parent_inventory = null;
		current_index = 0;
		current_item = null;
	}

	public void on_inventory_slot_changed (int index, InventoryItem item) {
		if (index == current_index) {
			current_item = item;

			update_visualization();
		}
		
	}

	public void update_visualization () {
		if (current_item != null) {
			string texture_path = (string) current_item.prototype["icon_texture"];
			item_texture.Texture = GD.Load<Texture2D>(texture_path);

			item_count.Text = current_item.count.ToString();
		} else {
			item_count.Text = "";
			item_texture.Texture = null;
		}
	}

	public override void _GuiInput(InputEvent @event)
	{
		base._GuiInput(@event);

		if (@event is InputEventMouseButton && interactable) {
			InputEventMouseButton mouse_event = @event as InputEventMouseButton;

			if (mouse_event.ButtonIndex == MouseButton.Left && mouse_event.Pressed) {
				if (Input.IsActionPressed("shift_modifier")) {
					if (current_item != null) {

						int result;
						if (current_item.parent_inventory == Player.instance.inventory) {
							if (Player.instance.active_inventory != null) {
								result = Player.instance.active_inventory.insert(current_item);
							}
						} else {
							result = Player.instance.inventory.insert(current_item);
						}

					}
				} else if (Player.hand_item.has_item()) {
					if (current_item != null && Player.hand_item.current_item.name == current_item.name) {
						int count = Player.hand_item.current_item.count;
						int result = current_item.add(Player.hand_item.current_item);

						if (count == result) {
							Player.hand_item.clear_item();
						} else {
							Player.hand_item.update_visualization();
						}
					} else {
						InventoryItem temp_item = current_item;
						int temp_index = Player.hand_item.current_index;
						Inventory temp_inventory = Player.hand_item.current_inventory;

						current_item = Player.hand_item.current_item;
						parent_inventory.set_item(current_item, current_index);

						if (temp_inventory != null) {
							temp_inventory.set_item(temp_item, temp_index);
							//Player.hand_item.set_item(temp_item, temp_inventory);
							Player.hand_item.clear_item();
						} else {
							Player.hand_item.set_item(null, null);
						}
					}
				} else {
					Player.hand_item.set_item(current_item, parent_inventory);
				}
			}

			if (mouse_event.ButtonIndex == MouseButton.Right && mouse_event.Pressed) {
				if (Player.hand_item.has_item()) {

					int count = Player.hand_item.current_item.count;
					int result = 0;

					if (current_item != null && Player.hand_item.current_item.name == current_item.name) {
						result = current_item.add(InventoryItem.new_item(Player.hand_item.current_item.name, 1));
					} else if (current_item == null) {
						parent_inventory.set_item(InventoryItem.new_item(Player.hand_item.current_item.name, 1), current_index);
						result = 1;
					}

					Player.hand_item.current_item.count -= result;

					if (count == result) {
						Player.hand_item.clear_item();
					} else {
						Player.hand_item.current_item.emit_update();
						Player.hand_item.update_visualization();
					}

				} else {
					if (current_item != null) {
						int previous_count = current_item.count;
						int count = (int) Math.Ceiling(current_item.count / 2.0);
						string name = current_item.name;
						current_item.count -= count;

						GD.Print(previous_count - count);
						
						if (previous_count - count > 0) {
							GD.Print("emitting update???");
							current_item.emit_update();
						}

						Player.hand_item.set_item(InventoryItem.new_item(name, count), null);
					}
				}
			}
		}
	}

	public static ItemRepresentation get_first_available_instance () {
		if (available_instances.Count > 0) {
			ItemRepresentation result = available_instances.Pop();
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