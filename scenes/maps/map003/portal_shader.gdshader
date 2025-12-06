shader_type spatial;
render_mode unshaded, cull_disabled, blend_add, depth_prepass_alpha;

uniform sampler2D portal_tex : source_color;
uniform sampler2D noise_tex  : source_color;     // any tiling noise
uniform vec3 portal_color = vec3(0.1, 1.0, 0.7); // green-ish
uniform float glow_strength = 4.0;
uniform float edge_thickness = 0.12;
uniform float noise_amount = 0.15;
uniform float noise_speed = 0.8;

void fragment()
{
    vec2 uv = UV;

    // Use your grayscale texture as alpha mask
    vec4 tex = texture(portal_tex, uv);
    float mask = tex.r;          // or tex.a if you stored it in alpha

    // Radial coordinates
    vec2 c = uv - vec2(0.5);
    float r = length(c);
    float angle = atan(c.y, c.x) / 6.28318 + 0.5; // 0..1

    // Noise along edge, scrolling over time
    vec2 n_uv = vec2(angle * 3.0, r * 3.0 + TIME * noise_speed);
    float n = texture(noise_tex, n_uv).r * 2.0 - 1.0;

    // Distorted edge band
    float edge_pos = 0.5; // “radius” of the ring inside 0..1
    float band = smoothstep(edge_pos - edge_thickness - noise_amount,
                            edge_pos + edge_thickness + noise_amount,
                            r + n * noise_amount);

    float alpha = mask * band;

    vec3 col = portal_color * glow_strength * alpha;

    ALBEDO   = col * 0.1; // faint base color
    EMISSION = col;       // glow
    ALPHA    = alpha;
}
