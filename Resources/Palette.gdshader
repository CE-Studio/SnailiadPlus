shader_type canvas_item;
uniform int num_colors: hint_range(2,56) = 56;
uniform sampler2D PALETTE_TEXTURE: hint_default_black;

vec3 palette_limiter (in vec3 albedo){
	float estimation_cutoff = 0.001;
	vec3 closest_color;
	float min_dist = 2.0;
	float n = float(num_colors);
	
	for (int i=0; i<num_colors; i++ ){
		float index = 1.000/(2.000*n)+float(i)/n;
		vec3 index_color = texture(PALETTE_TEXTURE, vec2(index,0.5)).rgb;
		float dist = length(index_color - albedo);
		if (dist < min_dist) {
			min_dist = dist;
			closest_color = index_color;
			if (min_dist < estimation_cutoff){
				return closest_color;
			}
		}
	}
	return closest_color;
}

void fragment() {
	COLOR.rgb = palette_limiter(texture(TEXTURE,UV).rgb);
}