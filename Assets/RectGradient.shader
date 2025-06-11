Shader "UI/RectGradient"
{
    Properties
    {
        _ColorOut ("Out Color", Color) = (1,1,1,1)
        _ColorIn ("In Color", Color) = (0,0,0,1)
        _Rect ("Rect", Vector) = (0.5, 0.5, 0, 0)
        _MaxDist ("Max Distance", Float) = 0.5
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
            fixed4 _Rect;
            float _MaxDist;

            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            float sdBox(float2 p, float2 b) {
                float2 d = abs(p) - b;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 center = float2(0.5, 0.5);
                float2 localPos = i.uv - center;
                float2 halfSize = _Rect.xy * 0.5;

                float dist = sdBox(localPos, halfSize);
                float t = saturate(dist / _MaxDist);
                
                return lerp(_ColorIn, _ColorOut, t);
            }
            ENDCG
        }
    }
}
