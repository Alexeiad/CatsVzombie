Shader "UI/iOS Glass Minimal"
{
    Properties
    {
        _Color("Color", Color) = (0.92, 0.96, 1.0, 0.12)
        _Radius("Corner Radius", Range(0, 0.5)) = 0.18
        _EdgeSoftness("Edge Softness", Range(0, 0.1)) = 0.015
    }
    
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
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
            
            float4 _Color;
            float _Radius;
            float _EdgeSoftness;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Простая функция скругления
                float2 uvCentered = i.uv - 0.5;
                float2 uvAbs = abs(uvCentered);
                
                float2 cornerDist = max(uvAbs - (0.5 - _Radius), 0.0);
                float dist = length(cornerDist);
                
                // Плавное скругление
                float alpha = 1.0 - smoothstep(_Radius - _EdgeSoftness, _Radius + _EdgeSoftness, dist);
                
                // Добавляем легкий градиент для объема
                float gradient = lerp(0.85, 1.0, i.uv.y);
                
                float4 col = _Color;
                col.a *= alpha * gradient;
                
                // Тонкая белая граница
                if(dist > _Radius - _EdgeSoftness * 2.0 && dist < _Radius)
                {
                    col.rgb = lerp(col.rgb, float3(1,1,1), 0.3);
                }
                
                return col;
            }
            ENDCG
        }
    }
}