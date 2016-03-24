/////////////////////////////////////////////////////////////////////////////////////////////
//      Cube Shader for Brush Edition
/////////////////////////////////////////////////////////////////////////////////////////////

float4x4 xWorld;
float4x4 xView;
float4x4 xProjection; 

float3 xDirectionalLightDir = float3(0.08f, 0.08f, 0.1f);

float1 xGridPower = 1;

float3 AmbientLightColor = float3(0.08f, 0.08f, 0.1f);

Texture2D  xTexSlot0;
sampler TexSamplerSlot0 = sampler_state
{
	texture = <xTexSlot0>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = Wrap; AddressV = Wrap;
};


struct VS_IN
{
	float4 vertexPosition	: POSITION;
	float3 vertexNormal     : NORMAL0;
	float4 vertexColor		: COLOR0;
};

struct PS_IN
{
	float4 position      : SV_POSITION;
	float4 color         : COLOR;
	float2 uv0			     : TEXCOORD0;
};


float3 gamma(float3 color) {
	return pow(color, float3(0.5, 0.5, 0.5));
}

PS_IN VertexShaderFunction(VS_IN input)
{
	PS_IN output;

	// We need the vertex and normal position in world space
	float4 worldPosition = mul(input.vertexPosition, xWorld);
	float3 normal = normalize(mul(input.vertexNormal, xWorld));

	// Direction to the Light
	float3 lightDirection = normalize(-xDirectionalLightDir);

	// Calculate Light
	float3 diffuse = saturate(abs(dot(lightDirection, normal)));
	float3 color = (input.vertexColor / 2) + input.vertexColor*diffuse;

	//Gamma correction 
	color = gamma(color);

	//Compute UV for grid
	float3 n = abs(input.vertexNormal.xyz);
	float2 tileUV = float2(dot(n, input.vertexPosition.zxx), dot(-n, input.vertexPosition.yzy));

	output.uv0 = tileUV * 0.5;

	// Set output
	output.color = float4(color.xyz, 1.0f);
	output.position = mul(input.vertexPosition, mul(mul(xWorld, xView), xProjection));

	return output;
}

float4 PixelShaderFunction(PS_IN input) : SV_Target
{
	float4 tex0Color = tex2D(TexSamplerSlot0, input.uv0);

	return (tex0Color * xGridPower) + input.color;
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
	}
}

