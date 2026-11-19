// The +9 Forge title: a light band sweeps across the text and a faint rainbow drifts over it. Bright pixels (the fill)
// take the effect, dark ones (the outline) keep their color. `time_override` >= 0 freezes the clock (Workshop GIF frames).
shader_type canvas_item;

uniform float time_override = -1.0;
uniform float period = 520.0;
uniform float band_width = 70.0;
uniform float speed = 260.0;

varying vec2 local_pos;

void vertex() {
    local_pos = VERTEX;
}

vec3 rainbow(float h) {
    return clamp(abs(mod(h * 6.0 + vec3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);
}

void fragment() {
    float t = time_override >= 0.0 ? time_override : TIME;
    float lum = dot(COLOR.rgb, vec3(0.299, 0.587, 0.114));
    float bright = smoothstep(0.35, 0.8, lum);
    float d = local_pos.x + local_pos.y * 0.6 - t * speed;
    float offset = abs(mod(d, period) - period * 0.5);
    float band = 1.0 - smoothstep(0.0, band_width, offset);
    vec3 iris = mix(vec3(1.0), rainbow(fract(local_pos.x * 0.004 - t * 0.25)) * 0.35 + 0.75, 0.6);
    vec3 rgb = COLOR.rgb * mix(vec3(1.0), iris, bright);
    rgb = mix(rgb, vec3(1.0), band * bright * 0.85);
    // The outline lights up a little under the band too, like the cut edge of a gem.
    rgb = mix(rgb, vec3(0.55, 0.95, 1.0), band * (1.0 - bright) * 0.45);
    COLOR.rgb = rgb;
}
