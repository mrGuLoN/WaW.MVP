Shader "Custom/RandomDeformationShader"
{
    Properties
    {
        _DeformAmount("Deformation Amount", Range(0, 1)) = 0.1
        _TimeSpeed("Time Speed", Range(0, 5)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        Pass
        {
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
                float4 vertex : SV_POSITION;
            };

            float _DeformAmount;
            float _TimeSpeed;

            v2f vert(appdata_t v)
            {
                v2f o;
                
                // Время для создания анимации
                float time = _Time.y * _TimeSpeed;

                // Случайная деформация на основе синуса
                float deform = sin(v.vertex.x * 10.0 + time) * _DeformAmount;

                // Применяем деформацию к вершине
                v.vertex.y += deform;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(1, 1, 1, 1); // Белый цвет
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}