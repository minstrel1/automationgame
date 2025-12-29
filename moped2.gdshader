shader_type spatial;
render_mode cull_disabled, depth_draw_never;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	ALBEDO = vec3(0.7, 0.5, 0.5);
	ALPHA = 0.3;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
