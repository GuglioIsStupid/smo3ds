﻿//This shader uses basic rim (fresnel-style) lighting. Scene/ambient lights are not used. Fully compatible with the 3DS family.

//Shader by Kevin Foley, 2021. Based loosely on the "Gourad Shader" from Bumpy Trail Games 
//(https://developer.nintendo.com/group/development/wtc6ppr2/forums/english/-/gts_message_boards/thread/294653147)

//You are free to use this shader in any game for the Nintendo 3DS/2DS family without restrictions.

Shader "3DS/Simple RimLit"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
		_SpecColor ("FresnelColor", Color) = (1, 1, 1, 1)
		_SpecularIntensityPower ("Fresnel (X: Intensity, Y: Power)", Vector) = (1, 1, 0, 0)
		_FresnelDistance ("Fresnel Distance", Range(0, 1)) = .75
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }
		LOD 100

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "Lighting.cginc"
			#include "UnityCG.cginc"
			
			struct VertexIn
			{
				float4 vertex : POSITION;
				float2 uv_TexA : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct VertexOut
			{
				float4 vertex : POSITION;
				float2 uv_TexA : TEXCOORD0;
				float4 color : COLOR;
			};

			float4 _Color;
			sampler2D _MainTex;
			float2 _SpecularIntensityPower;
			float _FresnelDistance;
			
			VertexOut vert(VertexIn i)
			{
				VertexOut o;

				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv_TexA = i.uv_TexA;

				float3 worldNormal = normalize(mul(unity_ObjectToWorld, i.normal));
				float3 worldVertex = mul(unity_ObjectToWorld, i.vertex);
				float3 viewDirection = normalize(WorldSpaceViewDir(i.vertex));
				
				float3 light1_Direction;
				if (_WorldSpaceLightPos0.a == 0) { //directional light
					light1_Direction = _WorldSpaceLightPos0.xyz;
				} else { //point light
					float3 lightPosition = _WorldSpaceLightPos0.xyz;
					light1_Direction = normalize(worldVertex - lightPosition);
				}
				
				//calculate fresnel intensity
				float3 light1_HalfVector = normalize(-viewDirection);
				float light1_Specular = max(0, dot(light1_HalfVector, worldNormal) + _FresnelDistance);

				//finalize light color
				float fresnelStrength = pow(light1_Specular, _SpecularIntensityPower.y) * _SpecularIntensityPower.x;
				o.color = _Color + _SpecColor * fresnelStrength;

				return o;
			}
			
			fixed4 frag(VertexOut i) : SV_Target
			{
				fixed4 color = tex2D(_MainTex, i.uv_TexA) * i.color;
				return color;
			}

			ENDCG
		}
	}
}
