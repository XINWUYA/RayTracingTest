#include "UnityRaytracingMeshUtils.cginc"

// Macro that interpolate any attribute using barycentric coordinates
#define INTERPOLATE_RAYTRACING_ATTRIBUTE(A0, A1, A2, BARYCENTRIC_COORDINATES) (A0 * BARYCENTRIC_COORDINATES.x + A1 * BARYCENTRIC_COORDINATES.y + A2 * BARYCENTRIC_COORDINATES.z)

float4x4 _InvCameraViewProj;
float3 _WorldSpaceCameraPos;
float _CameraFarDistance;
RaytracingAccelerationStructure _AccelerationStructure;

struct RayIntersection
{
    int remainingDepth;
	uint4 PRNGStates;
	float4 color;
};

struct AttributeData
{
	float2 barycentrics;
};

inline void GenCameraRay(out float3 voOrigin, out float3 voDirection)
{
	float2 DispatchIdx = DispatchRaysIndex().xy + 0.5f;//center in the middle of the pixel.
	float2 ScreenPos = (DispatchIdx / DispatchRaysDimensions().xy) * 2.0f - 1.0f;
	float4 WorldPos = mul(_InvCameraViewProj, float4(ScreenPos, 0.0f, 1.0f));

	WorldPos.xyz /= WorldPos.w;

	voOrigin = _WorldSpaceCameraPos.xyz;
	voDirection = normalize(WorldPos.xyz - voOrigin);
}

inline void GenCameraRayWithOffset(out float3 voOrigin, out float3 voDirection, float2 vOffset)
{
    float2 DispatchIdx = DispatchRaysIndex().xy + vOffset;
    float2 ScreenPos = (DispatchIdx / DispatchRaysDimensions().xy) * 2.0f - 1.0f;
    float4 WorldPos = mul(_InvCameraViewProj, float4(ScreenPos, 0.0f, 1.0f));

    WorldPos.xyz /= WorldPos.w;

    voOrigin = _WorldSpaceCameraPos.xyz;
    voDirection = normalize(WorldPos.xyz - voOrigin);
}