shader_type canvas_item;
render_mode unshaded, blend_mix;

uniform vec2 player_position = vec2(0.0, 0.0);
uniform float visible_radius = 1000.0;
uniform float shadow_radius  = 2000.0;
uniform vec4 tint_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);

float getFragmentAlpha(vec2 fragCoordPosition, vec2 position)
{
    vec2 pos = fragCoordPosition - position;
    float distance = length(pos);

    if (distance <= visible_radius) {
        // fully visible hole
        return 0.0;
    }

    if (distance >= shadow_radius) {
        // fully dark
        return 1.0;
    }

    // Smooth fade between visible_radius and shadow_radius
    float t = (distance - visible_radius) / (shadow_radius - visible_radius);
    // optional: use smoothstep for softer curve
    t = smoothstep(0.0, 1.0, t);

    return t;
}

void fragment()
{
    float alpha = getFragmentAlpha(FRAGCOORD.xy, player_position);
    COLOR = vec4(tint_color.rgb, alpha);
}
