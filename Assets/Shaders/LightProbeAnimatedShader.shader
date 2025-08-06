Shader "Custom/URP/AnimatedTexture"
{
    Properties
    {
        _AnimationTexture("Animation Texture", 2D) = "white" {} // Текстура анимации
        _TexelSize("TexelSize", Vector) = (0, 0, 0, 0)         // Размер текселей текстуры
        _FrameRate("Frame Rate", Float) = 30                   // Количество кадров в секунду
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            HLSLPROGRAM

            // Включаем библиотеку URP
            #pragma vertex VertexFunction
            #pragma fragment FragmentFunction

            // Подключаем необходимые файлы URP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Свойства шейдера
            sampler2D _AnimationTexture; // Текстура анимации

            float _FrameRate; // Частота кадров
            uint2 _TexelSize; // Размер текселей текстуры

            struct Attributes
            {
                float4 positionOS : POSITION; // Локальная позиция вершины
                float2 uv : TEXCOORD0;        // UV координаты
                uint vertexID : SV_VertexID; // ID вершины
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION; // Позиция в пространстве камеры
                float3 normalWS : NORMAL;         // Нормаль в мировом пространстве
            };

            // Вершинная функция
            Varyings VertexFunction(Attributes input)
            {
                Varyings output;

                // Получаем текущее время
                float time = _Time.y;

                // Количество кадров в текстуре
                float frameCount = _TexelSize.x;

                // Продолжительность анимации
                float duration = frameCount / _FrameRate;

                // Нормализованное время (циклическое от 0 до 1)
                float normalizedTime = fmod(time / duration, 1.0);

                // Вычисляем UV для позиции вершины
                float positionY = (input.vertexID * 2 + 0.5) * _TexelSize.y;
                float2 positionUV = float2(normalizedTime, positionY);

                // Считываем позицию вершины из текстуры
                float4 newPositionData = tex2Dlod(_AnimationTexture, float4(positionUV,0,0));
                float3 newPosition = newPositionData.xyz;
                if (any(isnan(newPosition)) || any(isinf(newPosition)))
                {
                    newPosition = float3(0, 0, 0); // Заменяем некорректные значения на ноль
                }

                // Вычисляем UV для нормали вершины
                float normalY = (input.vertexID * 2 + 1.5) * _TexelSize.y;
                float2 normalUV = float2(normalizedTime, normalY);

                // Считываем нормаль вершины из текстуры
                float4 newNormalData = tex2Dlod(_AnimationTexture, float4(normalUV,0,0));
                float3 newNormal = newNormalData.xyz;

                // Преобразуем позицию в пространство мира
                float4 worldPosition = mul(unity_ObjectToWorld, float4(newPosition, 1.0));

                // Преобразуем позицию в пространство камеры (clip space)
                output.positionHCS = TransformWorldToHClip(worldPosition.xyz);

                // Преобразуем нормаль в мировое пространство
                output.normalWS = TransformObjectToWorldNormal(newNormal);

                return output;
            }

            // Фрагментная функция
            half4 FragmentFunction(Varyings input) : SV_Target
            {
                // Для тестирования: выведем нормаль как цвет
                return half4(1, 0, 0, 1);

                // Альтернативно: можно вывести одинаковый цвет для всех пикселей
                // return half4(1, 0, 0, 1); // Красный цвет
            }

            ENDHLSL
        }
    }
}