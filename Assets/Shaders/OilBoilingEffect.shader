Shader "Universal Render Pipeline/Custom/OilBoilingEffect"
{
    Properties
    {
        [MainColor] _BaseColor("Oil Color", Color) = (0.8, 0.6, 0.1, 1)
        [MainTexture] _BaseMap("Base Texture", 2D) = "white" {}
        _NoiseMap("Noise Texture", 2D) = "gray" {}
        _Speed("Boiling Speed", Range(0.1, 5)) = 1.5
        _Distortion("Distortion", Range(0, 0.2)) = 0.1
        _Smoothness("Smoothness", Range(0, 1)) = 0.8
        _Metallic("Metallic", Range(0, 1)) = 0.3
        _BubbleScale("Bubble Scale", Range(0.01, 0.5)) = 0.1
        _FresnelPower("Fresnel Power", Range(1, 10)) = 3
        _EmissionIntensity("Emission Intensity", Range(0, 2)) = 0.5
    }

    SubShader
    {
        Tags 
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                float4 positionCS : SV_POSITION;
                float fogCoord : TEXCOORD4;
            };

            TEXTURE2D(_BaseMap);    SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NoiseMap);   SAMPLER(sampler_NoiseMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _Speed;
                float _Distortion;
                float _Smoothness;
                float _Metallic;
                float _BubbleScale;
                float _FresnelPower;
                float _EmissionIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Анимированный шум для эффекта кипения
                float2 uvNoise = input.positionWS.xz * _BubbleScale;
                uvNoise += _Time.y * _Speed * 0.5;
                
                half3 noise1 = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, uvNoise).rgb;
                half3 noise2 = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, uvNoise * 1.3 + float2(_Time.y * _Speed * 0.3, 0)).rgb;
                
                // Комбинированный шум
                half3 combinedNoise = (noise1 + noise2) * 0.5;
                
                // Искажение UV
                float2 distortedUV = input.uv + combinedNoise.rg * _Distortion;
                
                // Основной цвет
                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, distortedUV);
                half3 albedo = baseTex.rgb * _BaseColor.rgb;
                
                // Нормали для пузырей
                half3 bubbleNormals = half3(
                    combinedNoise.r - 0.5,
                    combinedNoise.g - 0.5,
                    1);
                half3 normalWS = normalize(input.normalWS + bubbleNormals * 0.3);
                
                // Френелевский эффект
                half fresnel = pow(1.0 - saturate(dot(normalize(input.viewDirWS), normalWS)), _FresnelPower);
                half3 emission = _BaseColor.rgb * fresnel * _EmissionIntensity;
                
                // Данные освещения
                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = input.positionWS;
                lightingInput.normalWS = normalWS;
                lightingInput.viewDirectionWS = normalize(input.viewDirWS);
                lightingInput.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                lightingInput.fogCoord = input.fogCoord;
                
                SurfaceData surfaceInput = (SurfaceData)0;
                surfaceInput.albedo = albedo;
                surfaceInput.metallic = _Metallic;
                surfaceInput.specular = half3(0.5h, 0.5h, 0.5h);
                surfaceInput.smoothness = _Smoothness * (0.5 + 0.5 * sin(_Time.y * _Speed));
                surfaceInput.emission = emission;
                surfaceInput.alpha = _BaseColor.a;
                
                // Применяем стандартное освещение URP
                half4 color = UniversalFragmentPBR(lightingInput, surfaceInput);
                color.rgb = MixFog(color.rgb, input.fogCoord);
                
                return color;
            }
            ENDHLSL
        }
        
        // Пасс для теней (опционально)
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
    
    FallBack "Universal Render Pipeline/Lit"
}