Shader "Hidden/Zios/Shadow Pass/Normal Index Map"{
	Properties{
	}
	SubShader{
		LOD 200
		Pass{
			Name "ShadowCaster"
			Tags{"LightMode"="ShadowCaster"}
			Offset 3, 3
			Fog{Mode Off}
			ZWrite On ZTest Less Cull Off
			CGPROGRAM
			#pragma vertex vertexPassShadow
			#pragma fragment pixelPassShadow
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			sampler2D normalIndexMap;
			float4 normalIndexMap_ST;
			struct vertexOutput{ 
				V2F_SHADOW_CASTER;
				float2 UV  : TEXCOORD1;
			};
			vertexOutput vertexPassShadow(appdata_base v){
				vertexOutput o;
				TRANSFER_SHADOW_CASTER(o);
				o.UV = TRANSFORM_TEX(v.texcoord,normalIndexMap);
				return o;
			}
			float4 pixelPassShadow(vertexOutput i) : COLOR{
				clip(tex2D(normalIndexMap,i.UV).a-1);
				SHADOW_CASTER_FRAGMENT(i);
			}
			ENDCG
		}
		Pass{
			Name "ShadowCollector"
			Tags{"LightMode" = "ShadowCollector"}
			Fog{Mode Off}
			ZWrite On ZTest Less
			CGPROGRAM
			#pragma vertex vertexPassShadow
			#pragma fragment pixelPassShadow
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_shadowcollector
			#define SHADOW_COLLECTOR_PASS
			#include "UnityCG.cginc"
			sampler2D normalIndexMap;
			float4 normalIndexMap_ST;
			struct vertexOutput{
				V2F_SHADOW_COLLECTOR;
				float2 UV : TEXCOORD5;
			};
			vertexOutput vertexPassShadow(appdata_base v){
				vertexOutput o;
				TRANSFER_SHADOW_COLLECTOR(o);
				o.UV = TRANSFORM_TEX(v.texcoord,normalIndexMap);
				return o;
			}
			fixed4 pixelPassShadow(vertexOutput i) : COLOR{
				clip(tex2D(normalIndexMap,i.UV).a-1);
				SHADOW_COLLECTOR_FRAGMENT(i);
			}
			ENDCG
		}
	}
}
