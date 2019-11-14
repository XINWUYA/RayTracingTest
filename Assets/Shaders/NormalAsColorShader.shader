Shader "SphereShader/NormalAsColorShader"
{
    Properties
    {
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = fixed4(0.5f * (normalize(i.normal) + 1.0f), 1.0f);
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
			Tags { "LightMode" = "RayTracing" }

			HLSLPROGRAM

			#pragma raytracing test

			#include "Common/Common.hlsl"

			struct IntersectionVertex
			{
				float3 NormalInModelSpace;
			};

			void FetchIntersectionVertex(uint vVertexIndex, out IntersectionVertex voVertex)
			{
				voVertex.NormalInModelSpace = UnityRayTracingFetchVertexAttribute3(vVertexIndex, kVertexAttributeNormal);
			}

			[shader("closesthit")]
			void ClosestHitShader(inout RayIntersection rayIntersection : SV_RayPayload, AttributeData attributeData : SV_IntersectionAttributes)
			{
				uint3 triangleIndices = UnityRayTracingFetchTriangleIndices(PrimitiveIndex());

				IntersectionVertex v0, v1, v2;
				FetchIntersectionVertex(triangleIndices.x, v0);
				FetchIntersectionVertex(triangleIndices.y, v1);
				FetchIntersectionVertex(triangleIndices.z, v2);

				float3 BarycentricCoordinates = float3(1.0f - attributeData.barycentrics.x - attributeData.barycentrics.y, attributeData.barycentrics.x, attributeData.barycentrics.y);
				float3 NormalInModelSpace = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.NormalInModelSpace, v1.NormalInModelSpace, v2.NormalInModelSpace, BarycentricCoordinates);
				float3x3 ModelMat = (float3x3)ObjectToWorld3x4();
				float3 NormalInWorldSpace = normalize(mul(ModelMat, NormalInModelSpace));
				rayIntersection.color = float4(0.5f * (NormalInWorldSpace + 1.0f), 0.0f);
			}

			ENDHLSL
		}
	}
}
