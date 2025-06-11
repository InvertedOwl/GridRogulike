Shader "UI/Gradient"
{
    Properties
    {
        _ColorOut ("Out Color", Color) = (1,1,1,1)
        _ColorIn ("In Color", Color) = (0,0,0,1)
        _Distance ("Distance", Int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _ColorIn;
            fixed4 _ColorOut;
            int _Distance;

            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                return lerp(_ColorIn, _ColorOut, saturate(dist * 2 / _Distance));
            }
            ENDCG
        }
    }
}
