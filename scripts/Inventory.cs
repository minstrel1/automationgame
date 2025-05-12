using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
public partial class Inventory : Node {

	public Array<InventoryItem> contents = new Array<InventoryItem>();

	[Signal]
	public delegate void OnItemSlotChangedEventHandler (int index, InventoryItem item);

	public int inventory_size = 1;

	public Inventory () {
		contents.Resize(10);
		inventory_size = 10;
	}
	
	public Inventory (int size = 10) {
		contents.Resize(size);
		inventory_size = size;
	}

	public static Inventory new_inventory (int size = 10) {
		return new Inventory(size);
	}

	public override void _Ready()
	{
		base._Ready();
	}

	public override string ToString()
	{
		return String.Format("Inventory of size {0}\nContents: {1}", contents.Count, contents.ToString());
	}

	public int get_first_slot_to_insert (String item_name) {
		for (int i = 0; i < contents.Count; i++) {
			if (contents[i] != null) {
				if (contents[i].name == item_name && contents[i].count < contents[i].stack_size) {
					return i;
				}
			} else {
				return i;
			}
		}
		return -1;
	}

	public int insert (InventoryItem item) {
		int pos_to_insert = get_first_slot_to_insert(item.name);
		if (pos_to_insert == -1) { return 0; }

		int items_inserted = 0;
		int difference = 0;
		int before_count = 0;

		int starting_amount = item.count;

		Array<int> changed_indexes = new Array<int>();

		while (pos_to_insert != -1) {

			if (contents[pos_to_insert] == null) {
				InventoryItem new_item = InventoryItem.new_item(item.name, item.count);
				contents[pos_to_insert] = new_item;
				new_item.parent_inventory = this;
				new_item.current_index = pos_to_insert;
				items_inserted += new_item.count;

				item.count -= new_item.count;

				changed_indexes.Add(pos_to_insert);

				break;
			} else {
				before_count = contents[pos_to_insert].count;
				contents[pos_to_insert].count = Math.Min(before_count + item.count, contents[pos_to_insert].stack_size);
				difference = contents[pos_to_insert].count - before_count;
				items_inserted += difference;

				changed_indexes.Add(pos_to_insert);
				item.count -= difference;

				if (items_inserted >= starting_amount) {
					if (items_inserted > starting_amount) {
						GD.PrintErr("An insert operation put more items in than started with.");
					}
					
					break;
				} else {
					pos_to_insert = get_first_slot_to_insert(item.name);
				}

			}
			
		}

		foreach (int index in changed_indexes) {
			EmitSignal(SignalName.OnItemSlotChanged, index, contents[index]);
		}

		return items_inserted;
	}

	public InventoryItem get_item_at_index (int index) {
		if (index >= 0 && index <= contents.Count - 1) {
			return contents[index];
		} else {
			return null;
		}
	}

	public int get_index (InventoryItem item) {
		if (contents.Contains(item)) {
			return contents.IndexOf(item);
		} else {
			return -1;
		}
	}

	public void set_item (InventoryItem item, int index) {
		contents[index] = item;
		if (item != null) {
			item.parent_inventory = this;
			item.current_index = index;
		}
		EmitSignal(SignalName.OnItemSlotChanged, index, contents[index]);
	}

	public void remove_item (InventoryItem item){
		int index = get_index(item);
		if (index != -1) {
			contents[index].parent_inventory = null;
			contents[index].current_index = 0;
			contents[index] = null;
			EmitSignal(SignalName.OnItemSlotChanged, index, contents[index]);
		}
	}

}