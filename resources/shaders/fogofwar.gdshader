shader_type canvas_item;

uniform sampler2D mask_tex : source_color, repeat_disable; // your gradient
uniform vec2 mask_scale = vec2(1.0, 1.0);  // scale of the mask in UV space
uniform vec2 mask_offset = vec2(0.0, 0.0); // offset of the mask in UV space

void fragment()
{
    // Start with solid black
    vec4 col = vec4(0.0, 0.0, 0.0, 1.0);

    // Sample mask texture
    vec2 uv = UV * mask_scale + mask_offset;
    float mask = texture(mask_tex, uv).r; // white center ≈ 1, edges ≈ 0

    // Invert mask: where mask is white => alpha 0 (transparent hole)
    col.a *= 1.0 - mask;

    COLOR = col;
}
