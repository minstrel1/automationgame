using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class HandItemRepresentation : Control {
	public static HandItemRepresentation instance;
	public InventoryItem current_item;
	public Inventory current_inventory;
	public int current_index;
	public TextureRect background;
	public TextureRect item_texture;
	public Label item_count;

	public override void _Ready()
	{
		background = GetNode<TextureRect>("Background");
		item_texture = GetNode<TextureRect>("ItemTexture");
		item_count = GetNode<Label>("ItemCount");

		Visible = false;

		instance = this;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		GlobalPosition = GetViewport().GetMousePosition();
	}

	public void set_item (InventoryItem item, Inventory inventory) {
		clear_item();

		current_inventory = inventory;
		current_item = item;

		if (current_inventory != null) {
			current_index = current_inventory.get_index(current_item);
			current_inventory.OnItemSlotChanged += on_inventory_slot_changed;
		} else {
			current_index = -1;
		}


		update_visualization();
	}

	public void clear_item () {
		current_item = null;
		current_index = -1;
		if (current_inventory != null) {
			current_inventory.OnItemSlotChanged -= on_inventory_slot_changed;
		}

		update_visualization();
	}

	public void update_visualization () {
		if (current_item != null) {
			string texture_path = (string) current_item.prototype["icon_texture"];
			item_texture.Texture = GD.Load<Texture2D>(texture_path);

			item_count.Text = current_item.count.ToString();
			Visible = true;
		} else {
			item_count.Text = "";
			item_texture.Texture = null;
			Visible = false;
		}
	}

	public void on_inventory_slot_changed (int index, InventoryItem item) {
		if (index == current_index) {
			if (current_item != item) {
				clear_item();
			} else {
				update_visualization();
			}
		}
	}

	public bool has_item () {
		return current_item != null;
	}
}