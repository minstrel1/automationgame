using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
public partial class InteractableDummy : CsgBox3D, IInteractable {

	[Export]
	public Node3D interaction_parent;

	public void on_hover_focus () {
		if (interaction_parent is IInteractable) {
			(interaction_parent as IInteractable).on_hover_focus();
		}
	}

	public void on_hover_unfocus () {
		if (interaction_parent is IInteractable) {
			(interaction_parent as IInteractable).on_hover_unfocus();
		}
	}

	public void on_interact () {
		if (interaction_parent is IInteractable) {
			(interaction_parent as IInteractable).on_interact();
		}
	}

	public string get_interact_text () {
		if (interaction_parent is IInteractable) {
			return (interaction_parent as IInteractable).get_interact_text();
		} else {
			return "Dummy Text";
		}
	}
}