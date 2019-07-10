#version 330 core

layout(location = 0) out vec3 Position;
layout(location = 1) out vec4 AlbedoSpecular;
layout(location = 2) out vec3 Normal;

in vec3 Norm;
in vec2 Tex;
in vec3 Pos;

void main()
{
	Position = Pos;
	AlbedoSpecular = vec4(1, 0.5, 0, 1);
	Normal = Norm;
}