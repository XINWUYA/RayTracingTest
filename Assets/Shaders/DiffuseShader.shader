Shader "SphereShader/DiffuseShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float3 normal : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

			fixed4 frag(v2f i) : SV_Target
			{
				float3 Up = float3(0.0f, 1.0f, 0.0f);
				fixed4 col = _Color * fixed4(dot(i.normal, Up).xxx, 1.0f);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }

    SubShader
    {
        Pass
        {
            Name "RayTracing"
            Tags{ "LightMode" = "RayTracing" }

            HLSLPROGRAM

            #pragma raytracing test

            #include "Common/Common.hlsl"
            #include "Common/PRNG.hlsl"

            struct IntersectionVertex
            {
                float3 NormalInModelSpace;
            };

            float4 _Color;

            void FetchIntersectionVertex(uint vVertexIndex, out IntersectionVertex voVertex)
            {
                voVertex.NormalInModelSpace = UnityRayTracingFetchVertexAttribute3(vVertexIndex, kVertexAttributeNormal);
            }

            [shader("closesthit")]
            void ClosestHitShader(inout RayIntersection rayIntersection : SV_RayPayload, AttributeData attributeData : SV_IntersectionAttributes)
            {
                // Fetch the indices of the currentr triangle
                uint3 TriangleIndices = UnityRayTracingFetchTriangleIndices(PrimitiveIndex());
                // Fetch the 3 vertices
                IntersectionVertex v0, v1, v2;
                FetchIntersectionVertex(TriangleIndices.x, v0);
                FetchIntersectionVertex(TriangleIndices.y, v1);
                FetchIntersectionVertex(TriangleIndices.z, v2);

                // Get normal in world space.
                float3 BarycentricCoordinates = float3(1.0f - attributeData.barycentrics.x - attributeData.barycentrics.y, attributeData.barycentrics.x, attributeData.barycentrics.y);
                float3 NormalInModelSpace = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.NormalInModelSpace, v1.NormalInModelSpace, v2.NormalInModelSpace, BarycentricCoordinates);      
                float3x3 ModelMat = (float3x3)ObjectToWorld3x4();
                float3 NormalInWorldSpace = normalize(mul(ModelMat, NormalInModelSpace));

                float4 Color = float4(0.0f, 0.0f, 0.0f, 1.0f);
                if(rayIntersection.remainingDepth > 0)
                {
                    // Get position in world space.
                    float3 Origin = WorldRayOrigin();
                    float3 Direction = WorldRayDirection();
                    float t = RayTCurrent();
					float3 PositionInWorldSpace = Origin + t * Direction;

                    // Make reflection ray.
                    RayDesc RayDescriptor;
                    RayDescriptor.Origin = PositionInWorldSpace + 0.001f * NormalInWorldSpace;
                    RayDescriptor.Direction = normalize(NormalInWorldSpace + GetRandomOnUnitSphere(rayIntersection.PRNGStates));
                    RayDescriptor.TMin = 1e-5f;
                    RayDescriptor.TMax = _CameraFarDistance;

                    // Tracing reflection.
                    RayIntersection ReflectionRayInt;
                    ReflectionRayInt.remainingDepth = rayIntersection.remainingDepth - 1;
                    ReflectionRayInt.PRNGStates = rayIntersection.PRNGStates;
                    ReflectionRayInt.color = float4(0.0f, 0.0f, 0.0f, 0.0f);

                    TraceRay(_AccelerationStructure, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xFF, 0, 1, 0, RayDescriptor, ReflectionRayInt);

                    rayIntersection.PRNGStates = ReflectionRayInt.PRNGStates;
					Color = ReflectionRayInt.color;
                }

				rayIntersection.color = _Color * 0.5f * Color;
            }
            ENDHLSL
        }
    }
}
