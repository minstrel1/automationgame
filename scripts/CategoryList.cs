using System;
using Godot;
using Godot.Collections;

public enum CategoryListMode {
	Buildings,
	Recipes,
}

public partial class CategoryList : GUI {


	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/category_list.tscn");

	public Array<CategoryListElement> elements = new Array<CategoryListElement>();

	[Signal]
	public delegate void OnChoiceSelectedEventHandler (Variant data);

	public GridContainer element_container;
	public VBoxContainer tab_container;

	public Control lower_view;
	public InventoryGUI lower_inventory_gui;
	public Control lower_inventory_parent;

	public Label lower_view_name;
	public TextureRect lower_view_icon;
	public RichTextLabel lower_view_desc;

	public Dictionary building_data;
	public Dictionary<string, Godot.Collections.Array<RecipePrototype>> recipe_data;


	public CategoryListMode mode;
	public Inventory lower_inventory;

	public string current_category = "agriculture";

	public CategoryListElement current_element = null;
	public string current_element_text = "";


	public override void _Ready()
	{
		base._Ready();

		element_container = GetNode<GridContainer>("VBoxContainer/HBoxContainer/Control2/PanelContainer/ScrollContainer/GridContainer");
		tab_container = GetNode<VBoxContainer>("VBoxContainer/HBoxContainer/Control/Control3");

		lower_view = GetNode<Control>("VBoxContainer/LowerView");
		lower_view_name = GetNode<Label>("VBoxContainer/LowerView/Control/Label");
		lower_view_icon = GetNode<TextureRect>("VBoxContainer/LowerView/Control/Control");
		lower_view_desc = GetNode<RichTextLabel>("VBoxContainer/LowerView/RichTextLabel");

		Array<GUIDummy> inventory_result = pop_dummy_type("InventoryGUI");
		GUIDummyData result = remove_dummy(inventory_result[0]);

		lower_inventory_parent = result.parent;

		update_element_visualization();
		update_tab_visualization();
		update_lower_visualization();
	}

	public static CategoryList make (CategoryListMode mode, Variant data, Control gui_parent) {
		CategoryList new_instance = scene.Instantiate<CategoryList>();

		new_instance.gui_parent = gui_parent;
		new_instance.mode = mode;

		switch (mode) {
			case CategoryListMode.Buildings:
				new_instance.building_data = (Dictionary) data;
				new_instance.current_category = "agriculture";
				break;
			
			case CategoryListMode.Recipes:
				new_instance.recipe_data = (Dictionary<string, Godot.Collections.Array<RecipePrototype>>) data;
				new_instance.current_category = "basic_crafting";
				break;
		}

		// foreach (string category_name in data.Keys) {
		// 	if (((Godot.Collections.Array) data[category_name]).Count > 0) {
		// 		new_instance.current_category = category_name;
		// 		break;
		// 	}
		// }

		new_instance.gui_parent.AddChild(new_instance);

		return new_instance;
	}

	public void update_element_visualization () {
		foreach (CategoryListElement element in elements) {
			element.release();
		}

		elements.Clear();

		switch (mode) {
			case CategoryListMode.Buildings:
				foreach (Dictionary element_data in ((Godot.Collections.Array) building_data[current_category])) {
					elements.Add(CategoryListElement.make(this, element_data, mode, element_container));
				}

				break;

			case CategoryListMode.Recipes:
				foreach (RecipePrototype recipe_data in recipe_data[current_category]) {
					elements.Add(CategoryListElement.make(this, recipe_data, mode, element_container));
				}
				break;
		}

		
	}

	public void update_tab_visualization () {
		switch (mode) {
			case CategoryListMode.Buildings:
				foreach (String category_name in building_data.Keys) {
					if (((Godot.Collections.Array) building_data[category_name]).Count > 0) {
						CategoryListTab.make(this, category_name, mode, tab_container);
					}
				}
				break;

			case CategoryListMode.Recipes:
				foreach (String category_name in recipe_data.Keys) {
					if (recipe_data[category_name].Count > 0) {
						CategoryListTab.make(this, category_name, mode, tab_container);
					}
				}
				break;
		}
	}

	public void update_lower_visualization () {
		if (lower_inventory_gui != null) {
			lower_inventory_gui.release();
		}
		lower_inventory_gui = null;

		if (lower_inventory != null) {
			lower_inventory.destroy();
		}
		lower_inventory = null;

		if (current_element == null) {
			lower_view.Visible = false;
		} else {
			lower_view.Visible = true;

			int cost_count = 0;
			
			switch (mode) {
				case CategoryListMode.Buildings:
					Dictionary element_data = current_element.building_data;

					cost_count = ((Godot.Collections.Array) element_data["building_cost"]).Count;
					
					lower_inventory = new Inventory(cost_count);

					foreach (Dictionary item in (Godot.Collections.Array) element_data["building_cost"]) {
						lower_inventory.insert(new InventoryItem((string) item["name"], (int) item["amount"]));
					}

					lower_inventory_gui = InventoryGUI.make(lower_inventory, lower_inventory_parent, false);

					lower_inventory_gui.CustomMinimumSize = new Vector2((cost_count * 42) + ((cost_count - 1) * 4), 42);
					lower_inventory_gui.Size = new Vector2((cost_count * 42) + ((cost_count - 1) * 4), 42);

					lower_view_name.Text = (string) element_data["display_name"];
					lower_view_desc.Text = (string) element_data["display_description"];
					lower_view_icon.Texture = (Texture2D) element_data["display_icon"];
					break;

				case CategoryListMode.Recipes:
					RecipePrototype recipe_data = current_element.recipe_data;

					lower_view_name.Text = recipe_data.display_name;
					lower_view_desc.Text = recipe_data.display_description;
					lower_view_icon.Texture = GD.Load<Texture2D>(recipe_data.icon_texture);
					break;
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}

	public void choice_selected (Variant data) {
		EmitSignal(CategoryList.SignalName.OnChoiceSelected, data);
	}

	public override void release()
	{
		foreach (CategoryListElement element in elements) {
			element.release();
		}

		base.release();
	}

	public override void _ExitTree()
	{
		base._ExitTree();

	}

}