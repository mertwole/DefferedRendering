#version 330 core

out vec4 color;

in vec2 tex;

uniform vec3 view_point;

uniform sampler2D g_position;
uniform sampler2D g_normal;
uniform sampler2D g_albedo;
uniform sampler2D g_metallic;
uniform sampler2D g_roughness;

struct PointLight
{
	vec3 position;
	vec3 color;
};

#define INV_PI 0.3183098861

PointLight[] PointLights = 
{
	{vec3(0, 0, 3), vec3(1)}
};

float GGX_selfshadowing(float sqr_roughness, float angle_cos)
{
	float angle_cos_sqr = angle_cos * angle_cos;
	return 2 / (1 + sqrt(1 + sqr_roughness * ((1 - angle_cos_sqr) / angle_cos_sqr)));
}

float GGX_microedgesdistribution(float sqr_roughness, float NHangle_cos)
{
	float NHangle_cos_sqr = NHangle_cos * NHangle_cos; 
	float denominator_sqrt = NHangle_cos_sqr * (sqr_roughness + (1 - NHangle_cos_sqr) / NHangle_cos_sqr);

	return sqr_roughness * INV_PI / (denominator_sqrt * denominator_sqrt);
}

vec3 Frenel(vec3 reflected, float NLangle_cos)
{
	return reflected + (vec3(1) - reflected) * pow(1 - NLangle_cos, 5);
}

void main()
{
	vec3 position = texture(g_position, tex).xyz;
	vec3 normal = texture(g_normal, tex).xyz;
	vec3 albedo = texture(g_albedo, tex).xyz;
	float metallic = texture(g_metallic, tex).x;
	float sqr_roughness = texture(g_roughness, tex).x;
	sqr_roughness *= sqr_roughness;

	vec3 res_color;

	vec3 V = normalize(view_point - position);//view
	float NV = dot(normal, V);

	vec3 frenel_k = albedo * metallic + vec3(0.04) * (1 - metallic);

	for(int i = 0; i < PointLights.length(); i++)
	{		
		vec3 L = PointLights[i].position - position;//light	
		float L_len = length(L);
		L /= L_len;
		
		vec3 H = normalize(L + V);//half vector
	
		float NL = dot(normal, L);

		vec3 specular = Frenel(frenel_k, NL);
	
		if(NL >= 0 && NV >= 0)
		{
			vec3 color;

			// specular
			color += specular *
			GGX_microedgesdistribution(sqr_roughness, dot(normal, H)) *
			GGX_selfshadowing(sqr_roughness, NL) *
			GGX_selfshadowing(sqr_roughness, NV) / (4 * NV);
			// diffuse
			color += (1 - metallic) * (1 - specular) * albedo * NL * INV_PI;

			color *= PointLights[i].color;

			res_color += color;
		}
	}

	res_color = res_color / (res_color + vec3(1));
	res_color = pow(res_color, vec3(1 / 2.2));

	color = vec4(res_color, 1);
}