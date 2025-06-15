using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using Limbo.Console.Sharp;

public partial class Player : CharacterBody3D {

	[ConsoleCommand("give_item", "tries to insert items into the player's inventory.")]
	public void dingaling (string item_name, int count) {
        if (Prototypes.items.ContainsKey(item_name)) {
            int result = inventory.insert(new SimpleItem{name = item_name, count = count});
            LimboConsole.Info(String.Format("{0} {1} was inserted.", result, item_name));
        } else {
            LimboConsole.Warn(String.Format("{0} is not a valid item name.", item_name));
        }
	}

    
}