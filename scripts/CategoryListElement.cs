using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class CategoryListElement : GUI {

	public static Array<CategoryListElement> instances = new Array<CategoryListElement>();
	public static Stack<CategoryListElement> available_instances = new Stack<CategoryListElement>();

	public CategoryList parent_category_list;
	
	public static PackedScene scene = GD.Load<PackedScene>("res://gui_scenes/category_list_element.tscn");

	public TextureRect icon;
	public Label text;

	public Dictionary data;

	public static CategoryListElement make (CategoryList category_parent, Dictionary data, Control parent) {
		CategoryListElement new_rep = get_first_available_instance();

		if (new_rep == null) {
			new_rep = scene.Instantiate<CategoryListElement>();
			instances.Add(new_rep);
		} else {
			if (new_rep.GetParent() != null) {
				GD.Print("WHATT HE FUCK>?>???");
			}
		}

		new_rep.gui_parent = parent;
		new_rep.parent_category_list = category_parent;
		new_rep.data = data;

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

		icon = GetNode<TextureRect>("VBoxContainer/Control2/Control");
		text = GetNode<Label>("VBoxContainer/Label");

		update_visualization();
	
	}

	public void clear () {
		gui_parent = null;
		parent_category_list = null;
		data = null;
	}

	public void update_visualization () {
		if (data != null) {
			icon.Texture = (Texture2D) data["display_icon"];
			text.Text = (string) data["display_name"];
		}
	}

	public override void _GuiInput(InputEvent @event)
	{
		base._GuiInput(@event);


		if (@event is InputEventMouse) {
			InputEventMouse mouse_event = @event as InputEventMouse;

			if (parent_category_list.current_element != this) {
				parent_category_list.current_element = this;
				parent_category_list.update_lower_visualization();
			}
			
		}

		if (@event is InputEventMouseButton) {
			InputEventMouseButton mouse_event = @event as InputEventMouseButton;

			if (mouse_event.Pressed) {
				AcceptEvent();
				parent_category_list.choice_selected(data);
			}
		}
	}

	public static CategoryListElement get_first_available_instance () {
		if (available_instances.Count > 0) {
			CategoryListElement result = available_instances.Pop();
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