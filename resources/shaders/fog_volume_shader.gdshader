shader_type spatial;
render_mode unshaded, blend_mix, cull_disabled, depth_draw_always;

uniform vec4 fog_color : source_color = vec4(0.6, 0.7, 0.9, 1.0);
uniform float density = 0.3; // 0 = invisible, 1 = opaque

void fragment() {
    ALBEDO = fog_color.rgb;
    ALPHA = clamp(density, 0.0, 1.0);
}
