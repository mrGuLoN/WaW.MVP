Shader "Custom/PointLightShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _LightColor ("Light Color", Color) = (1, 1, 1, 1)
        _LightPosition ("Light Position", Vector) = (0, 5, 0, 0)
        _LightIntensity ("Light Intensity", Float) = 1.0
        _LightRange ("Light Range", Float) = 10.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1; // Мировая позиция для передачи в фрагментный шейдер
            };

            sampler2D _MainTex; // Основная текстура
            float4 _MainTex_ST; // Структура для UV текстуры
            float4 _LightColor; // Цвет источника света
            float4 _LightPosition; // Позиция источника света
            float _LightIntensity; // Интенсивность света
            float _LightRange; // Дальность света

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); // Преобразуем в мировые координаты
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Получаем цвет из основной текстуры
                float4 mainTexColor = tex2D(_MainTex, i.uv);

                // Вычисляем вектор от источника света до текущей позиции
                float3 lightDir = _LightPosition.xyz - i.worldPos;
                float distance = length(lightDir);
                lightDir = normalize(lightDir);

                // Вычисляем интенсивность освещения с учетом расстояния
                float attenuation = saturate(1.0 - (distance / _LightRange)); // Затухание
                float3 normal = float3(0.0, 0.0, 1.0); // Предполагаем, что нормали направлены вперед

                // Вычисляем диффузное освещение
                float diffuse = max(0.0, dot(lightDir, lightDir)) * _LightIntensity * attenuation;

                // Комбинируем цвет освещения с цветом текстуры
                float3 lightColor = _LightColor.rgb * diffuse;
                float3 finalColor = mainTexColor.rgb * lightColor;

                // Возвращаем итоговый цвет
                return float4(finalColor, mainTexColor.a); // Сохраняем альфа-канал
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}

