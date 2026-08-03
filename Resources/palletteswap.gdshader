shader_type canvas_item;

uniform sampler2D palettes;
instance uniform int palette_id;

uniform float color_compliance = 0.01;
uniform float alpha_cutoff = 0.1;


void fragment() {
	vec4 current_color = texture(TEXTURE, UV);
	if (current_color.a < alpha_cutoff){
		COLOR = current_color;
	}
	else {
        vec4 final_color = current_color;
        float max_dist_sq = color_compliance * color_compliance;
        
       // Width = color amount
		// Height = npc palette
        ivec2 tex_size = textureSize(palettes, 0);
        int columns = tex_size.x;
        int rows = tex_size.y;
        
        for (int i = 0; i < columns; i++) {
            vec4 base_color = texelFetch(palettes, ivec2(i,0),0);
            vec3 diff = current_color.rgb - base_color.rgb;
            
            if (dot(diff, diff) < max_dist_sq) {
                vec4 new_color = texelFetch(palettes, ivec2(i,palette_id + 1),0);
                final_color = new_color;
                break; 
            }
        }
        COLOR = final_color;
    }
}
