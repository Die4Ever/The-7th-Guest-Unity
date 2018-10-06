Shader "Unlit/renderTextureShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_thresh("Threshold", Range(0, 5)) = 0.65
		_slope("Slope", Range(0, 1)) = 0.63
		_keyingColor("KeyColor", Color) = (0,1,0,1)
		_multColor("MultColor", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100
		Lighting Off
		//ZWrite Off
		//AlphaTest Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float3 _keyingColor;
			float4 _multColor;
			float _thresh;
			float _slope;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float3 col = tex2D(_MainTex, i.uv).rgb;
				float d = abs(length(abs(_keyingColor.rgb - col.rgb)));
				float edge0 = _thresh * (1 - _slope);
				float alpha = smoothstep(edge0, _thresh, d);
				//return float4(0, 1, 0, 0.5);
				return float4(col, alpha) * _multColor;
				//return col;
			}
			ENDCG
		}
	}
}
