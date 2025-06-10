using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
public partial class Inventory : Node {

	public Array<InventoryItem> contents = new Array<InventoryItem>();
	public Array<FilterBase> filters = new Array<FilterBase>();

	[Signal]
	public delegate void OnItemSlotChangedEventHandler (int index, InventoryItem item);

	[Signal]
	public delegate void OnInventoryChangedEventHandler (Inventory inventory);

	public int inventory_size = 1;

	public Inventory () {
		contents.Resize(10);
		filters.Resize(10);
		inventory_size = 10;
	}
	
	public Inventory (int size = 10) {
		contents.Resize(size);
		filters.Resize(size);
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

	public int get_first_slot_to_insert (String item_name, int start = 0) {
		for (int i = start; i < contents.Count; i++) {
			if (contents[i] != null) {
				if (contents[i].name == item_name && contents[i].count < contents[i].stack_size) {
					return i;
				}
			} else {
				if (filters[i] == null || (filters[i].match(item_name))) {
					return i;
				}
			}
		}
		return -1;
	}

	public int get_last_slot_to_remove (String item_name, int start = -1) {
		if (start == -1) {
			start = contents.Count - 1;
		}
		for (int i = start; i >= 0; i--) {
			if (contents[i] != null) {
				if (contents[i].name == item_name) {
					return i;
				}
			}
		}

		return -1;
	}

	public int get_first_item (FilterBase filter, int start = 0) {
		for (int i = start; i < contents.Count; i++) {
			if (contents[i] != null) {
				if (filter.match(contents[i].name)) {
					return i;
				}
			}
		}

		return -1;
	}

	public int get_last_item (FilterBase filter, int start = -1) {
		if (start == -1) {
			start = contents.Count - 1;
		}

		for (int i = start; i >= 0; i--) {
			if (contents[i] != null) {
				if (filter.match(contents[i].name)) {
					return i;
				}
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
					pos_to_insert = get_first_slot_to_insert(item.name, pos_to_insert + 1);
				}

			}
			
		}

		bool changed = false;
		foreach (int index in changed_indexes) {
			EmitSignal(SignalName.OnItemSlotChanged, index, contents[index]);
			changed = true;
		}
		
		if (changed) {
			EmitSignal(SignalName.OnInventoryChanged, this);
		}

		if (item != null) {
			item.emit_update();
		}

		return items_inserted;
	}

	public int insert (SimpleItem item) {
		return insert(new InventoryItem(item.name, item.count));
	}

	public int insert (Array<InventoryItem> items) {
		int total = 0;
		foreach (InventoryItem item in items) {
			total += insert(item);
		}

		return total;
	}

	public int insert (SimpleItem[] items) {
		int total = 0;
		foreach (SimpleItem item in items) {
			total += insert(item);
		}

		return total;
	}

	public int remove (InventoryItem item) {
		int pos_to_remove = get_last_slot_to_remove(item.name);
		if (pos_to_remove == -1) { return 0; }

		int items_removed = 0;
		int before_count = 0;

		int starting_amount = item.count;

		Array<int> changed_indexes = new Array<int>();

		while (pos_to_remove != -1) {
			if (contents[pos_to_remove].count >= item.count) {
				contents[pos_to_remove].count -= item.count;
				items_removed += item.count;
				
				item.count = 0;

				changed_indexes.Add(pos_to_remove);
				break;
			} else {
				before_count = contents[pos_to_remove].count;
				contents[pos_to_remove].count = 0;

				item.count -= before_count;
				items_removed -= before_count;

				changed_indexes.Add(pos_to_remove);

				pos_to_remove = get_last_slot_to_remove(item.name, pos_to_remove - 1);
			}
		}

		bool changed = false;
		foreach (int index in changed_indexes) {
			EmitSignal(SignalName.OnItemSlotChanged, index, contents[index]);
			changed = true;
		}
		
		if (changed) {
			EmitSignal(SignalName.OnInventoryChanged, this);
		}

		if (item != null) {
			item.emit_update();
		}

		return items_removed;
	}

	public bool can_insert (InventoryItem item) {
		int pos_to_insert = get_first_slot_to_insert(item.name);
		if (pos_to_insert == -1) { return false; }

		int current_count = item.count;
		int starting_count = item.count;
		int items_inserted = 0;
		int difference = 0;
		int before_count = 0;
		int new_count = 0;

		while (pos_to_insert != -1) {

			if (contents[pos_to_insert] == null) {

				items_inserted += current_count;
				current_count -= current_count;

				break;
			} else {
				before_count = contents[pos_to_insert].count;
				new_count = Math.Min(before_count + current_count, contents[pos_to_insert].stack_size);
				difference = new_count - before_count;
				items_inserted += difference;

				current_count -= difference;

				if (items_inserted >= starting_count) {
					if (items_inserted > starting_count) {
						GD.PrintErr("A can_insert operation put more items in than started with.");
					}
					
					break;
				} else {
					pos_to_insert = get_first_slot_to_insert(item.name, pos_to_insert + 1);
				}

			}
			
		}

		return items_inserted == starting_count;
	}

	public bool can_insert (Array<InventoryItem> items) {
		foreach (InventoryItem item in items) {
			if (!can_insert(item)) {
				return false;
			}
		}
		return true;
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
		EmitSignal(SignalName.OnInventoryChanged, this);

	}

	public void remove_item (InventoryItem item){
		int index = get_index(item);
		if (index != -1) {
			contents[index].parent_inventory = null;
			contents[index].current_index = 0;
			contents[index] = null;
			EmitSignal(SignalName.OnItemSlotChanged, index, contents[index]);
			EmitSignal(SignalName.OnInventoryChanged, this);
		}
	}

	public void set_filter (FilterBase filter, int index) {
		filters[index] = filter;
	}

	public FilterBase get_filter (int index) {
		return filters[index];
	}

	public void remove_filter (int index) {
		filters[index] = null;
	}

	public void destroy () {
		foreach (InventoryItem item in contents) {
			item.QueueFree();
		}

		this.QueueFree();
	}

	public void resize (int size = 10) {
		contents.Resize(size);
		filters.Resize(size);
		inventory_size = size;
	}

	public void emit_update () {
		EmitSignal(SignalName.OnInventoryChanged, this);
	}

}