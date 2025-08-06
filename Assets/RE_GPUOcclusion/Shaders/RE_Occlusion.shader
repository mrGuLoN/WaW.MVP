// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "RE/Occlusion" {
	Properties {
		_MainTex ("MainTex (RGB)", 2D) = "white" {}
		_ObjectIndex( "ObjectIndex", float ) = 0
		_InstanceOffset("InstanceOffset", float ) = 0
		_DebugBoxes( "DebugBoxes", int ) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent" }
		
		LOD 200		
		Blend SrcAlpha OneMinusSrcAlpha
		ZClip Off
		ZTest On
		ZWrite Off
		Cull Off
		
		Pass 
		{
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
			#pragma multi_compile_instancing
			//#pragma enable_d3d11_debug_symbols

            #include "UnityCG.cginc"

			struct VS_IN
			{
                float4 vertex : POSITION;
				uint inst : SV_InstanceID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };
			struct VS_OUT
			{
                float4 pos:SV_POSITION;
				uint inst:INSTANCE;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };
			UNITY_INSTANCING_BUFFER_START( Props )
				UNITY_DEFINE_INSTANCED_PROP( float, _ObjectIndex )
			UNITY_INSTANCING_BUFFER_END( Props )

			uniform sampler2D _MainTex;
			uniform RWStructuredBuffer<uint> CounterBuffer : register(u1);

            VS_OUT vert(VS_IN In)
            {
            	VS_OUT Out;
				UNITY_SETUP_INSTANCE_ID( In );
				UNITY_TRANSFER_INSTANCE_ID( In, Out );
				Out.inst = In.inst;

            	float3 LocalPos = In.vertex.xyz;
            	float3 WorldPosition = mul( unity_ObjectToWorld, float4(LocalPos,1) );
            	
            	Out.pos = mul (UNITY_MATRIX_VP, float4( WorldPosition, 1) );
                return Out;
            }
			
			float _InstanceOffset;
			int _DebugBoxes;

			[earlydepthstencil]
            float4 frag(VS_OUT In) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID( In );

				//int Offset = (int)UNITY_ACCESS_INSTANCED_PROP( Props, _ObjectIndex );
				int Offset = (int)_InstanceOffset + (int)In.inst;
				int CurrentValue;
				//InterlockedAdd( CounterBuffer[ Offset ], 1, CurrentValue );
				CounterBuffer[ Offset ] = 1;
				
				if( !_DebugBoxes )
					discard;
				
				//For debugging only
				float Alpha = 0.3f;
				return float4( 1, 1, 1, Alpha );				
            }

            ENDCG
        }
	} 
	FallBack "Diffuse"
}
