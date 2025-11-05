Shader "Custom/SimpleWater"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _WaveSpeed ("Wave Speed", Float) = 1
        _WaveScale ("Wave Scale", Float) = 0.1
        _WaterColor ("Water Color", Color) = (0,0.5,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _WaveSpeed;
            float _WaveScale;
            float4 _WaterColor;

            v2f vert (appdata v)
            {
                v2f o;
                // Use Unity's built-in _Time.y for time in seconds
                float wave = sin((v.vertex.x + _Time.y * _WaveSpeed)) * _WaveScale;
                v.vertex.y += wave;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb *= _WaterColor.rgb; // Tint with water color
                col.a = 0.7; // semi-transparent
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}