Shader "Custom/AnimatedSpriteSubsection"
{
	Properties
	{
		_MainTex ("Sprite Sheet", 2D) = "white" {}
		_Dimensions ("Sheet Dimensions (Columns, Rows)", Vector) = (4, 4, 0, 0)
		_Index ("Start Frame Index (Column, Row)", Vector) = (0, 0, 0, 0)
		_FrameCount ("Frame Count", Float) = 4
		_Speed ("Frames per Second", Float) = 6
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
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float2 _Dimensions;
			float2 _Index;
			float _FrameCount;
			float _Speed;

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

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				// Get time in seconds (mod large float to avoid precision issues)
				float time = fmod(_Time.y, 10000.0);

				// Calculate current frame
				float frame = floor(time * _Speed);
				float frameIndex = fmod(frame, _FrameCount);

				// Calculate tile size (1 / Columns, 1 / Rows)
				float2 tileSize = 1.0 / _Dimensions;

				// Absolute column index = startX + frameIndex
				float col = _Index.x + frameIndex;
				float row = _Index.y;

				// UV offset
				float2 offset;
				offset.x = col * tileSize.x;
				offset.y = 1.0 - ((row + 1.0) * tileSize.y); // Flip Y

				// Adjust original UV to frame
				float2 uv = i.uv * tileSize + offset;

				// Sample the frame
				return tex2D(_MainTex, uv);
			}
			ENDCG
		}
	}
}
