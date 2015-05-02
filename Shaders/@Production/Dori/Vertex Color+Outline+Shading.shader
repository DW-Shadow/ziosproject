﻿Shader "Zios/Dori/Vertex Color + Outline + Shading"{
	Properties{
		cullDistance("Distance",float) = 20
		outlineSize("Outline Size",Range(0.002,0.01)) = 0.005
		outlineIntensity("Outline Intensity",Range(0,1)) = 0.8
		contrast("Contrast",Range(-0.5,0.5)) = 0.1107143
		shadingRamp("Shading Ramp",2D) = "white"{}
		shadeIntensity("Shade Intensity",Range(0,1)) = 0.8571429
	}
	SubShader{
		Tags{"LightMode"="ForwardBase"}
		Pass{
			Cull Front
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			float cullDistance;
			fixed outlineIntensity;
			fixed outlineSize;
			struct vertexInput{
				float4 vertex        : POSITION;
				float3 normal        : NORMAL;
				float4 color         : COLOR;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 color    	 : COLOR;
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				if(distance(ObjSpaceViewDir(input.vertex),output.pos) > cullDistance){
					output.pos.w = -1;
					return output;
				}
				float3 normal = mul((float3x3)UNITY_MATRIX_IT_MV,input.normal);
				float2 offset = TransformViewToProjection(normal.xy);
				output.pos.xy += offset * output.pos.z * outlineSize;
				output.color = input.color * outlineIntensity;
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output;
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output.color = input.color;
				return output;
			}
			ENDCG
		}
		Pass{
			Cull Off
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			float cullDistance;
			fixed shadeIntensity;
			fixed contrast;
			fixed4 _LightColor0;
			sampler2D shadingRamp;
			struct vertexInput{
				float4 vertex          : POSITION;
				float4 texcoord        : TEXCOORD0;
				float3 normal          : NORMAL;
				float4 color           : COLOR;
			};
			struct vertexOutput{
				float4 pos             : POSITION;
				float3 color    	   : COLOR;
				float4 normal          : NORMAL;
				float3 lightNormal	   : TEXCOORD0;
				float4 UV              : TEXCOORD2;
				float  lighting        : TEXCOORD3;
			};
			struct pixelOutput{
				float4 color           : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output.color = float4(0,0,0,0);
				return output;
			}
			vertexOutput setupHalfLighting(vertexOutput input){
				input.lighting = dot(input.normal,input.lightNormal) * 0.5f + 0.5f;
				return input;
			}
			pixelOutput applyVertexColor(vertexOutput input,pixelOutput output){
				output.color.rgb += input.color.rgb;
				return output;
			}
			pixelOutput applySceneAmbient(vertexOutput input,pixelOutput output){
				output.color.rgb += UNITY_LIGHTMODEL_AMBIENT.rgb;
				output.color.rgb *= _LightColor0.rgb;
				return output;
			}
			pixelOutput applyDiffuseLerpShading(vertexOutput input,pixelOutput output){
				output.color.rgb *= tex2D(shadingRamp,input.lighting) + (1-shadeIntensity);
				//output.color.rgb *= tex2D(shadingRamp,input.lighting);
				return output;
			}
			pixelOutput applyContrast(vertexOutput input,pixelOutput output){
				output.color.rgb = (output.color.rgb - 0.5f) * max(contrast + 1.0,0) + 0.5f;
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				if(distance(ObjSpaceViewDir(input.vertex),output.pos) > cullDistance){
					output.pos.w = -1;
					return output;
				}
				output.UV = float4(input.texcoord.xy,0,0);
				output.color = input.color;
				output.normal = float4(input.normal,0);
				output.lightNormal = ObjSpaceLightDir(input.vertex);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				input.normal = normalize(input.normal);
				input.lightNormal = normalize(input.lightNormal);
				input = setupHalfLighting(input);
				output = applyVertexColor(input,output);
				output = applyDiffuseLerpShading(input,output);
				output = applySceneAmbient(input,output);
				output = applyContrast(input,output);
				output.color.rgb *= 2;
				return output;
			}
			ENDCG
		}
	}
	//CustomEditor "Zios.ExtendedMaterialEditor"
}