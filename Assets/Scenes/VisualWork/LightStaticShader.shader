Shader "Custom/LightStaticShader"
{
    Properties
    {
        [Header(Base Textures)]
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseTex ("Base Texture (Albedo)", 2D) = "white" {}
        _NormalTex ("Normal Texture", 2D) = "bump" {}
        _MetallicGlossTex ("Metallic, Roughness", 2D) = "white" {} // R: Metallic, G: AO, B: Not Used, A: Smoothness
        _EmissionTex ("Emission Texture", 2D) = "black" {}
        _EmissionColor ("Emission Color", Color) = (0,0,0,1)

        [Header(Baked Lighting)]
        _BakedLightingTex ("Baked Lighting Texture", 2D) = "white" {}
        _BakedLightingTex_ST ("Baked Lighting Texture Tiling", Vector) = (1,1,0,0) // xy=scale, zw=offset
        _LightingMultiplier ("Lighting Multiplier", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include <HLSLSupport.cginc>

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1; // Baked UV (UV1, TEXCOORD1)
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1; // Baked UV
            };

            sampler2D _BaseTex;
            float4 _BaseTex_ST;
            fixed4 _BaseColor;

            sampler2D _NormalTex;
            float4 _NormalTex_ST;

            sampler2D _MetallicGlossTex;
            float4 _MetallicGlossTex_ST;

            sampler2D _EmissionTex;
            float4 _EmissionTex_ST;
            fixed4 _EmissionColor;

            sampler2D _BakedLightingTex;
            float4 _BakedLightingTex_ST; // x=scale.x, y=scale.y, z=offset.x, w=offset.y
            float _LightingMultiplier;

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv0 = TRANSFORM_TEX(input.uv0, _BaseTex);

                // Apply lightmap scale and offset to UV1 (baked UV)
                output.uv1 = input.uv1 * _BakedLightingTex_ST.xy + _BakedLightingTex_ST.zw;

                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                // 1. Sample base textures
                half4 baseColor = tex2D(_BaseTex, input.uv0) * _BaseColor;
                half4 emission = tex2D(_EmissionTex, input.uv0) * _EmissionColor;

                // 2. Sample baked lighting using UV1 with offset and scale applied
                half4 bakedLighting = tex2D(_BakedLightingTex, input.uv1);

                half3 finalLighting = bakedLighting.rgb * _LightingMultiplier;

                // 3. Combine base color and lighting
                half4 finalColor = half4(baseColor.rgb * finalLighting, baseColor.a);

                // 4. Add emission
                finalColor.rgb += emission.rgb;

                return finalColor;
            }
            ENDHLSL
        }
    }
}