Shader "Hidden/nickeltin/SDF/UI Legacy" 
{
    Properties 
    {
        //#region Unity UI/Default
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        [PerRendererData] _Color ("Tint", Color) = (1,1,1,1)
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        //#endregion
        
        
        //#region Custom
        //This is actually an SDF texture, but called _AlphaTex to be able to set it natively with CanvasRenderer.SetAlphaTexture()
//        [PerRendererData] _AlphaTex("SDF Texture", 2D) = "black" {}

        _FaceColor("Face Color", Color) = (1,1,1,1)
        _FaceTex("Face Texture", 2D) = "white" {}
        _FaceDilate("Face Dilate", Range(-1,1)) = 0.3
        [NoScaleOffset] _FaceAnimationTex("Face Anim Tex", 2D) = "white" {} 
        _FaceSpeed("Face Speed", Vector) = (0,0,0,0)
        
        [Toggle(OUTLINE_ON)] _EnableOutline("Enable Outline", Float) = 0
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineTex("Outline Texture", 2D) = "white" {}
        _OutlineWidth("Outline Thickness", Range(0,1)) = 0.1    
        _OutlineSoftness("Outline Softness", Range(0,1)) = 0.1
         [NoScaleOffset] _OutlineAnimationTex("Outline Anim Tex", 2D) = "white" {} 
        _OutlineSpeed("Outline Speed", Vector) = (0,0,0,0)
        
        [Toggle(UNDERLAY_ON)] UNDERLAY("Underlay", Float) = 1
        _UnderlayColor("Border Color", Color) = (0,0,0,0.5)
//        _UnderlayTex("Underlay Texture", 2D) = "white" {}
        _UnderlayOffsetX("Border OffsetX", Range(-0.5,0.5)) = 0
        _UnderlayOffsetY("Border OffsetY", Range(-0.5,0.5)) = 0
        _UnderlayDilate("Border Dilate", Range(-1,1)) = 0
        _UnderlaySoftness("Border Softness", Range(0,1)) = 0
        
        _GradientScale("Gradient Scale", float) = 5
        //#endregion
    }
    
    SubShader 
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="False"
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
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        Pass
        {
            Name "Default SDF"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #pragma multi_compile_local _ SDF_PREVIEW
            
            #pragma multi_compile_local _ OUTLINE_ON
            #pragma multi_compile_local _ UNDERLAY_ON
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float4 uv0 : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 uv0 : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4 mask : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            //Default Unity UI/Default
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
           
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            //Custom
            // sampler2D _AlphaTex;
            // float4 _AlphaTex_ST;
            // float4 _AlphaTex_TexelSize;
            
            half4 _FaceColor;
            fixed _FaceDilate;
            half _GradientScale;
            
            sampler2D _FaceTex;
            float4 _FaceTex_ST;
            
            sampler2D _FaceAnimationTex;
            float3 _FaceSpeed;

#if OUTLINE_ON
            fixed _OutlineSoftness;
            fixed _OutlineWidth;
            half4 _OutlineColor;
            sampler2D _OutlineTex;
            float4 _OutlineTex_ST;
            sampler2D _OutlineAnimationTex;
            float2 _OutlineSpeed;
#endif
            
#if UNDERLAY_ON
            fixed _UnderlayOffsetX;
            fixed _UnderlayOffsetY;
            fixed _UnderlayDilate;
            fixed _UnderlaySoftness;
            half4 _UnderlayColor;
            // sampler2D _UnderlayTex;
            // float4 _UnderlayTex_ST;
#endif

            v2f vert (appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                float4 vPosition = UnityObjectToClipPos(v.vertex);
                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;

                
                float2 pixelSize = vPosition.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                // float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
                OUT.uv0 = float4(TRANSFORM_TEX(v.uv0.xy, _MainTex), v.uv0.z, v.uv0.w);
                OUT.mask = float4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 /
                    (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));
                
                OUT.color = v.color * _Color;
                return OUT;
            }

            float invLerp(const float a, const float b, const float value)
            {
                return clamp((value - a) / (b - a), 0.0, 1.0);
            }
            
            fixed4 sdfFrag(v2f IN)
            {
                // return IN.;
                half sdfMultiplier = IN.uv0.z;
                half gradientScale = _GradientScale;
                
#ifdef SDF_PREVIEW
                sdfMultiplier = 1;
                // gradientScale = 0;
#endif

                // return IN.uv0;
                const half faceDialate = _FaceDilate * sdfMultiplier;
                const half scale = 1.0 / (gradientScale * fwidth(IN.uv0));
                half bias = 0.5 - faceDialate / 2;
                
                // Compute density value
                fixed d = tex2D(_MainTex, IN.uv0).a;

                const half mainFrom = max(sign(d - 0.5), 0);
                const half mainTo = saturate((d - bias) * scale + 0.5);
                // Compute result color
                half4 sdfColor = _FaceColor * mainTo;

                const half d_remaped = invLerp(0.5, bias, d);
                half t = invLerp(mainFrom, mainTo, d_remaped);
                const float radianAngle = radians(_FaceSpeed.z * _Time.y);
                const float cosAngle = cos(radianAngle);
                const float sinAngle = sin(radianAngle);
                half2 uv = IN.uv0 + _FaceSpeed.xy * _Time.x;

                const float2 rotatedUV = float2(
                    uv.x * cosAngle - uv.y * sinAngle,
                    uv.x * sinAngle + uv.y * cosAngle
                );
                
                sdfColor *= tex2D(_FaceTex, half2(t, 0)) * tex2D(_FaceAnimationTex, rotatedUV);
                
                
                // Append outline
#if OUTLINE_ON
                half outlineWidth = _OutlineWidth * sdfMultiplier;
                if (outlineWidth > 0)
                {
                    half outlineFade = max(_OutlineSoftness, fwidth(IN.uv0 * gradientScale));
                    half ol_from = min(1, bias + outlineWidth / 2 + outlineFade / 2);
                    half ol_to = max(0, bias - outlineWidth / 2 - outlineFade / 2);
                    half ol_t = invLerp(ol_from, ol_to, d);
                    half4 overlay = tex2D(_OutlineAnimationTex, IN.uv0 + _OutlineSpeed * _Time.x);
                    sdfColor = lerp(sdfColor, _OutlineColor
                        * tex2D(_OutlineTex, half2(ol_t,0))
                        * overlay, saturate((ol_from - d) / outlineFade) * overlay.a);
                    sdfColor *= saturate((d - ol_to) / outlineFade);
                }
#endif

                // Append underlay (drop shadow)
#if UNDERLAY_ON
                half underlayDialate = _UnderlayDilate * sdfMultiplier;
                half underlayOfssetX = _UnderlayOffsetX * sdfMultiplier;
                half underlayOfssetY = _UnderlayOffsetY * sdfMultiplier;
                {
                    half ul_from = max(0, bias - underlayDialate - _UnderlaySoftness / 2);
                    half ul_to = min(1, bias - underlayDialate + _UnderlaySoftness / 2);
                    float2 underlayUV = IN.uv0 - float2(underlayOfssetX, underlayOfssetY);
                    d = tex2D(_MainTex, underlayUV).a;
                    // half ul_t = invLerp(0, ul_from, d); 
                    half4 shadow  = float4(_UnderlayColor.rgb, 1) * (_UnderlayColor.a * (1 - sdfColor.a)) *
                        saturate((d - ul_from) / (ul_to - ul_from));
                    sdfColor += shadow;
                    // sdfColor = lerp(shadow, sdfColor, sdfColor.a);
                }
#endif

                sdfColor *= IN.color;
                return sdfColor;
            }
            
            //Default UI frag 
            fixed4 frag(v2f IN) : SV_Target
            {
                //Round up the alpha color coming from the interpolator (to 1.0/256.0 steps)
                //The incoming alpha could have numerical instability, which makes it very sensible to
                //HDR color transparency blend, when it blends with the world's texture.
                const half alphaPrecision = half(0xff);
                const half invAlphaPrecision = half(1.0/alphaPrecision);
                IN.color.a = round(IN.color.a * alphaPrecision)*invAlphaPrecision;

                half4 color = IN.color * (tex2D(_MainTex, IN.uv0) + _TextureSampleAdd);

                if (IN.uv0.w > 0)
                {
                    color = sdfFrag(IN);
                }
                
#ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
#endif

#ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
#endif

                color.rgb *= color.a;
                return color;
            }
            
            ENDCG
        }
    }
    
    Fallback "UI/Default"
//    CustomEditor "nickeltin.SDF.Editor.SDFShaderGUI"
}
