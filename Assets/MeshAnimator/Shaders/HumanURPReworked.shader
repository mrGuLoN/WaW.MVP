Shader "Mesh Animator/Example/HumanURPReworked" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MaxLights ("Max Lights", Float) = 10.0
        _LightPower("Light power", Range(0,1)) = 0.8
        _ShadowPower("Shadow power", Range(0,1)) = 0.8
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 150

        Pass {
            Name "FORWARD"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "MeshAnimator.cginc"

            // Properties
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _LightPower;
            float _ShadowPower;
            float _MaxLights;

            StructuredBuffer<float3> _LightPositions; // Массив позиций источников света
            StructuredBuffer<float4> _LightColors; // Массив цветов источников света
            StructuredBuffer<float2> _RangeIntensityBuffer; // Массив диапазонов и интенсивностей

            struct appdata_ma {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                uint vertexId : SV_VertexID;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                float3 worldPos : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata_ma v) {
                UNITY_SETUP_INSTANCE_ID(v);
                
                // Применяем анимацию к вершинам и нормалям
                v.vertex = ApplyMeshAnimation(v.vertex, v.vertexId);
                v.normal = GetAnimatedMeshNormal(v.normal, v.vertexId);
                
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = normalize(v.normal); // Нормализуем нормали
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); // Получаем мировую позицию
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                return o;
            }

            float4 frag(v2f i) : SV_Target {
                // Получаем цвет из основной текстуры
                float4 mainTexColor = tex2D(_MainTex, i.uv);
                float3 finalColor = float3(0.0, 0.0, 0.0);

                // Проходим по всем источникам света
                for (int index = 0; index < _MaxLights; index++) {
                    float3 lightPos = _LightPositions[index].xyz;
                    float3 lightColor = _LightColors[index].rgb;
                    float2 lightRangeIntensity = _RangeIntensityBuffer[index];

                    // Вычисляем вектор от источника света до текущей позиции
                    float3 lightDir = lightPos - i.worldPos;
                    float distanceSquared = dot(lightDir, lightDir); // Квадрат расстояния
                    float maxDistanceSquared = lightRangeIntensity.x * lightRangeIntensity.x; // Квадрат максимального расстояния

                    // Проверяем, не превышает ли квадрат расстояния максимальный квадрат расстояния
                    if (distanceSquared < maxDistanceSquared) {
                        // Нормализуем направление света
                        lightDir = normalize(lightDir);

                        // Вычисляем интенсивность освещения с учетом расстояния
                        float attenuation = saturate((1.0 - (sqrt(distanceSquared) / lightRangeIntensity.x)) * lightRangeIntensity.y); // Затухание

                        // Вычисляем угол между нормалью и направлением света
                        float angle = dot(i.normal, lightDir);
                        float influence = 0.0;                     

                        // Определяем влияние света на основе угла
                        if (angle > 0.0) { // Нормаль и направление света в одном направлении
                            influence = 0.1 + _LightPower * angle; // Смешиваем влияние
                        }
                        influence = lerp(1.0, influence, _ShadowPower); // Плавный переход

                        // Учитываем затухание и комбинируем цвет освещения с цветом текстуры
                        finalColor += lightColor * influence * attenuation * mainTexColor.rgb;
                    }
                }

                // Умножаем итоговый цвет на заданный цвет
                finalColor *= _Color.rgb;

                // Возвращаем итоговый цвет с сохранением альфа-канала
                return float4(finalColor, mainTexColor.a); // Сохраняем альфа-канал
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
