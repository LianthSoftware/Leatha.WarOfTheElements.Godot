shader_type canvas_item;

uniform float radius = 0.5;
uniform vec2 center = vec2(0.5, 0.5);

// When true, the visible circular area becomes grayscale
uniform bool grayscale = false;

void fragment() {
    vec2 uv = UV;
    float dist = distance(uv, center);

    // Circular clipping
    if (dist > radius) {
        discard;
    }

    vec4 col = texture(TEXTURE, uv);

    if (grayscale) {
        // Standard luminance conversion
        float gray = dot(col.rgb, vec3(0.299, 0.587, 0.114));
        col.rgb = vec3(gray);
    }

    COLOR = col;
}
