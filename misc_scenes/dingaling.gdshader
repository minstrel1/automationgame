shader_type spatial;
render_mode cull_disabled, depth_draw_always, unshaded;

varying vec3 world_position;
uniform float range = 10.0;
uniform float falloff_range = 3.0;

void vertex() {
	world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
	vec3 camera_pos = (INV_VIEW_MATRIX * vec4(0.0, 0.0, 0.0, 1.0)).xyz;
	float dist = distance(world_position, camera_pos);
	
	ALBEDO = COLOR.rgb;
	if (dist > range) {
		ALPHA = clamp(1.0 - ((dist - range) / falloff_range), 0.0, 1.0);
	} else {
		ALPHA = 1.0;
	}
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
