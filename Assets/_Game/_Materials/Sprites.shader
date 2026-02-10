Shader "Custom/Sprite Lit Metallic"
{
    Properties
    {
        [MainTexture] _MainTex("Diffuse", 2D) = "white" {}
        [MainColor] _Color("Color", Color) = (1,1,1,1)
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        
        // Masking
        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
        _ColorMask("Color Mask", Float) = 15
        
        // Blending
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest[unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask[_ColorMask]

        Pass
        {
            Name "Universal Forward"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma target 2.0
            
            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #pragma multi_compile_fog
            
            // Lighting keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:SetupSpriteRendererColor
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float3 normalOS     : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float3 positionWS   : TEXCOORD1;
                float3 normalWS     : TEXCOORD2;
                float fogCoord      : TEXCOORD3;
                
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    float4 shadowCoord : TEXCOORD4;
                #endif
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Metallic;
                half _Smoothness;
            CBUFFER_END
            
            // Функция для получения теневых координат
            float4 GetShadowCoord(float3 positionWS, float4 positionCS)
            {
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    return ComputeScreenPos(positionCS);
                #else
                    return TransformWorldToShadowCoord(positionWS);
                #endif
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                // Transform position
                float3 positionWS = TransformObjectToWorld(input.positionOS);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS;
                
                // Normals
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.normalWS = normalize(output.normalWS);
                
                // UVs and color
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                
                // Fog
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                
                // Shadows
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    output.shadowCoord = GetShadowCoord(positionWS, output.positionCS);
                #endif
                
                #ifdef PIXELSNAP_ON
                    output.positionCS = UnityPixelSnap(output.positionCS);
                #endif
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                // Sample texture
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 color = texColor * input.color;
                
                // Alpha clip
                #if defined(UNITY_UI_ALPHACLIP)
                    clip(color.a - 0.001);
                #endif
                
                // Основной свет
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    float4 shadowCoord = input.shadowCoord;
                #else
                    float4 shadowCoord = GetShadowCoord(input.positionWS, input.positionCS);
                #endif
                
                Light mainLight = GetMainLight(shadowCoord);
                
                // Вычисляем освещение
                half3 albedo = color.rgb;
                half3 normalWS = normalize(input.normalWS);
                
                // Диффузное освещение
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = albedo * mainLight.color * NdotL * mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                
                // Спекулярное освещение (металличность)
                half3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                half3 reflectDir = reflect(-mainLight.direction, normalWS);
                half3 halfwayDir = normalize(mainLight.direction + viewDirWS);
                
                half NdotH = saturate(dot(normalWS, halfwayDir));
                half NdotV = saturate(dot(normalWS, viewDirWS));
                
                // Fresnel для металлов
                half3 F0 = lerp(half3(0.04, 0.04, 0.04), albedo, _Metallic);
                half3 fresnel = F0 + (1.0 - F0) * pow(1.0 - NdotV, 5.0);
                
                // Спекулярный член (упрощенный Blinn-Phong)
                half specularPower = exp2(10.0 * _Smoothness + 1.0);
                half3 specular = pow(NdotH, specularPower) * fresnel * _Metallic * mainLight.color;
                
                // Окружающее освещение
                half3 ambient = SampleSH(normalWS) * albedo;
                
                // Дополнительные источники света
                half3 additionalLights = half3(0, 0, 0);
                #if defined(_ADDITIONAL_LIGHTS)
                    uint pixelLightCount = GetAdditionalLightsCount();
                    for (uint lightIndex = 0; lightIndex < pixelLightCount; ++lightIndex)
                    {
                        Light light = GetAdditionalLight(lightIndex, input.positionWS);
                        half3 lightColor = light.color * light.distanceAttenuation * light.shadowAttenuation;
                        
                        half NdotLAdd = saturate(dot(normalWS, light.direction));
                        additionalLights += albedo * lightColor * NdotLAdd * (1.0 - _Metallic);
                        
                        // Спекуляр для дополнительных источников
                        half3 halfwayDirAdd = normalize(light.direction + viewDirWS);
                        half NdotHAdd = saturate(dot(normalWS, halfwayDirAdd));
                        half3 specularAdd = pow(NdotHAdd, specularPower) * fresnel * _Metallic * lightColor;
                        additionalLights += specularAdd;
                    }
                #endif
                
                // Комбинируем все освещение
                half3 finalColor = ambient + diffuse + specular + additionalLights;
                
                // Применяем туман
                finalColor = MixFog(finalColor, input.fogCoord);
                
                return half4(finalColor, color.a);
            }
            
            ENDHLSL
        }
        
        // Проход для отбрасывания теней (обязателен!)
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma target 2.0
            
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float3 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                float3 positionWS = TransformObjectToWorld(input.positionOS);
                output.positionCS = TransformWorldToHClip(positionWS);
                
                // Shadow bias
                #if UNITY_REVERSED_Z
                    output.positionCS.z += 0.0001;
                #else
                    output.positionCS.z -= 0.0001;
                #endif
                
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                #ifdef PIXELSNAP_ON
                    output.positionCS = UnityPixelSnap(output.positionCS);
                #endif
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                // Простая проверка альфы для теней
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                clip(color.a - 0.5);
                
                return 0;
            }
            
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/2D/Sprite-Lit-Default"
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.SpriteLitShader"
}