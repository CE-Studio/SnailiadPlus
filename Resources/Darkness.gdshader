shader_type canvas_item;

uniform vec2 aspect;
uniform int lights = 8;
uniform vec3 light_pos[128];



uniform vec4 main_color: source_color;
uniform vec4 ring_color: source_color;
uniform vec4 bacgroun_color: source_color;
uniform sampler2D dither;
uniform float dither_size;
uniform float halo_thickness = 20.0;

//negative if in circle, postivie if out of croicel
float circle_sdf(vec2 p, vec2 c, float r){
	return length(p-c) - r;
}

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec2 uv = vec2(UV.x * aspect.x, UV.y * aspect.y);
	float d = 1e6;

	for (int i = 0; i < lights; i++){
		vec2 c = light_pos[i].xy;
		float r = light_pos[i].z;
		d = min(d, circle_sdf(uv, c, r));
	}
	//iniside cirlce d < 0

	vec4 color;

	//inside cirlce
	if (d < 0.0 ){
		color = main_color;
	}
	//inside halo
	else if(d < halo_thickness){
		vec2 d_uv = fract(uv/length(aspect) * dither_size);
		float dither_value = texture(dither,d_uv).a;
		dither_value = step(0.5,dither_value);
		color = mix(main_color,ring_color,dither_value);
	}
	//backgroun
	else {
		color = bacgroun_color;
	}

	COLOR = color;
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}