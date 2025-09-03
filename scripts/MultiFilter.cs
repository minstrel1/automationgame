using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class MultiFilter : FilterBase {

    public Array<string> filters = new Array<string>();
    public Dictionary<string, bool> filter_dict = new Dictionary<string, bool>();

    public String prototype_type = "items";

    [Signal]
    public delegate void OnFiltersChangedEventHandler (Array<string> filters);

    public int max_filters = 5;

    public MultiFilter () {

    }

    public MultiFilter (Array<String> new_filters, String prototype_type = "items", int max_filters = 5) {
        this.max_filters = max_filters;
        filters.Resize(this.max_filters);

        this.prototype_type = prototype_type;

        this.set_filters(new_filters);
    }

    public void on_filters_changed () {
        filter_dict.Clear();

        foreach (String filter in filters) {
            if (filter != "") {
                filter_dict[filter] = true;
            }
        }

        EmitSignal(SignalName.OnFiltersChanged, filters);
    }

    public void set_filters (Array<String> new_filters) {
        int index = 0;
        foreach (String filter in new_filters) {
            if (index < this.max_filters) {
                filters[index] = filter;
            }
        }

        on_filters_changed();
    }

    public override bool match(string test) {
        return filter_dict.ContainsKey(test);
    }
}