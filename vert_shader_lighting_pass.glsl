#version 330 core

layout(location = 0) in vec2 position;
layout(location = 1) in vec2 tex_coord;

out vec2 tex;

void main()
{
	gl_Position = vec4(position, 0, 1);

	tex = tex_coord;
}