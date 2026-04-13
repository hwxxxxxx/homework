Shader "Game/Enemy/Dissolve"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _Dissolve("Dissolve", Range(0,1)) = 0
        _EdgeWidth("Edge Width", Range(0.001,0.5)) = 0.1
        _EdgeColor("Edge Color", Color) = (1,0.5,0.2,1)
        _NoiseScale("Noise Scale", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Dissolve;
                float _EdgeWidth;
                float4 _EdgeColor;
                float _NoiseScale;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            float Hash31(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                float noise = Hash31(IN.positionWS * _NoiseScale);
                float threshold = _Dissolve;
                float edge = smoothstep(threshold, threshold + _EdgeWidth, noise);
                float cut = noise - threshold;
                clip(cut);

                float bodyFade = saturate(1.0 - _Dissolve);
                half4 edgeColor = _EdgeColor * (1.0 - edge);
                half3 finalRgb = lerp(edgeColor.rgb, baseColor.rgb, edge) + edgeColor.rgb * 0.35;
                half finalAlpha = baseColor.a * bodyFade;

                return half4(finalRgb, finalAlpha);
            }
            ENDHLSL
        }
    }
}
