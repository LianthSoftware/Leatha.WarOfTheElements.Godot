shader_type spatial;
render_mode unshaded, cull_disabled, blend_premul_alpha;

// Top / bottom border
uniform vec4 border_color : source_color = vec4(0.122, 0.102, 0.059, 1.0); // #1f1a0f
uniform float border_thickness = 0.02; // UV space, tweak for ~3px look

// Gradient texture (set a GradientTexture2D here)
uniform sampler2D gradient_tex;
uniform float gradient_v = 0.5; // which horizontal "row" of the texture to sample

// Optional fallback if no texture is set
uniform vec4 fallback_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);

void fragment()
{
    float y = UV.y;

    bool is_border =
        (y <= border_thickness) ||
        (y >= 1.0 - border_thickness);

    vec4 col;

    if (is_border)
    {
        // solid top/bottom border
        col = border_color;
    }
    else
    {
        // left→right gradient from texture
        vec4 tex_col = texture(gradient_tex, vec2(UV.x, gradient_v));

        // If gradient_tex is not assigned, tex_col may be (0,0,0,0), so
        // blend with fallback_color as a safety.
        col = mix(fallback_color, tex_col, tex_col.a);
    }

    ALBEDO = col.rgb;
    ALPHA  = col.a;
}
