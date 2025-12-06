shader_type canvas_item;
render_mode unshaded, blend_mix;

// Inner and outer radii of the shadow (in pixels, in THIS control's space)
uniform float visible_radius = 300.0;
uniform float shadow_radius  = 800.0;

// Size of the control in pixels (set from C#)
uniform vec2 control_size = vec2(1920.0, 1080.0);

// Color of the shadow
uniform vec4 tint_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);

float get_alpha(float dist)
{
    // BLACK at center, TRANSPARENT at edges

    if (dist <= visible_radius)
        return 1.0;   // fully dark

    if (dist >= shadow_radius)
        return 0.0;   // fully transparent

    float t = (dist - visible_radius) / (shadow_radius - visible_radius);
    t = smoothstep(0.0, 1.0, t);

    return 1.0 - t;   // reverse fade: 1 at center, 0 at edge
}

void fragment()
{
    // UV is 0..1 inside THIS control's rectangle
    vec2 pixel = UV * control_size;
    vec2 center = control_size * 0.5;

    float dist = length(pixel - center);
    float alpha = get_alpha(dist);

    COLOR = vec4(tint_color.rgb, alpha);
}
