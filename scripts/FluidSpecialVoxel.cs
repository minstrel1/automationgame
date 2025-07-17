using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

public partial class FluidSpecialVoxel : SpecialVoxel {
	
	public FluidContainer parent_container;

	public Dictionary<FluidContainer, SpecialVoxelFlags> connected_containers = new Dictionary<FluidContainer, SpecialVoxelFlags>();

	public float flow_rate = 600.0f;

	public bool connected_containers_cleared_this_frame = false;

	public bool force_update = false;

	public FluidSystem previous_system;

	public override void update_voxel_connections()
	{
		base.update_voxel_connections();

		ulong total_start = Time.GetTicksUsec();

		if (parent_container != null) {

			if (connections_changed_this_frame || force_update) {

				if (force_update) {
					GD.Print(name + " force updated");
					connected_containers_cleared_this_frame = true;
				}

				force_update = false;
				
				if (!connected_containers_cleared_this_frame) {
					connected_containers.Clear();
					connected_containers_cleared_this_frame = true;
				}

				if (parent_container == null || parent_container.connected_system == null) {
					GD.Print("something is null???");
					return;
				}

				switch (voxel_flags) {
					case SpecialVoxelFlags.FluidInputOutput: // combine with other systems
						// GD.Print("what the fuck");
						// GD.Print(connected_voxels);
						
						foreach (SpecialVoxel voxel in connected_voxels) {
							if (voxel is FluidSpecialVoxel) {
								FluidSpecialVoxel fluid_voxel = (FluidSpecialVoxel) voxel;

								switch (fluid_voxel.voxel_flags) {
									case SpecialVoxelFlags.FluidInputOutput:
										if (fluid_voxel.parent_container != null && fluid_voxel.parent_container.connected_system != null) {
											if (parent_container.connected_system != fluid_voxel.parent_container.connected_system) {
												GD.Print("found system to merge");
												if (parent_container.connected_system.is_system_compatible(fluid_voxel.parent_container.connected_system)) {

													if (fluid_voxel.parent_container.connected_system.connected_containers.Count >= parent_container.connected_system.connected_containers.Count) {
														fluid_voxel.parent_container.connected_system.merge_system(parent_container.connected_system);
													} else {
														parent_container.connected_system.merge_system(fluid_voxel.parent_container.connected_system);
													}

													add_connected_container(fluid_voxel.parent_container, SpecialVoxelFlags.FluidInputOutput);
													fluid_voxel.add_connected_container(parent_container, SpecialVoxelFlags.FluidInputOutput);
													
													//connected_containers[fluid_voxel.parent_container] = SpecialVoxelFlags.FluidInputOutput;
													//fluid_voxel.connected_containers[parent_container] = SpecialVoxelFlags.FluidInputOutput;

												} else {
													GD.Print("incompatible systems");
												}
												
											} else {
												connected_containers[fluid_voxel.parent_container] = SpecialVoxelFlags.FluidInputOutput;
											}										
										}

										break;

									case SpecialVoxelFlags.FluidInput:
										if (fluid_voxel.parent_container != null && fluid_voxel.parent_container.connected_system != null) {
											if (parent_container.connected_system != null) {
												if (parent_container.connected_system.is_system_compatible(fluid_voxel.parent_container.connected_system)) {
													if (connected_containers.ContainsKey(fluid_voxel.parent_container) && fluid_voxel.connected_containers.ContainsKey(parent_container)) {
														GD.Print("connection already exists, skipping");
													} else if (connected_containers.ContainsKey(fluid_voxel.parent_container) || fluid_voxel.connected_containers.ContainsKey(parent_container)) {
														GD.Print("uneven connection exists, check that out!");
														GD.Print(connected_containers);
														GD.Print(fluid_voxel.connected_containers);
														add_connected_container(fluid_voxel.parent_container, SpecialVoxelFlags.FluidOutput);
														fluid_voxel.add_connected_container(parent_container, SpecialVoxelFlags.FluidInput);
														
													} else {
														parent_container.connected_system.add_output(fluid_voxel.parent_container.connected_system);

														GD.Print("adding output from fluidIO???");

														add_connected_container(fluid_voxel.parent_container, SpecialVoxelFlags.FluidOutput);
														fluid_voxel.add_connected_container(parent_container, SpecialVoxelFlags.FluidInput);
													}
												}
											}
										}
										
										break;

									case SpecialVoxelFlags.FluidOutput:
										if (fluid_voxel.parent_container != null && fluid_voxel.parent_container.connected_system != null) {
											if (parent_container.connected_system != null) {
												if (fluid_voxel.parent_container.connected_system.is_system_compatible(parent_container.connected_system)) {
													if (connected_containers.ContainsKey(fluid_voxel.parent_container) && fluid_voxel.connected_containers.ContainsKey(parent_container)) {
														GD.Print("connection already exists, skipping");
													} else if (connected_containers.ContainsKey(fluid_voxel.parent_container) || fluid_voxel.connected_containers.ContainsKey(parent_container)) {
														GD.Print("uneven connection exists, check that out!");
														GD.Print(connected_containers.ContainsKey(fluid_voxel.parent_container));
														GD.Print(connected_containers);
														GD.Print(fluid_voxel.connected_containers.ContainsKey(parent_container));
														GD.Print(fluid_voxel.connected_containers);
														add_connected_container(fluid_voxel.parent_container, SpecialVoxelFlags.FluidInput);
														fluid_voxel.add_connected_container(parent_container, SpecialVoxelFlags.FluidOutput);
													} else {
														fluid_voxel.parent_container.connected_system.add_output(parent_container.connected_system);

														GD.Print("adding input from fluidIO???");

														add_connected_container(fluid_voxel.parent_container, SpecialVoxelFlags.FluidInput);
														fluid_voxel.add_connected_container(parent_container, SpecialVoxelFlags.FluidOutput);
													}
													

													//connected_containers[fluid_voxel.parent_container] = SpecialVoxelFlags.FluidInput;
													//fluid_voxel.connected_containers[parent_container] = SpecialVoxelFlags.FluidOutput;
												}
											}
										}
										
										break;
								}
							}
						}

						break;

					case SpecialVoxelFlags.FluidInput:
						foreach (SpecialVoxel voxel in connected_voxels) {
							if (voxel is FluidSpecialVoxel) {
								FluidSpecialVoxel fluid_voxel = (FluidSpecialVoxel) voxel;

								if (fluid_voxel.voxel_flags == SpecialVoxelFlags.FluidOutput || fluid_voxel.voxel_flags == SpecialVoxelFlags.FluidInputOutput) {
									if (fluid_voxel.parent_container != null && fluid_voxel.parent_container.connected_system != null) {
										
										if (connected_containers.ContainsKey(fluid_voxel.parent_container) && fluid_voxel.connected_containers.ContainsKey(parent_container)) {
											GD.Print("connection already exists, skipping");
										} else if (connected_containers.ContainsKey(fluid_voxel.parent_container) || fluid_voxel.connected_containers.ContainsKey(parent_container)) {
											GD.Print("uneven connection exists, check that out!");
											GD.Print(Name);
											GD.Print(connected_containers);
											GD.Print(fluid_voxel.Name);
											GD.Print(fluid_voxel.connected_containers);
											add_connected_container(fluid_voxel.parent_container, SpecialVoxelFlags.FluidInput);
											fluid_voxel.add_connected_container(parent_container, SpecialVoxelFlags.FluidOutput);
										} else {
											GD.Print("adding output from fluidINPUT???");

											GD.Print(parent_container.connected_system.connected_outputs);
											GD.Print(fluid_voxel.parent_container.connected_system.connected_inputs);

											fluid_voxel.parent_container.connected_system.add_output(parent_container.connected_system);

											GD.Print(parent_container.connected_system.connected_outputs);
											GD.Print(fluid_voxel.parent_container.connected_system.connected_inputs);

											GD.Print(connected_containers);
											GD.Print(fluid_voxel.connected_containers);

											add_connected_container(fluid_voxel.parent_container, SpecialVoxelFlags.FluidInput);
											fluid_voxel.add_connected_container(parent_container, SpecialVoxelFlags.FluidOutput);

											GD.Print(connected_containers);
											GD.Print(fluid_voxel.connected_containers);
											
										}
									}
								}
							}
						}
						break;

					case SpecialVoxelFlags.FluidOutput:
						foreach (SpecialVoxel voxel in connected_voxels) {
							if (voxel is FluidSpecialVoxel) {
								FluidSpecialVoxel fluid_voxel = (FluidSpecialVoxel) voxel;

								if (fluid_voxel.voxel_flags == SpecialVoxelFlags.FluidInput || fluid_voxel.voxel_flags == SpecialVoxelFlags.FluidInputOutput) {
									if (fluid_voxel.parent_container != null && fluid_voxel.parent_container.connected_system != null) {
										
										if (connected_containers.ContainsKey(fluid_voxel.parent_container) && fluid_voxel.connected_containers.ContainsKey(parent_container)) {
											GD.Print("connection already exists, skipping");
										} else if (connected_containers.ContainsKey(fluid_voxel.parent_container) || fluid_voxel.connected_containers.ContainsKey(parent_container)) {
											GD.Print("uneven connection exists, check that out!");
											GD.Print(Name);
											GD.Print(connected_containers);
											GD.Print(fluid_voxel.Name);
											GD.Print(fluid_voxel.connected_containers);
											add_connected_container(fluid_voxel.parent_container, SpecialVoxelFlags.FluidOutput);
											fluid_voxel.add_connected_container(parent_container, SpecialVoxelFlags.FluidInput);
										} else {
											GD.Print("adding input from fluidOUTPUT???");

											GD.Print(parent_container.connected_system.connected_outputs);
											GD.Print(fluid_voxel.parent_container.connected_system.connected_inputs);

											parent_container.connected_system.add_output(fluid_voxel.parent_container.connected_system);

											GD.Print(parent_container.connected_system.connected_outputs);
											GD.Print(fluid_voxel.parent_container.connected_system.connected_inputs);

											GD.Print(connected_containers);
											GD.Print(fluid_voxel.connected_containers);

											add_connected_container(fluid_voxel.parent_container, SpecialVoxelFlags.FluidOutput);
											fluid_voxel.add_connected_container(parent_container, SpecialVoxelFlags.FluidInput);

											GD.Print(connected_containers);
											GD.Print(fluid_voxel.connected_containers);
											
										}
									}
								}
							}
						}
						break;
						
				}

			} else {
				//GD.Print("no change??");
			}

			
		} else {
			//GD.Print("we have no parent container???");
		}

		ulong time = Time.GetTicksUsec() - total_start;
		//GD.Print("FLUIDSPECIALVOXEL UPDATE TIME:" + time.ToString());
		
	}

	public void add_connected_container (FluidContainer container, SpecialVoxelFlags flags) {
		if (!connected_containers_cleared_this_frame) {
			connected_containers.Clear();
			connected_containers_cleared_this_frame = true;
		}

		connected_containers[container] = flags;
	}

	public override void on_build()
	{
		base.on_build();

		GD.Print(connected_voxels);

		if (parent_container != null) {
			switch (voxel_flags) {
				case SpecialVoxelFlags.FluidInputOutput: // combine with other systems
					if (parent_container.connected_system == null) {
						parent_container.connected_system = new FluidSystem();
						parent_container.connected_system.add_container(parent_container);
					} 

					previous_system = parent_container.connected_system;
					break;

				case SpecialVoxelFlags.FluidInput:
					if (parent_container.connected_system == null) {
						parent_container.connected_system = new FluidSystem();
						parent_container.connected_system.add_container(parent_container);
					} 

					previous_system = parent_container.connected_system;
					break;

				case SpecialVoxelFlags.FluidOutput:
					if (parent_container.connected_system == null) {
						parent_container.connected_system = new FluidSystem();
						parent_container.connected_system.add_container(parent_container);
					} 

					previous_system = parent_container.connected_system;
					break;
					
			}
		}
	}

	public void set_container (FluidContainer container) {
		parent_container = container;
		if (!parent_container.connection_points.Contains(this)) {
			parent_container.connection_points.Add(this);
		}
	}

	public override void update () {
		base.update();

		connected_containers_cleared_this_frame = false;
	}
}