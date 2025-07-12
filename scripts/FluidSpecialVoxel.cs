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

	public override void update_voxel_connections()
	{
		base.update_voxel_connections();

		ulong total_start = Time.GetTicksUsec();

		if (parent_container != null) {

			if (connections_changed_this_frame) {

				// if (!parent_container.new_system_this_frame) {
				// 	if (parent_container.connected_system != null) {
				// 		parent_container.connected_system.remove_container(parent_container);
				// 	}

				// 	parent_container.connected_system = new FluidSystem();
				// 	parent_container.connected_system.add_container(parent_container);

				// 	parent_container.new_system_this_frame = true;
				// }
				
				connected_containers.Clear();

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
													parent_container.connected_outputs[fluid_voxel.parent_container] = SpecialVoxelFlags.FluidInputOutput;
													fluid_voxel.parent_container.connected_outputs[parent_container] = SpecialVoxelFlags.FluidInputOutput;

													if (fluid_voxel.parent_container.connected_system.connected_containers.Count >= parent_container.connected_system.connected_containers.Count) {
														fluid_voxel.parent_container.connected_system.merge_system(parent_container.connected_system);
													} else {
														parent_container.connected_system.merge_system(fluid_voxel.parent_container.connected_system);
													}
													
													connected_containers[fluid_voxel.parent_container] = SpecialVoxelFlags.FluidInputOutput;
													fluid_voxel.connected_containers[parent_container] = SpecialVoxelFlags.FluidInputOutput;

												} else {
													GD.Print("incompatible systems");
												}
												
											}											
										}

										break;

									case SpecialVoxelFlags.FluidInput:
										if (fluid_voxel.parent_container != null && fluid_voxel.parent_container.connected_system != null) {
											if (parent_container.connected_system != null) {
												if (parent_container.connected_system.is_system_compatible(fluid_voxel.parent_container.connected_system)) {
													parent_container.connected_system.add_output(fluid_voxel.parent_container.connected_system);

													connected_containers[fluid_voxel.parent_container] = SpecialVoxelFlags.FluidOutput;
													fluid_voxel.connected_containers[parent_container] = SpecialVoxelFlags.FluidInput;
												}
											}
										}
										
										break;

									case SpecialVoxelFlags.FluidOutput:
										// if (fluid_voxel.parent_container != null && fluid_voxel.parent_container.connected_system != null) {
										// 	if (parent_container.connected_system != null) {
										// 		if (fluid_voxel.parent_container.connected_system.is_system_compatible(parent_container.connected_system)) {
										// 			fluid_voxel.parent_container.connected_system.add_output(parent_container.connected_system);
										// 		}
										// 	}
										// }
										
										break;
								}
							}
						}

						break;

					case SpecialVoxelFlags.FluidInput:
						foreach (SpecialVoxel voxel in connected_voxels) {
							if (voxel is FluidSpecialVoxel) {
								FluidSpecialVoxel fluid_voxel = (FluidSpecialVoxel) voxel;

								// if (fluid_voxel.parent_container != null && fluid_voxel.parent_container.connected_system != null) {
								// 	fluid_voxel.parent_container.connected_system.add_output(parent_container.connected_system);
								// }
							}
						}

						break;

					case SpecialVoxelFlags.FluidOutput:
						foreach (SpecialVoxel voxel in connected_voxels) {
							if (voxel is FluidSpecialVoxel) {
								FluidSpecialVoxel fluid_voxel = (FluidSpecialVoxel) voxel;

								if (fluid_voxel.voxel_flags == SpecialVoxelFlags.FluidInput || fluid_voxel.voxel_flags == SpecialVoxelFlags.FluidInputOutput) {
									if (fluid_voxel.parent_container != null && fluid_voxel.parent_container.connected_system != null) {
										parent_container.connected_system.add_output(fluid_voxel.parent_container.connected_system);

										connected_containers[fluid_voxel.parent_container] = SpecialVoxelFlags.FluidOutput;
										fluid_voxel.connected_containers[parent_container] = SpecialVoxelFlags.FluidInput;
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

					break;

				case SpecialVoxelFlags.FluidInput:
					if (parent_container.connected_system == null) {
						parent_container.connected_system = new FluidSystem();
						parent_container.connected_system.add_container(parent_container);
					} 

					break;

				case SpecialVoxelFlags.FluidOutput:
					if (parent_container.connected_system == null) {
						parent_container.connected_system = new FluidSystem();
						parent_container.connected_system.add_container(parent_container);
					} 

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
}