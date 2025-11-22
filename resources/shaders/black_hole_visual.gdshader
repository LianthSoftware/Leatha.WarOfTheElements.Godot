shader_type canvas_item;

uniform vec2 player_position = vec2(0.0, 0.0);
uniform float visible_radius = 1000.0;
uniform float shadow_radius = 2000.0;

float getFragmentColorAlpha(vec2 fragCoordPosition, vec2 position)
{
	vec2 pos = fragCoordPosition - position;
    float distance = length(pos);

    if (distance < visible_radius) {
        return 0.0;//vec4(0.0, 1.0, 0.0, 0.0); // Green fully visible area
    } else if (distance < shadow_radius) {
        float alpha = (distance - visible_radius) / (shadow_radius - visible_radius);
        return alpha;//vec4(0.0, 0.0, 0.0, alpha); // Blue shadow area
	} else if (distance < shadow_radius * 1.5) {
        return 0.97; // vec4(0.0, 0.0, 0.0, 0.97); // Almost full (97%) shadow area
	} else if (distance < shadow_radius * 2.0) {
        return 0.99; //vec4(0.0, 0.0, 0.0, 0.99); // Almost full (99%) shadow area
    } else {
        return 1.0; //vec4(0.0, 0.0, 0.0, 1.0); // Fully (100%) black area
    }
}

void fragment() {
    /*vec2 pos = FRAGCOORD.xy - player_position;
    float distance = length(pos);

    if (distance < visible_radius) {
        COLOR = vec4(0.0, 1.0, 0.0, 0.0); // Green fully visible area
    } else if (distance < shadow_radius) {
        float alpha = (distance - visible_radius) / (shadow_radius - visible_radius);
        COLOR = vec4(0.0, 0.0, 0.0, alpha); // Blue shadow area
	} else if (distance < shadow_radius * 1.5) {
        COLOR = vec4(0.0, 0.0, 0.0, 0.97); // Almost full (97%) shadow area
	} else if (distance < shadow_radius * 2.0) {
        COLOR = vec4(0.0, 0.0, 0.0, 0.99); // Almost full (99%) shadow area
    } else {
        COLOR = vec4(0.0, 0.0, 0.0, 1.0); // Fully (100%) black area
    }*/

	float alpha = getFragmentColorAlpha(FRAGCOORD.xy, player_position);
	COLOR = vec4(0.0, 0.0, 0.0, alpha);
}


/*shader_type canvas_item;

uniform vec2 player_position = vec2(0.0, 0.0);
uniform float visible_radius = 1000.0;
uniform float shadow_radius = 2000.0;
uniform int num_light_points = 1;
uniform vec2 light_positions[10]; // Maximum of 10 light points

void fragment() {
    vec2 pos = FRAGCOORD.xy;

    for (int i = 0; i < num_light_points; i++) {
        vec2 light_pos = light_positions[i];

        vec2 to_light = pos - light_pos;
        float distance = length(to_light);

        if (distance < visible_radius) {
            COLOR = vec4(0.0, 1.0, 0.0, 0.0); // Green fully visible area
            break;
        } else if (distance < shadow_radius) {
        	float alpha = (distance - visible_radius) / (shadow_radius - visible_radius);
        	COLOR = vec4(0.0, 0.0, 0.0, alpha); // Blue shadow area
	    } else {
	        COLOR = vec4(0.0, 0.0, 0.0, 1.0); // Fully black area
	    }
    }

	vec2 playerPos = FRAGCOORD.xy - player_position;
    float playerDistance = length(playerPos);

    if (playerDistance < visible_radius) {
        COLOR = vec4(0.0, 1.0, 0.0, 0.0); // Green fully visible area
    } else if (playerDistance < shadow_radius) {
        float alpha = (playerDistance - visible_radius) / (shadow_radius - visible_radius);
        COLOR = vec4(0.0, 0.0, 0.0, alpha); // Blue shadow area
    } else {
        COLOR = vec4(0.0, 0.0, 0.0, 1.0); // Fully black area
    }
}*/