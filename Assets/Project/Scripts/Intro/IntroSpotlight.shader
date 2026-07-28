Shader "UI/IntroSpotlight"
{
    // Unity UI Image가 전달하는 기본 텍스처/색상과 손전등 제어값입니다.
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0, 0, 0, 1)
        _SpotlightPosition ("Spotlight Position", Vector) = (0.5, 0.5, 0, 0)
        _SpotlightSize ("Spotlight Size", Vector) = (0.32, 0.42, 0, 0)
        _SpotlightSoftness ("Spotlight Softness", Range(0.001, 1)) = 0.35
        _Darkness ("Darkness", Range(0, 1)) = 0.92
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        // 일반 UI와 같은 투명 렌더링 순서를 사용합니다.
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
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
            Name "Spotlight"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float2 _SpotlightPosition;
            float2 _SpotlightSize;
            float _SpotlightSoftness;
            float _Darkness;

            // UI 정점을 클립 공간으로 변환하고 UV와 색상을 Fragment 단계로 전달합니다.
            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.texcoord = input.texcoord;
                output.color = input.color * _Color;
                return output;
            }

            // 타원 중심과의 거리를 계산하여 타원 안쪽은 투명하게,
            // 바깥쪽은 Darkness 값만큼 어둡게 덮습니다.
            fixed4 frag(v2f input) : SV_Target
            {
                float2 safeSize = max(_SpotlightSize * 0.5, float2(0.0001, 0.0001));
                float distanceFromCenter = length((input.texcoord - _SpotlightPosition) / safeSize);
                float innerEdge = saturate(1.0 - _SpotlightSoftness);
                float outsideSpotlight = smoothstep(innerEdge, 1.0, distanceFromCenter);

                fixed4 color = input.color;
                color.rgb = _Color.rgb;
                color.a = outsideSpotlight * _Darkness * input.color.a;
                return color;
            }
            ENDCG
        }
    }
}
