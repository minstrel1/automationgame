using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

public partial class Profiler : Node {
	private static Dictionary<string, double> measured_times = new Dictionary<string, double>();
	public static Dictionary<string, double> last_frame_times = new Dictionary<string, double>();

	private static Dictionary<string, Stopwatch> stopwatches = new Dictionary<string, Stopwatch>();

	private static ulong previous_frame_timestamp;
	private static ulong previous_frame_time;

	public override void _Ready() {
		base._Ready();

		GD.Print("timer frequency " + Stopwatch.Frequency.ToString());
		GD.Print("timer is high resolution " + Stopwatch.IsHighResolution.ToString());
	}

	public override void _PhysicsProcess(double delta) {
		last_frame_times = measured_times;
		measured_times = new Dictionary<string, double>();

		measured_times["profiler"] = 0;
		mark_time_begin("profiler");

		//previous_frame_timestamp = Time.GetTicksUsec();
		//CallDeferred(Profiler.MethodName.mark_frame_end);

		ulong ticks = Godot.Time.GetTicksUsec();

		mark_time_end("profiler");

		// foreach (String key in last_frame_times.Keys) {
		// 	GD.Print(key + " : " + last_frame_times[key].ToString());
		// }
	}

	private void mark_frame_end () {
		ulong now_time = Time.GetTicksUsec();

		previous_frame_time = now_time - previous_frame_timestamp;
	}

	public static void mark_time_begin (String name) {
		if (!stopwatches.ContainsKey(name)) {
			stopwatches[name] = new Stopwatch();
		}

		stopwatches[name].Restart();
	}

	public static void mark_time_end (String name) {
		if (stopwatches.ContainsKey(name)) {
			if (measured_times.ContainsKey(name)) {
				stopwatches[name].Stop();
				measured_times[name] += Math.Truncate(((double) stopwatches[name].ElapsedTicks / Stopwatch.Frequency) * (1000000)); 
			} else {
				stopwatches[name].Stop();
				measured_times[name] = Math.Truncate(((double) stopwatches[name].ElapsedTicks / Stopwatch.Frequency) * (1000000)); 
			}
		}
	}
}