Shader "UI/RectGridWorldAligned"
{
    Properties
    {
        _ColorOut ("Out Color", Color) = (1,1,1,1)
        _ColorIn  ("In Color",  Color) = (0,0,0,1)

        // Grid cell size in WORLD UNITS (X,Y)
        _CellSize ("Cell Size (World Units X,Y)", Vector) = (0.5, 0.5, 0, 0)

        // 0 = use object X/Y axes; 1 = use object X/Z axes
        _GridPlane ("Grid Plane (0=XY, 1=XZ)", Float) = 0
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

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0; // unused; kept for compatibility
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            fixed4 _ColorIn;
            fixed4 _ColorOut;
            float4 _CellSize;  // xy used
            float  _GridPlane; // 0=XY, 1=XZ

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex   = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // --- Build an object-aligned, scale-free basis in world space ---
                // Columns of unity_ObjectToWorld are the object’s axes in world space.
                // Normalizing them removes scaling so cell size stays in world units.
                float3 axisX = normalize(float3(unity_ObjectToWorld._m00,
                                                unity_ObjectToWorld._m10,
                                                unity_ObjectToWorld._m20));
                float3 axisY = normalize(float3(unity_ObjectToWorld._m01,
                                                unity_ObjectToWorld._m11,
                                                unity_ObjectToWorld._m21));
                float3 axisZ = normalize(float3(unity_ObjectToWorld._m02,
                                                unity_ObjectToWorld._m12,
                                                unity_ObjectToWorld._m22));

                // Object origin in world space
                float3 objWorldOrigin = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;

                // Position relative to the object origin
                float3 rel = i.worldPos - objWorldOrigin;

                // Choose plane, but keep it aligned with the object's rotation
                float2 p = (_GridPlane >= 0.5)
                         ? float2(dot(rel, axisX), dot(rel, axisZ))  // XZ plane in object space
                         : float2(dot(rel, axisX), dot(rel, axisY)); // XY plane in object space

                // Avoid divide-by-zero (if user sets 0)
                float2 cellSize = max(_CellSize.xy, float2(1e-6, 1e-6));

                // Compute checker parity in object-aligned world units
                float2 cell = floor(p / cellSize);
                float parity = fmod(cell.x + cell.y, 2.0);

                return lerp(_ColorIn, _ColorOut, parity);
            }
            ENDCG
        }
    }
}
