shader_type canvas_item;
// Texture R8 with byte data
uniform sampler2D my_array;

float get_array_value(ivec2 coord)
{
	// Texture is an R8
	float texture_value = texelFetch(my_array, coord, 0).r;
	texture_value *= 255.0;
	return texture_value;
}

void fragment() {
	// Screen
	float x = 128.0;
	float y = 128.0;
	// Pixel data
	float i = roundEven(get_array_value( ivec2(int(UV.x * x), int(UV.y * y)) ));
	int bit = int(i);
	// Color
	if (bit == 0) COLOR = vec4(0,0,0, 255); // BLACK
	else if (bit == 1) COLOR = vec4(255,255,255, 255); // WHITE
	// Extra
	else if (bit == 2) COLOR = vec4(255,0,0, 255); // RED
	else if (bit == 3) COLOR = vec4(0,255,0, 255); // GREEN
	else if (bit == 4) COLOR = vec4(0,0,255, 255); // BLUE
	else if (bit == 5) COLOR = vec4(255, 159, 0, 255); // ORANGE
	else COLOR = vec4(0,0,0, 255); // BLACK
}
