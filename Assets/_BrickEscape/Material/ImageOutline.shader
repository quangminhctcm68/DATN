Shader "UI/SDF_HDR_Outline"
{
    Properties
    {
        _MainTex ("SDF Texture", 2D) = "white" {}
        _FillColor ("Fill Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color (HDR)", Color) = (3,2,0.5,1)

        _Threshold ("SDF Threshold", Range(0,1)) = 0.5
        _OutlineWidth ("Outline Width", Range(0,0.2)) = 0.05
        _Glow ("Glow Intensity", Range(0,5)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _FillColor;
            float4 _OutlineColor;
            float _Threshold;
            float _OutlineWidth;
            float _Glow;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float sdf = tex2D(_MainTex, i.uv).r;

                float fill = smoothstep(_Threshold - 0.01, _Threshold + 0.01, sdf);
                float outline = smoothstep(
                    _Threshold - _OutlineWidth,
                    _Threshold,
                    sdf
                );

                float glow = outline - fill;

                fixed4 col = _FillColor * fill;
                col.rgb += _OutlineColor.rgb * glow * _Glow;
                col.a = max(fill, glow);

                return col;
            }
            ENDCG
        }
    }
}
