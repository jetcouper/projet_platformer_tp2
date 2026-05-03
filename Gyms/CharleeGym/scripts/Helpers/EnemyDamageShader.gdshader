shader_type canvas_item;

uniform float flash_strength = 0.0;
uniform float death_strength = 0.0;

void fragment()
{
    vec4 tex = texture(TEXTURE, UV);

    vec4 red = vec4(1.0, 0.0, 0.0, tex.a);
    vec4 black = vec4(0.0, 0.0, 0.0, tex.a);

    // normal hit flash
    vec4 hit_flash = mix(tex, red, flash_strength);

    // death pulse (red ↔ black)
    float pulse = abs(sin(TIME * 20.0));
    vec4 death_flash = mix(red, black, pulse);

    // blend states
    vec4 final_color = mix(hit_flash, death_flash, death_strength);

    COLOR = final_color;
}

//IA utilisée pour coder le shader
// ChatGPT
// Prompt : “Can you explain how shaders work in Godot and show a simple example that blends a sprite with red using a uniform?”
//2026-05-03