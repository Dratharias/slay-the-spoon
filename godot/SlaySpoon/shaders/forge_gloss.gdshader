// A forged card's frame: a light band sweeps across it, stronger with the level (`strength`). `time_override` >= 0
// freezes the clock (Workshop GIF frames). Alpha is kept, so only drawn pixels shine.
shader_type canvas_item;

uniform float strength = 0.3;
uniform float time_override = -1.0;
uniform float period = 900.0;
uniform float band_width = 90.0;
uniform float speed = 300.0;

varying vec2 local_pos;

void vertex() {
    local_pos = VERTEX;
}

void fragment() {
    float t = time_override >= 0.0 ? time_override : TIME;
    float d = local_pos.x + local_pos.y * 0.6 - t * speed;
    float band = 1.0 - smoothstep(0.0, band_width, abs(mod(d, period) - period * 0.5));
    COLOR.rgb = mix(COLOR.rgb, vec3(0.92, 0.98, 1.0), band * strength);
}
