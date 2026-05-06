Shader "UI/Gradient (Masked)"
{
    Properties
    {
        // Gradient
        _ColorOut ("Out Color", Color) = (1,1,1,1)
        _ColorIn  ("In Color",  Color) = (0,0,0,1)
        _Distance ("Distance", Int)   = 1

        // Pulse
        _PulseEnabled ("Pulse Enabled", Float) = 0
        _PulseColorIn ("Pulse In Color", Color) = (1,0,0,1)
        _PulseColorOut ("Pulse Out Color", Color) = (0,0,1,1)
        _PulseSpeed ("Pulse Speed", Float) = 1

        // --- UI Mask / Stencil plumbing (matches Unity UI/Default) ---
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask",  Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        // For RectMask2D
        _ClipRect ("Clip Rect", Vector) = (0,0,0,0)
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        LOD 100
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv          : TEXCOORD0;
                float4 worldPos    : TEXCOORD1;
                float4 vertex      : SV_POSITION;
            };

            fixed4 _ColorIn;
            fixed4 _ColorOut;
            int    _Distance;

            float  _PulseEnabled;
            fixed4 _PulseColorIn;
            fixed4 _PulseColorOut;
            float  _PulseSpeed;

            float4 _ClipRect;
            float  _UseUIAlphaClip;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex   = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv       = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                #ifdef UNITY_UI_CLIP_RECT
                if (UnityGet2DClipping(i.worldPos.xy, _ClipRect) < 0)
                    discard;
                #endif

                float pulse = (sin(_Time.y * _PulseSpeed) + 1.0) * 0.5;

                fixed4 colorIn = lerp(_ColorIn, _PulseColorIn, pulse * _PulseEnabled);
                fixed4 colorOut = lerp(_ColorOut, _PulseColorOut, pulse * _PulseEnabled);

                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);

                fixed4 col = lerp(colorIn, colorOut, saturate(dist * 2 / max(1, _Distance)));

                #ifdef UNITY_UI_ALPHACLIP
                if (_UseUIAlphaClip > 0.5 && col.a <= 0)
                    clip(-1);
                #endif

                return col;
            }
            ENDCG
        }
    }
}