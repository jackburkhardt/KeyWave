Shader "UI/CircleMask"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
        
        _Circle ("Circle", Range(0, 1)) = 1
        
        _RadiusX ("Outer Radius X", Range(0,1)) = 1
        _RadiusY ("Outer Radius Y", Range(0,1)) = 1
        
        _ScaleX ("Scale X", Float) = 1
        _ScaleY ("Scale Y", Float) = 1
        
        _IRadiusX ("Inner Radius X", Range(0,1)) = 1
        _IRadiusY ("Inner Radius Y", Range(0,1)) = 1
        
      
        _AntialiasThreshold ("Antialias Threshold", Range(0, 0.9999)) = 0.96

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
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
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float2 originalTexcoord : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _RadiusX;
            float _RadiusY;
            float _ScaleX;
            float _ScaleY;
            float _AntialiasThreshold;
            float _IRadiusX;
            float _IRadiusY;
            float _IScaleX;
            float _IScaleY;
            int _Circle;

            float circleDelta(float2 texcoord) {
                if (_RadiusX <= 0.0001 || _RadiusY <= 0.0001) return 0;
                float x = (2 * (texcoord.x - 0.5)) / _RadiusX;
                float y = _Circle == 1 ? (2 * (texcoord.y - 0.5)) / _RadiusX :  (2 * (texcoord.y - 0.5)) / _RadiusY;

               
                
                float value = x * x + y * y;

                value = smoothstep(1.0, _AntialiasThreshold, value);

                float ivalue = 0;

                if (_IRadiusX > 0.0001 && _IRadiusY > 0.0001) {
                    float ix = (2 * (texcoord.x - 0.5)) / _IRadiusX;
                    float iy = _Circle == 1 ? (2 * (texcoord.y - 0.5)) / _IRadiusX : (2 * (texcoord.y - 0.5)) / _IRadiusY;
                    ivalue = ix * ix + iy * iy;
                    ivalue = smoothstep(1.0, _AntialiasThreshold, ivalue);
                }

                return value - ivalue;
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.originalTexcoord = v.texcoord;
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // scale before circle clipping
                float2 size = float2(1 / _ScaleX, 1 / _ScaleY);
                float2 halfSize = size * 0.5;
                float2 center = float2(0.5, 0.5);
                half4 color = tex2D(_MainTex, IN.texcoord * size + center - halfSize) * IN.color;
    
                color.a *= circleDelta(IN.texcoord);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}