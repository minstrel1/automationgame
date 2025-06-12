using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
#if TOOLS
[Tool]
#endif
public partial class Crafter : BuildingGridPlacable, IBuildingWithInventory, IInteractable {

	[ExportCategory("Crafter Properties")]
	[Export]
	public string interact_name = "Crafter";
	[Export]
	public float crafting_speed = 1.0f;

	public Inventory input_inventory;
	public Inventory output_inventory;

	bool check_input = true;
	bool check_output = true;

	public RecipePrototype current_recipe;
	bool recipe_set = false;

	int ingredient_count = Prototypes.max_recipe_ingredients;
	int[] ingredient_counts = new int[Prototypes.max_recipe_ingredients];
	bool[] is_item = new bool[Prototypes.max_recipe_ingredients];

	bool waiting_to_output = false;
	Array<InventoryItem> to_insert_items = new Array<InventoryItem>();

	public double current_crafting_time = 0d;
	public bool crafting = false;

	NoneFilter none_filter;

	CsgShape3D collider;

	public override void _Ready() {
		base._Ready();

		input_inventory = new Inventory(Prototypes.max_recipe_ingredients);
		input_inventory.OnInventoryChanged += on_input_inventory_changed;

		output_inventory = new Inventory(Prototypes.max_recipe_ingredients);

		ingredient_counts = new int[Prototypes.max_recipe_ingredients];

		is_item = new bool[Prototypes.max_recipe_ingredients];

		foreach (SpecialVoxel voxel in special_voxels.Values) {
			if (voxel.voxel_flags == SpecialVoxelFlags.ItemInput) {
				((ItemSpecialVoxel) voxel).set_inventory(input_inventory);
			} else if (voxel.voxel_flags == SpecialVoxelFlags.ItemInput) {
				((ItemSpecialVoxel) voxel).set_inventory(output_inventory);
			}
		}

		// ((ItemSpecialVoxel) special_voxels["input1"]).set_inventory(input_inventory);
		// ((ItemSpecialVoxel) special_voxels["input2"]).set_inventory(input_inventory);
		// ((ItemSpecialVoxel) special_voxels["input3"]).set_inventory(input_inventory);
		// ((ItemSpecialVoxel) special_voxels["output1"]).set_inventory(output_inventory);
		// ((ItemSpecialVoxel) special_voxels["output2"]).set_inventory(output_inventory);

		none_filter = new NoneFilter();

		collider = GetNode<CsgBox3D>("CSGBox3D");
	}

	public override void on_build()
	{
		base.on_build();

		((ItemSpecialVoxel) special_voxels["input1"]).auto_input = false;
		((ItemSpecialVoxel) special_voxels["input2"]).auto_input = false;
		((ItemSpecialVoxel) special_voxels["input3"]).auto_input = false;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (recipe_set) {
			if (crafting) {
				if (current_crafting_time < current_recipe.time_to_craft) {
					current_crafting_time += crafting_speed * delta;
				} else {
					//GD.Print("product finished!");
					if (!waiting_to_output) {
						(Array<InventoryItem>, int) results = current_recipe.get_products();

						to_insert_items = results.Item1;

						waiting_to_output = true;
					}
					if (check_output) {
						if (output_inventory.can_insert(to_insert_items)) {
							output_inventory.insert(to_insert_items);

							waiting_to_output = false;
							check_input = true;
							crafting = false;
							current_crafting_time = 0;
						} 

						check_output = false;
					}
				}
			}
			
			if (check_input) {
				//GD.Print("checking crafter input");
				//GD.Print(input_inventory);
				bool input_valid = true;
				for (int i = 0; i < ingredient_count; i++) {
					if (is_item[i]) {
						//GD.Print(i.ToString() + " is an item");
						if (input_inventory.contents[i] == null || input_inventory.contents[i].count < ingredient_counts[i]) {
							input_valid = false;
						}
					}
				}

				//GD.Print(input_valid);

				if (input_valid && !crafting) {
					for (int i = 0; i < ingredient_count; i++) {
						if (is_item[i]) {
							input_inventory.contents[i].count -= ingredient_counts[i];
							if (input_inventory.contents[i] != null) {
								input_inventory.contents[i].emit_update();
							}
						}
					}

					//GD.Print(input_inventory);

					crafting = true;
					check_output = true;
					current_crafting_time = 0;
				}

				check_input = false;
			}
		}
	}

	public void on_input_inventory_changed (Inventory inventory) {
		check_input = true;
	}

	public void on_output_inventory_changed (Inventory inventory) {
		check_output = true;
	}


	public Inventory get_input_inventory () {
		return input_inventory;
	}

	public Inventory get_output_inventory () {
		return output_inventory;
	}

	public override void set_collision(bool value)
	{
		collider.UseCollision = value;
	}

	public void on_hover_focus () {
		set_mesh_visibility(true);
	}

	public void on_hover_unfocus () {
		set_mesh_visibility(false);
	}

	public void clear_recipe (bool transfer_to_player = false) {
		if (transfer_to_player) {
			foreach (InventoryItem item in output_inventory.contents) {
				if (item != null) {
					Player.instance.inventory.insert(item);
				}
			}

			foreach (InventoryItem item in input_inventory.contents) {
				if (item != null) {
					Player.instance.inventory.insert(item);
				}
			}

			if (crafting && current_crafting_time < current_recipe.time_to_craft) {
				for (int i = 0; i < ingredient_count; i++) {
					if (input_inventory.filters[i] != null) {
						// This is janky asf and i feel like this could be redone somehow
						InventoryItem new_item = InventoryItem.new_item(((ItemFilter)input_inventory.filters[i]).name, ingredient_counts[i]);
						Player.instance.inventory.insert(new_item);
					}
				} 
			}
		}

		input_inventory.resize(0);
		output_inventory.resize(0);

		crafting = false;

		current_recipe = null;
		recipe_set = false;

		((ItemSpecialVoxel) special_voxels["input1"]).auto_input = false;
		((ItemSpecialVoxel) special_voxels["input2"]).auto_input = false;
		((ItemSpecialVoxel) special_voxels["input3"]).auto_input = false;
	}

	public void on_recipe_selected (Variant data) {
		current_recipe = (RecipePrototype) data;
		GD.Print(current_recipe);

		(Array<InventoryItem>, int) ingredients = current_recipe.get_ingredients();
		Array<InventoryItem> ingredient_items = ingredients.Item1;

		ingredient_count = current_recipe.ingredients.Count;

		input_inventory.resize(ingredient_count);
		ingredient_counts = new int[ingredient_count];
		is_item = new bool[ingredient_count];

		int index = 0;
		foreach (InventoryItem item in ingredient_items) {
			GD.Print(item);
			if (item != null) {
				GD.Print(index.ToString() + " is an item");
				input_inventory.set_filter(new ItemFilter(item.name), index);
				ingredient_counts[index] = item.count;
				is_item[index] = true;
			} else {
				input_inventory.set_filter(none_filter, index);
				is_item[index] = false;
			}

			index += 1;
		}

		(Array<InventoryItem>, int) products = current_recipe.get_products();
		Array<InventoryItem> product_items = products.Item1;

		int product_count = current_recipe.products.Count;

		output_inventory.resize(product_count);

		index = 0;
		foreach (InventoryItem item in product_items) {
			if (item != null) {
				output_inventory.set_filter(new ItemFilter(item.name), index);
			} else {
				output_inventory.set_filter(none_filter, index);
			}

			index += 1;
		}

		recipe_set = true;

		((ItemSpecialVoxel) special_voxels["input1"]).auto_input = true;
		((ItemSpecialVoxel) special_voxels["input2"]).auto_input = true;
		((ItemSpecialVoxel) special_voxels["input3"]).auto_input = true;

		GD.Print(ingredient_counts);

		Player.set_active_gui(CrafterGUI.make(this, Player.instance.gui_parent));
	}

	public void make_recipe_gui () {
		CategoryList new_instance = CategoryList.make(CategoryListMode.Recipes, Prototypes.get_recipes_categorized(), Player.instance.gui_parent);
		new_instance.OnChoiceSelected += on_recipe_selected;
		Player.set_active_gui(new_instance);
	}

	public void on_interact () {
		if (is_built) {
			if (Player.instance.active_gui is CategoryList) {
				Player.instance.clear_active_gui();
			} else {
				if (current_recipe == null) {
					make_recipe_gui();
				} else {
					Player.set_active_gui(CrafterGUI.make(this, Player.instance.gui_parent));
				}
			}
		}
	}

	public string get_interact_text() {
		return "Configure " + interact_name;
	}
}