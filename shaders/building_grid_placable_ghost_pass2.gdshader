shader_type spatial;
render_mode depth_draw_never;

instance uniform float alpha : instance_index(4) = 0.3;
instance uniform vec4 color : instance_index(5) = vec4(0.5, 0.5, 0.5, 1.0);

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	ALBEDO = vec3(0.7, 0.5, 0.5);
	ALPHA = alpha;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
