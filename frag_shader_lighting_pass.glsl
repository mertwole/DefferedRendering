#version 330 core

out vec4 color;

in vec2 tex;

uniform sampler2D g_position;
uniform sampler2D g_normal;
uniform sampler2D g_albedo;
uniform sampler2D g_metallic;
uniform sampler2D g_roughness;

void main()
{
	color = vec4(texture2D(g_normal, tex).xyz, 1);
}