using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public struct SimpleItem {
	public string name;
	public int count;
}

[GlobalClass]
public partial class InventoryItem : Node {

	[Signal]
	public delegate void OnCountChangedEventHandler ();

	[Signal]
	public delegate void OnPreDestoryEventHandler ();

	public string name = "item";
	public int count {
		get {
			return field;
		}
		set {
			field = value;
			if (field == 0) {
				prep_for_deletion();
				QueueFree();
			}
		}
	} = 1;
	public int stack_size = 1;
	public ItemPrototype prototype;
	public Inventory parent_inventory;
	public int current_index = -1;

	public InventoryItem (string new_name, int new_count) {
		if (Prototypes.items.ContainsKey(new_name)) {
			ItemPrototype data = Prototypes.items[new_name];
			if (new_count <= 0) {
				throw new Exception("Items cannot be initialized with counts 0 or less.");
			}

			this.prototype = data;

			this.name = (string) data.name;
			this.count = new_count;
			this.stack_size = (int) data.stack_size;
		} else {
			GD.Print("This item doesn't exist.");
			
		}
	}

	public static InventoryItem new_item (string new_name, int new_count) {
		return new InventoryItem(new_name, new_count);
	}

	public InventoryItem clone () {
		InventoryItem new_instance = (InventoryItem) MemberwiseClone();

		return new_instance;
	}

	public override string ToString()
	{
		return String.Format("Item Name: {0}, Count {1}", name, count);
	}

	public override void _Ready()
	{
		base._Ready();
	}

	public int add (InventoryItem other_item) {
		int before_count = count;
		count = Math.Min(before_count + other_item.count, stack_size);
		int difference = count - before_count;
		other_item.count -= difference;
		emit_update();
		other_item.emit_update();
		return difference;
	}

	public void prep_for_deletion () {
		if (parent_inventory != null) {
			parent_inventory.remove_item(this);
		} else {
			//GD.Print("we aint got no fuckin parent inventory");
		}
	}

	public void emit_update () {
		if (parent_inventory != null) {
			parent_inventory.EmitSignal(Inventory.SignalName.OnItemSlotChanged, current_index, this);
			parent_inventory.EmitSignal(Inventory.SignalName.OnInventoryChanged, parent_inventory);
		} else {
			//GD.Print("we aint got no fuckin parent inventory");
		}
	}
}