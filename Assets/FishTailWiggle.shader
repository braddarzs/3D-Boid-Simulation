Shader "Custom/FishTailOnlyURP_VertexColor"
{
    Properties
    {
        // Albedo
        [MainColor]_BaseColor ("Base Color (tint)", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.25
        [MainTexture]_BaseMap ("URP BaseMap", 2D) = "white" {}
        _MainTex ("Legacy MainTex", 2D) = "white" {}
        // 0=BaseMap, 1=MainTex, 2=VertexColor
        [KeywordEnum(BASEMAP, MAINTEX, VERTEXCOLOR)] _ALBEDO_SOURCE ("Albedo Source", Float) = 2

        // Tail-only wiggle controls
        _Amplitude ("Tail Amplitude (m)", Range(0,0.3)) = 0.12
        _Speed     ("Wiggle Speed", Range(0,6)) = 2.0

        // Head->Tail axis and S range (in object space)
        _Axis   ("Forward Axis (XYZ)", Vector) = (0,0,1,0)
        _HeadS  ("Head S (along Axis)", Float) = -0.5
        _TailS  ("Tail S (along Axis)", Float) =  0.5
        _TailStartS ("Tail Start S (along Axis)", Float) =  0.15
        _TailMaskPow ("Tail Mask Power", Range(0.2,4)) = 1.3

        // (Optional) fin flutter is disabled by default; enable by raising amp
        _FinFlutterAmp ("Fin Flutter Amp (m)", Range(0,0.05)) = 0.0
        _FinFlutterFreq("Fin Flutter Freq", Range(0,20)) = 12.0
        // Choose fin mask if you use flutter: 0=None, 1=VC Blue, 2=VC Alpha, 3=UV.y
        [KeywordEnum(NONE, VC_BLUE, VC_ALPHA, UVY)] _FINMASK_SOURCE ("Fin Mask Source", Float) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 150

        // Prevent “holes” when cards tilt
        Cull Off
        ZWrite On
        ZTest LEqual
        AlphaToMask On
        Blend One Zero

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma shader_feature_local _ALBEDO_SOURCE_BASEMAP _ALBEDO_SOURCE_MAINTEX _ALBEDO_SOURCE_VERTEXCOLOR
            #pragma shader_feature_local _FINMASK_SOURCE_VC_BLUE _FINMASK_SOURCE_VC_ALPHA _FINMASK_SOURCE_UVY

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;   // RGB for color; A/B can be masks if you use them
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings {
                float2 uv         : TEXCOORD0;
                float4 positionHCS: SV_POSITION;
                float  fogCoord   : TEXCOORD1;
                float4 vColor     : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor; float _Cutoff;

                float  _Amplitude, _Speed;
                float4 _Axis;        // object-space head->tail direction
                float  _HeadS, _TailS, _TailStartS, _TailMaskPow;

                float  _FinFlutterAmp, _FinFlutterFreq;
            CBUFFER_END

            float4 _BaseMap_ST, _MainTex_ST;

            float4 SampleWithST(TEXTURE2D_PARAM(tex,samp), float2 uv, float4 st) {
                float2 uvst = uv*st.xy + st.zw;
                return SAMPLE_TEXTURE2D(tex,samp,uvst);
            }

            // Map s to 0..1 between TailStartS..TailS, then shape with power
            float TailOnlyMask(float s, float headS, float tailStartS, float tailS, float powK)
            {
                float denom = max(tailS - tailStartS, 1e-5);
                float t = saturate((s - tailStartS) / denom); // 0 at body, 1 at very tail
                return pow(t, powK);
            }

            Varyings vert (Attributes IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                Varyings OUT; UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                float3 p   = IN.positionOS.xyz;
                float3 fwd = normalize(_Axis.xyz + 1e-6);

                // coordinate along fish
                float s = dot(p, fwd);

                // Tail mask that is 0 over the body, ramps up only at the tail section
                float tailMask = TailOnlyMask(s, _HeadS, _TailStartS, _TailS, _TailMaskPow);

                // Build a stable lateral axis perpendicular to fwd
                float3 upGuess = float3(0,1,0);
                if (abs(dot(upGuess, fwd)) > 0.99) upGuess = float3(1,0,0);
                float3 lat = normalize(cross(fwd, upGuess));

                // Time
                float t = _Time.y * _Speed;

                // Tail-only wag: single phase over time (no along-body term)
                float wag = sin(t) * _Amplitude * tailMask;

                // Optional fin flutter (off by default)
                float finMask =
                #if defined(_FINMASK_SOURCE_VC_BLUE)
                    saturate(IN.color.b);
                #elif defined(_FINMASK_SOURCE_VC_ALPHA)
                    saturate(IN.color.a);
                #elif defined(_FINMASK_SOURCE_UVY)
                    saturate(IN.uv.y);
                #else
                    0.0;
                #endif
                float flutter = sin(_FinFlutterFreq * _Time.y + s * 5.0) * _FinFlutterAmp * finMask;

                // Apply displacement
                p += (wag + flutter) * lat;

                // Output
                float3 posWS = TransformObjectToWorld(float4(p,1)).xyz;
                OUT.positionHCS = TransformWorldToHClip(posWS);
                OUT.uv = IN.uv;
                OUT.fogCoord = ComputeFogFactor(OUT.positionHCS.z);
                OUT.vColor = IN.color;
                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                float4 albedo;
                #if defined(_ALBEDO_SOURCE_MAINTEX)
                    albedo = SampleWithST(TEXTURE2D_ARGS(_MainTex, sampler_MainTex), IN.uv, _MainTex_ST);
                #elif defined(_ALBEDO_SOURCE_VERTEXCOLOR)
                    albedo = float4(IN.vColor.rgb, 1.0);
                #else
                    albedo = SampleWithST(TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap), IN.uv, _BaseMap_ST);
                #endif

                albedo *= _BaseColor;
                clip(albedo.a - _Cutoff);
                albedo.rgb = MixFog(albedo.rgb, IN.fogCoord);
                return albedo;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
