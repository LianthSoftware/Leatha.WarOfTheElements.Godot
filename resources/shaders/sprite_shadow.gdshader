shader_type canvas_item;

uniform vec4 glow_color : source_color = vec4(1.0, 1.0, 1.0, 0.4); // Glow color
uniform float glow_size = 8.0; // In pixels (controls spread)
uniform float softness = 4.0; // How soft the glow fades out
uniform float alpha_threshold = 0.1;

void fragment() {
    vec4 tex = texture(TEXTURE, UV);
    float base_alpha = tex.a;

    // If this is the sprite pixel, draw it normally
    if (base_alpha > alpha_threshold) {
        COLOR = tex;
    }
	else
	{
	// Otherwise, calculate soft glow around it
    float glow = 0.0;

    // Sample in a radial pattern
    for (float angle = 0.0; angle < 6.28318; angle += 0.3927) { // step ~22.5 degrees
        for (float r = 1.0; r <= glow_size; r++) {
            vec2 offset = vec2(cos(angle), sin(angle)) * r * TEXTURE_PIXEL_SIZE;
            float a = texture(TEXTURE, UV + offset).a;
            glow += a * (1.0 - r / glow_size); // fade with distance
        }
    }

    glow = clamp(glow / glow_size, 0.0, 1.0); // Normalize

    if (glow > 0.0) {
        COLOR = vec4(glow_color.rgb, glow_color.a * glow);
    } else {
        discard; // Fully transparent if no glow detected
    }
	}
}
