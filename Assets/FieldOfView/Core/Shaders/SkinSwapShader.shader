Shader "Hidden/Field Of View/Skin Swap" {
    Properties {
        [Enum(Color,0,Texture,1)] _SkinType ("Skin Type", Int) = 0

        _PassiveColor ("Passive Color", Color) = (0,1,0,0.25)
        _ActiveColor ("Active Color", Color) = (1,0,0,0.5)

        _PassiveTexture ("Passive Texture", 2D) = ""
        _ActiveTexture ("Active Texture", 2D) = ""

        _GameTime ("Game Time", Float) = 0
        _ExecutionStartTime ("Execution Start Time", Float) = 0
        _FadeInSpeed ("Fade-in Speed", Float) = 0
        _FadeOutSpeed ("Fade-out Speed", Float) = 0
        _SkinsOffset ("Skins Offset", Float) = 0
    }

    SubShader {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Unlit alpha vertex:vert
        #pragma target 2.0
        #pragma multi_compile NONE PASSIVE_MODE ACTIVE_MODE FADE_OUT_MODE FADE_IN_MODE

        half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten) {
            fixed4 color;
            color.rgb = s.Albedo; 
            color.a = s.Alpha;
            return color;
        }

        struct Input {
            float3 worldPos;
            float3 worldOrigin;
                    
            float2 uv_PassiveTexture;
            float2 uv_ActiveTexture;
        };

        void vert (inout appdata_full v, out Input o) {
           UNITY_INITIALIZE_OUTPUT(Input, o);
           
           o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1));
           o.worldOrigin = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
        }

        int _SkinType;      

        fixed4 _PassiveColor;
        fixed4 _ActiveColor;

        sampler2D _PassiveTexture;
        sampler2D _ActiveTexture;

        float _GameTime;
        float _ExecutionStartTime;
        float _FadeInSpeed;
        float _FadeOutSpeed;
        float _SkinsOffset;

        float4 getPassiveSkinPixelColor (Input input) {
            if (_SkinType == 0) {
                return _PassiveColor.rgba;
            }
            else if (_SkinType == 1) {
                return tex2D(_PassiveTexture, input.uv_PassiveTexture.xy).rgba;
            }
            else {
                return float4(0, 0, 0, 0);
            }
        }

        float4 getActiveSkinPixelColor (Input input) {
            if (_SkinType == 0) {
                return _ActiveColor.rgba;
            }
            else if (_SkinType == 1) {
                return tex2D(_ActiveTexture, input.uv_ActiveTexture.xy).rgba;
            }
            else {
                return float4(0, 0, 0, 0);
            }
        }

        void passiveMode (Input input, inout SurfaceOutput output) {
            float4 passiveSkinPixel = getPassiveSkinPixelColor(input);
            output.Albedo = passiveSkinPixel.rgb;
            output.Alpha = passiveSkinPixel.a;
        }

        void activeMode (Input input, inout SurfaceOutput output) {
            float4 activeSkinPixel = getActiveSkinPixelColor(input);
            output.Albedo = activeSkinPixel.rgb;
            output.Alpha = activeSkinPixel.a;
        }

        void fadeOutMode (Input input, inout SurfaceOutput output) {
            if (_SkinsOffset - distance(input.worldPos, input.worldOrigin) < (_GameTime - _ExecutionStartTime) * _FadeOutSpeed) {
                float4 passiveSkinPixel = getPassiveSkinPixelColor(input);
                output.Albedo = passiveSkinPixel.rgb;
                output.Alpha = passiveSkinPixel.a;
            }
            else {
                float4 activeSkinPixel = getActiveSkinPixelColor(input);
                output.Albedo = activeSkinPixel.rgb;
                output.Alpha = activeSkinPixel.a;
            }
        }

        void fadeInMode (Input input, inout SurfaceOutput output) {
            if (distance(input.worldPos, input.worldOrigin) - _SkinsOffset < (_GameTime - _ExecutionStartTime) * _FadeInSpeed) {
                float4 activeSkinPixel = getActiveSkinPixelColor(input);
                output.Albedo = activeSkinPixel.rgb;
                output.Alpha = activeSkinPixel.a;
            }
            else {
                float4 passiveSkinPixel = getPassiveSkinPixelColor(input);
                output.Albedo = passiveSkinPixel.rgb;
                output.Alpha = passiveSkinPixel.a;
            }
        }

        void surf (Input input, inout SurfaceOutput output) {

            #if PASSIVE_MODE
            passiveMode(input, output);
            #endif

            #if ACTIVE_MODE
            activeMode(input, output);
            #endif

            #if FADE_OUT_MODE
            fadeOutMode(input, output);
            #endif

            #if FADE_IN_MODE
            fadeInMode(input, output);
            #endif
        }

        ENDCG
    }

    CustomEditor "FieldOfViewSkinEditor"
}