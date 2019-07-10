#version 330 core

out vec4 color;

in vec2 tex;

uniform sampler2D g_position;
uniform sampler2D g_albedospecular;
uniform sampler2D g_normal;

void main()
{
	color = vec4(texture2D(g_albedospecular, tex).xyz, 1);
}