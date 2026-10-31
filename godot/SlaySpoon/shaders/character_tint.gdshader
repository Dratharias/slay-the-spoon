// A player's character color, after the game's own hsv.gdshader: put on the creature's Spine normal material
// (or the static body's material). mode 1 lays a flat color over the body at `strength` opacity; mode 2 turns only
// the pixels close to the character's dominant hue (`source_hue`, within `threshold`) to `target_hue`.
shader_type canvas_item;

uniform int mode = 0;
uniform vec3 target_color = vec3(0.0, 1.0, 1.0);
uniform float target_hue = 0.5;
uniform float source_hue = 0.33;
uniform float threshold = 0.11;
uniform float strength = 0.2;

varying vec4 modulate_color;

void vertex() {
    modulate_color = COLOR;
}

vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

void fragment() {
    vec4 col = texture(TEXTURE, UV);
    if (mode == 1) {
        col.rgb = mix(col.rgb, target_color, strength);
    } else if (mode == 2) {
        vec3 hsv = rgb2hsv(col.rgb);
        float d = abs(hsv.x - source_hue);
        d = min(d, 1.0 - d);
        // Near the dominant hue and not grey: shift by the same angle, so shading inside the color is kept.
        float w = (1.0 - smoothstep(threshold * 0.6, threshold, d)) * smoothstep(0.08, 0.2, hsv.y);
        vec3 shifted = hsv2rgb(vec3(fract(hsv.x + target_hue - source_hue), hsv.y, hsv.z));
        col.rgb = mix(col.rgb, shifted, w * strength);
    }
    COLOR = col * modulate_color;
}
