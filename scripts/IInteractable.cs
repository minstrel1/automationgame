using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public interface IInteractable {
	void on_hover_focus ();

	void on_hover_unfocus ();

	void on_interact ();

	string get_interact_text ();

}