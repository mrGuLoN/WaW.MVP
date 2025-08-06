Shader "Custom/LightNewShaderLit"
{
    Properties
    {
        [MainTexture] _MainTex ("Main Texture", 2D) = "white" {}
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _EmissionMap ("Emission Map", 2D) = "black" {}
        
        // Параметры дизольва
        _DissolveTex ("Dissolve Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _DissolveEdgeWidth ("Edge Width", Range(0, 0.2)) = 0.1
        [HDR] _DissolveEdgeColor ("Edge Color", Color) = (1, 1, 0, 1)
        _DissolveEdgeIntensity ("Edge Intensity", Range(0, 10)) = 1.0
        
        // Дополнительные эффекты
        _RimPower ("Rim Power", Range(0, 10)) = 3.0
        [HDR] _RimColor ("Rim Color", Color) = (1,1,1,1)
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(-2, 2)) = 1.0
        
        // Контроль яркости
        _MaxBrightness ("Max Brightness", Range(0.1, 10)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _EMISSION_MAP_ON
            #pragma multi_compile _ _NORMALMAP
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 lightmapUV : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 normal : NORMAL;
                float2 emissionUV : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 5);
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);
            TEXTURE2D(_DissolveTex);
            SAMPLER(sampler_DissolveTex);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _EmissionMap_ST;
                float4 _EmissionColor;
                
                // Дизольв параметры
                float4 _DissolveTex_ST;
                float _DissolveAmount;
                float _DissolveEdgeWidth;
                float4 _DissolveEdgeColor;
                float _DissolveEdgeIntensity;
                
                // Дополнительные эффекты
                float _RimPower;
                float4 _RimColor;
                float _NormalStrength;
                
                // Контроль яркости
                float _MaxBrightness;
            CBUFFER_END

            v2f vert(appdata_t v)
            {
                v2f o;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                o.pos = vertexInput.positionCS;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.emissionUV = TRANSFORM_TEX(v.uv, _EmissionMap);
                o.worldPos = vertexInput.positionWS;
                o.normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                o.viewDir = GetWorldSpaceViewDir(vertexInput.positionWS);
                o.shadowCoord = TransformWorldToShadowCoord(vertexInput.positionWS);
                
                // Light Probes
                OUTPUT_LIGHTMAP_UV(v.lightmapUV, unity_LightmapST, o.lightmapUV);
                OUTPUT_SH(o.normal, o.vertexSH);
                
                return o;
            }

            float3 ApplyNormalMap(float2 uv, float3 normal, float3 tangent, float3 bitangent)
            {
                #ifdef _NORMALMAP
                    float4 normalMap = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv);
                    float3 normalTS = UnpackNormal(normalMap);
                    normalTS.z *= _NormalStrength;
                    float3x3 TBN = float3x3(tangent, bitangent, normal);
                    return normalize(mul(normalTS, TBN));
                #else
                    return normal;
                #endif
            }

            float3 SafeBrightness(float3 color, float maxBrightness)
            {
                float brightness = max(max(color.r, color.g), color.b);
                if (brightness > maxBrightness)
                {
                    return color * (maxBrightness / brightness);
                }
                return color;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Нормали и векторы
                float3 normal = normalize(i.normal);
                float3 viewDir = normalize(i.viewDir);
                
                // Основной цвет текстуры
                float4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                
                // Настройка данных для освещения
                InputData inputData = (InputData)0;
                inputData.positionWS = i.worldPos;
                inputData.normalWS = normal;
                inputData.viewDirectionWS = viewDir;
                inputData.shadowCoord = i.shadowCoord;
                inputData.bakedGI = SAMPLE_GI(i.lightmapUV, i.vertexSH, normal);
                
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = mainTexColor.rgb;
                surfaceData.alpha = mainTexColor.a;
                surfaceData.emission = 0;
                surfaceData.specular = 0;
                surfaceData.metallic = 0;
                surfaceData.smoothness = 0;
                surfaceData.occlusion = 1;
                
                // Получаем освещение
                float4 finalColor = UniversalFragmentPBR(inputData, surfaceData);
                
                // Дизольв эффект
                float dissolveValue = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, i.uv).r - _DissolveAmount;
                if (dissolveValue < 0) discard;
                
                float edge = smoothstep(0, _DissolveEdgeWidth, dissolveValue);
                float3 edgeColor = _DissolveEdgeColor.rgb * (1 - edge) * _DissolveEdgeIntensity;
                
                // Эмиссия
                float3 emission = float3(0, 0, 0);
                #ifdef _EMISSION_MAP_ON
                    float3 emissionMap = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, i.emissionUV).rgb;
                    emission = SafeBrightness(emissionMap * _EmissionColor.rgb, _MaxBrightness);
                #endif

                // Rim эффект
                float rim = saturate(1.0 - dot(viewDir, normal));
                float3 rimLight = SafeBrightness(_RimColor.rgb * pow(rim, _RimPower), _MaxBrightness);
                
                // Комбинируем с контролем яркости
                float3 combinedColor = SafeBrightness(
                    (finalColor.rgb + emission + rimLight) * edge + edgeColor,
                    _MaxBrightness
                );
                
                return float4(combinedColor, mainTexColor.a * step(0, dissolveValue));
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };
            
            TEXTURE2D(_DissolveTex);
            SAMPLER(sampler_DissolveTex);
            
            CBUFFER_START(UnityPerMaterial)
                float _DissolveAmount;
            CBUFFER_END
            
            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                float dissolveValue = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, i.uv).r - _DissolveAmount;
                clip(dissolveValue);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}