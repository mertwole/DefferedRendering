#version 330 core

layout(location = 0) in vec3 position; 
layout(location = 1) in vec3 normal;
layout(location = 2) in vec3 tangent;
layout(location = 3) in vec3 bitangent;
layout(location = 4) in vec2 tex_coord;

out vec3 Norm;
flat out vec3 Tang;
flat out vec3 BiTang;
out vec2 Tex;
out vec3 Pos;

uniform mat4 model_mat;
uniform mat4 view_mat;
uniform mat4 projection_mat;

void main()
{
	Norm = normal;
	Tex = tex_coord;
	Pos = position;

	gl_Position = projection_mat * view_mat * model_mat * vec4(position, 1);	
}