#version 330 core

layout(location = 0) out vec3 Position;
layout(location = 1) out vec3 Normal;
layout(location = 2) out vec3 Albedo;
layout(location = 3) out float Metallic;
layout(location = 4) out float Roughness;

in vec3 Norm;
in vec3 Tang;
in vec3 BiTang;
in vec2 Tex;
in vec3 Pos;

uniform sampler2DArray AlbedoNormalsMaps;
uniform sampler2DArray MetallRoughnessMaps;

void main()
{
	mat3 TBN = mat3(
	Tang.x, BiTang.x, Norm.x,
	Tang.y, BiTang.y, Norm.y,
	Tang.z, BiTang.z, Norm.z);

	Position = Pos;
	Albedo = texture(AlbedoNormalsMaps, vec3(Tex, 0)).xyz;
	Normal = (texture(AlbedoNormalsMaps, vec3(Tex, 1)).xyz * 2 - vec3(1)) * TBN;
	Metallic = texture(MetallRoughnessMaps, vec3(Tex, 0)).x;
	Roughness = texture(MetallRoughnessMaps, vec3(Tex, 1)).x;
}