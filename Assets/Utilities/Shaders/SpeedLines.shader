Shader "Hidden/Custom/AnimeSpeedLinesPostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Strength ("Strength", Range(0, 1)) = 0.5
        _Speed ("Animation Speed", Range(0, 10)) = 3.0
        _LineCount ("Line Count", Range(10, 100)) = 40
        _LineWidth ("Line Width", Range(0.01, 0.5)) = 0.1
        _FadeDistance ("Edge Fade Distance", Range(0.1, 10)) = 0.2
        _CenterOffset ("Center Offset", Vector) = (0,0,0,0)
    }
    
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);
    
    float _Strength;
    float _Speed;
    float _LineCount;
    float _LineWidth;
    float _FadeDistance;
    float2 _CenterOffset;

    struct Attributes
    {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    Varyings Vert(Attributes IN)
    {
        Varyings OUT;
        OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
        OUT.uv = IN.uv;
        return OUT;
    }

    float GenerateAnimeSpeedLines(float2 uv)
    {
        float2 centeredUV = uv - 0.5 - _CenterOffset;
        float angle = atan2(centeredUV.y, centeredUV.x);
        float radius = length(centeredUV);
        
        float animatedAngle = angle + _Time.y * _Speed;
        float linePattern = step(1.0 - _LineWidth, frac(animatedAngle * (_LineCount / 6.28318))); 
        
        float edgeMask = smoothstep(1.0 - _FadeDistance, 1.0, radius) * smoothstep(0.0, _FadeDistance, radius);
        float dirBias = 1.0 - 0.7 * abs(sin(angle));
        
        return linePattern * edgeMask * dirBias;
    }
    
    half4 Frag(Varyings IN) : SV_Target
    {
        half4 sceneColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
        float lines = GenerateAnimeSpeedLines(IN.uv);
        
        half3 darkenedBackground = sceneColor.rgb * (1.0 - lines * _Strength * 0.3);
        half3 finalColor = darkenedBackground + lines * _Strength;
        
        finalColor = min(finalColor, 1.0);
        
        return half4(finalColor, sceneColor.a);
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        
        Cull Off
        ZWrite Off
        ZTest Always
        
        Pass
        {
            Name "AnimeSpeedLinesEffect"
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}