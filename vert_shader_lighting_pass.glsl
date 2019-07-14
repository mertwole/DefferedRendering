#version 330 core

#define SSAO

layout(location = 0) in vec2 position;
layout(location = 1) in vec2 tex_coord;

out vec2 tex;

#ifdef SSAO
	uniform mat4 view_mat;
	uniform mat4 projection_mat;

	out mat4 transform_mat;
#endif

void main()
{
	gl_Position = vec4(position, 0, 1);

	tex = tex_coord;
	transform_mat = projection_mat * view_mat;
}