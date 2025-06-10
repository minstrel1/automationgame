using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class CategoryListTab : GUI {

	public static Array<CategoryListTab> instances = new Array<CategoryListTab>();
	public static Stack<CategoryListTab> available_instances = new Stack<CategoryListTab>();

	public CategoryList parent_category_list;
	
	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/category_list_tab.tscn");

	public TextureRect icon;
	public Label text;

	public String category;
	public CategoryListMode mode;
	public Dictionary category_data;

	public static CategoryListTab make (CategoryList category_parent, string category, CategoryListMode mode, Control parent) {
		CategoryListTab new_rep = get_first_available_instance();

		if (new_rep == null) {
			new_rep = scene.Instantiate<CategoryListTab>();
			instances.Add(new_rep);
		} else {
			if (new_rep.GetParent() != null) {
				GD.Print("WHATT HE FUCK>?>???");
			}
		}

		new_rep.gui_parent = parent;
		new_rep.parent_category_list = category_parent;
		new_rep.mode = mode;
		new_rep.category = category;

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

		icon = GetNode<TextureRect>("Control/Control");
		text = GetNode<Label>("Control/Label");

		switch (mode) {
			case CategoryListMode.Buildings:
				category_data = (Dictionary) Prototypes.building_category_properties[category];
				break;

			case CategoryListMode.Recipes:
				category_data = (Dictionary) Prototypes.recipe_category_properties[category];
				break;
		}

		icon.Texture = GD.Load<Texture2D>((string) category_data["icon_texture"]);
		text.Text = (string) category_data["display_name"];

		update_visualization();
	
	}

	public void clear () {
		gui_parent = null;
		parent_category_list = null;
		category = "";
	}

	public void update_visualization () {
		// if (data != null) {
		// 	icon.Texture = (Texture2D) data["display_icon"];
		// 	text.Text = (string) data["display_name"];
		// }
	}

	public override void _GuiInput(InputEvent @event)
	{
		base._GuiInput(@event);


		if (@event is InputEventMouse) {
			InputEventMouse mouse_event = @event as InputEventMouse;

			
			
		}

		if (@event is InputEventMouseButton) {
			InputEventMouseButton mouse_event = @event as InputEventMouseButton;

			if (mouse_event.Pressed) {
				parent_category_list.current_category = category;

				parent_category_list.update_element_visualization();
			}
		}
	}

	public static CategoryListTab get_first_available_instance () {
		if (available_instances.Count > 0) {
			CategoryListTab result = available_instances.Pop();
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