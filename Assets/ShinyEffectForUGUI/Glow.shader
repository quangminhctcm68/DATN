Shader "UI/Hidden/UI-Effect-ShinyRadial"
{
    Properties
    {
        [PerRendererData] _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;

                half4 effectFactor : TEXCOORD2;
                half2 effectFactor2 : TEXCOORD3;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            // ===== unpack giữ nguyên =====
            fixed4 UnpackToVec4(float v)
            {
                const int STEP = 64;
                const int PREC = STEP - 1;
                fixed4 o;
                o.r = (v % STEP) / PREC; v = floor(v / STEP);
                o.g = (v % STEP) / PREC; v = floor(v / STEP);
                o.b = (v % STEP) / PREC; v = floor(v / STEP);
                o.a = (v % STEP) / PREC;
                return o;
            }

            half2 UnpackToVec2(float v)
            {
                const int STEP = 4096;
                const int PREC = STEP - 1;
                half2 o;
                o.x = (v % STEP) / PREC;
                v = floor(v / STEP);
                o.y = (v % STEP) / PREC;
                return o;
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.worldPosition = IN.vertex;
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;

                OUT.effectFactor = UnpackToVec4(IN.uv1.x);
                OUT.effectFactor2 = UnpackToVec2(IN.uv1.y);

                // center offset
                OUT.effectFactor2.x = OUT.effectFactor2.x * 2 - 0.5;

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd;
                fixed alpha = col.a;
                col *= IN.color;
                col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                // ===== ĐIỂM KHÁC BIỆT =====
                // TỎA 2 BÊN TỪ CENTER
                half pos = abs(IN.effectFactor.x - IN.effectFactor2.x);

                half normalized = 1 - saturate(pos / IN.effectFactor.z);
                half shine = smoothstep(0, IN.effectFactor.y * 2, normalized);

                half3 reflect = lerp(1, col.rgb * 10, IN.effectFactor2.y);
                col.rgb += alpha * shine * IN.effectFactor.w * reflect;

                return col;
            }
        ENDCG
        }
    }
}
