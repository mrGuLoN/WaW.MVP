Shader "Custom/LightNewShader"
{
    Properties
    {
        [MainTexture] _MainTex ("Main Texture", 2D) = "white" {}
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _EmissionMap ("Emission Map", 2D) = "black" {}
        _MaxLights ("Max Lights", Float) = 10.0
        _LightPower("Light power", Range(0,1)) = 0.8
        _ShadowPower("Shadow power", Range(0,1)) = 0.8
        _EmissionIntensity("Emission Intensity", Range(0, 10)) = 1.0
        
        // Улучшенные параметры дизольва
        _DissolveTex ("Dissolve Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _DissolveEdgeWidth ("Edge Width", Range(0, 0.2)) = 0.1
        [HDR] _DissolveEdgeColor ("Edge Color", Color) = (1, 1, 0, 1)
        _DissolveEdgeIntensity ("Edge Intensity", Range(0, 10)) = 1.0
        _DissolveNoiseScale ("Noise Scale", Range(0, 10)) = 1.0
        _DissolveNoiseSpeed ("Noise Speed", Range(0, 1)) = 0.1
        
        // Дополнительные эффекты
        _RimPower ("Rim Power", Range(0, 10)) = 3.0
        [HDR] _RimColor ("Rim Color", Color) = (1,1,1,1)
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(-2, 2)) = 1.0
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 normal : NORMAL;
                float2 emissionUV : TEXCOORD2;
                float2 dissolveUV : TEXCOORD3;
                float3 tangent : TEXCOORD4;
                float3 bitangent : TEXCOORD5;
                float3 viewDir : TEXCOORD6;
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
                float _LightPower;
                float _ShadowPower;
                float _MaxLights;
                float _EmissionIntensity;
                
                // Дизольв параметры
                float4 _DissolveTex_ST;
                float _DissolveAmount;
                float _DissolveEdgeWidth;
                float4 _DissolveEdgeColor;
                float _DissolveEdgeIntensity;
                float _DissolveNoiseScale;
                float _DissolveNoiseSpeed;
                
                // Дополнительные эффекты
                float _RimPower;
                float4 _RimColor;
                float _NormalStrength;
            CBUFFER_END

            StructuredBuffer<float3> _LightPositions;
            StructuredBuffer<float4> _LightColors;
            StructuredBuffer<float2> _RangeIntesivityBuffer;

            v2f vert(appdata_t v)
            {
                v2f o;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                o.pos = vertexInput.positionCS;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.emissionUV = TRANSFORM_TEX(v.uv, _EmissionMap);
                o.dissolveUV = TRANSFORM_TEX(v.uv, _DissolveTex) * _DissolveNoiseScale;
                o.worldPos = vertexInput.positionWS;
                o.normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                
                // Тангенциальное пространство для нормал мапа
                o.tangent = normalize(mul((float3x3)unity_ObjectToWorld, v.tangent.xyz));
                o.bitangent = cross(o.normal, o.tangent) * v.tangent.w;
                
                // Вектор взгляда
                o.viewDir = GetWorldSpaceViewDir(vertexInput.positionWS);
                
                // Анимируем UV дизольва
                o.dissolveUV += _Time.y * _DissolveNoiseSpeed;
                
                return o;
            }

            float3 ApplyNormalMap(float2 uv, float3 normal, float3 tangent, float3 bitangent)
            {
                #ifdef _NORMALMAP
                    float4 normalMap = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv);
                    float3 normalTS = UnpackNormalScale(normalMap, _NormalStrength);
                    float3x3 TBN = float3x3(tangent, bitangent, normal);
                    return normalize(mul(normalTS, TBN));
                #else
                    return normal;
                #endif
            }

            float4 frag(v2f i) : SV_Target
            {
                // Применяем нормал мап
                float3 normal = ApplyNormalMap(i.uv, i.normal, i.tangent, i.bitangent);
                
                // Улучшенный дизольв эффект с анимацией
                float dissolveNoise = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, i.dissolveUV).r;
                float dissolveValue = dissolveNoise - _DissolveAmount;
                
                if (dissolveValue < 0)
                    discard;
                
                float edge = smoothstep(0, _DissolveEdgeWidth, dissolveValue);
                float3 edgeColor = _DissolveEdgeColor.rgb * (1 - edge) * _DissolveEdgeIntensity;
                
                // Основной цвет текстуры
                float4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float3 finalColor = float3(0.0, 0.0, 0.0);

                // Обработка источников света с учетом нормал мапа
                for (int index = 0; index < _MaxLights; index++)
                {
                    float3 lightPos = _LightPositions[index].xyz;
                    float3 lightColor = _LightColors[index].rgb;
                    float2 lightRangeIntensity = _RangeIntesivityBuffer[index];

                    float3 lightDir = lightPos - i.worldPos;
                    float distanceSquared = dot(lightDir, lightDir);
                    float maxDistanceSquared = lightRangeIntensity.x * lightRangeIntensity.x;

                    if (distanceSquared < maxDistanceSquared)
                    {
                        lightDir = normalize(lightDir);
                        float attenuation = saturate((1.0 - (sqrt(distanceSquared) / lightRangeIntensity.x)) * lightRangeIntensity.y);
                        float angle = dot(normal, lightDir);
                        float influence = 0.0;

                        if (angle > 0.0)
                        {
                            influence = 0.1 + _LightPower * angle;
                        }
                        influence = lerp(1.0, influence, _ShadowPower);
                        finalColor += lightColor * influence * attenuation * mainTexColor.rgb;
                    }
                }

                // Добавление свечения (если есть карта)
                float3 emission = float3(0, 0, 0);
              
                #ifdef _EMISSION_MAP_ON
                    float4 emissionMap = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, i.emissionUV);
                    emission = emissionMap.rgb * _EmissionColor.rgb * _EmissionIntensity;
                #endif

                // Rim lighting эффект
                float rim = 1.0 - saturate(dot(normalize(i.viewDir), normal));
                float3 rimLight = _RimColor.rgb * pow(rim, _RimPower);
                
                // Комбинируем все эффекты
                float3 combinedColor = (finalColor + emission + rimLight) * edge + edgeColor;
                return float4(combinedColor, mainTexColor.a * step(0, dissolveValue));
            }
            ENDHLSL
        }       
      
    }

    FallBack "Universal Render Pipeline/Lit"
}