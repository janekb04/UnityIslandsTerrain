void NormalFromHeightLOD_float(Texture2D<float4> height, SamplerState Sampler, float2 UV, float LOD, float Offset, float Strength, out float3 Out, out float Height)
{
	Offset = pow(Offset, 3) * 0.1;
	float2 offsetU = float2(UV.x + Offset, UV.y);
	float2 offsetV = float2(UV.x, UV.y + Offset);
	float normalSample = SAMPLE_TEXTURE2D_LOD(height, Sampler, UV, LOD).r;
	float uSample = SAMPLE_TEXTURE2D_LOD(height, Sampler, offsetU, LOD).r;
	float vSample = SAMPLE_TEXTURE2D_LOD(height, Sampler, offsetV, LOD).r;
	float3 va = float3(1, 0, (uSample - normalSample) * Strength);
	float3 vb = float3(0, 1, (vSample - normalSample) * Strength);
	Out = normalize(cross(va, vb));
	Out.y = -Out.y;
	Height = normalSample;
}
