using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class Player {
	public Control gui_parent;

	public GUI active_gui;
	public Inventory active_inventory;

	public bool is_in_gui () {
		return active_gui != null;
	}

	public void clear_active_gui (bool recapture_mouse = true) {
		if (is_in_gui()) {
			active_gui.release();
			active_gui = null;
		}
		if (recapture_mouse) {
			capture_mouse();
		}
	}

	public static void set_active_gui (GUI gui) {
		instance.clear_active_gui(false);
		instance.active_gui = gui;
		instance.release_mouse();
	}
}