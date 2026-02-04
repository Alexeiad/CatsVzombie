Shader "Custom/RoundedEdgesSmooth"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Radius("Corner Radius", Range(0, 0.5)) = 0.1
        _Feather("Feather", Range(0, 0.1)) = 0.02
        _BackgroundColor("Background Color", Color) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

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
            float _Radius;
            float _Feather;
            float4 _BackgroundColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float roundedBox(float2 uv, float2 center, float2 size, float radius)
            {
                uv -= center;
                uv = abs(uv);
                
                // Если мы внутри прямоугольника без учета скруглений
                if (uv.x < size.x - radius && uv.y < size.y - radius)
                    return 1.0;
                
                // Если мы за пределами прямоугольника
                if (uv.x > size.x || uv.y > size.y)
                    return 0.0;
                
                // Вычисляем расстояние до скругленного угла
                float2 q = uv - float2(size.x - radius, size.y - radius);
                q = max(q, 0);
                float dist = length(q);
                
                // Плавное сглаживание
                return smoothstep(radius + _Feather, radius - _Feather, dist);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Центрируем UV координаты
                float2 uv = i.uv - 0.5;
                
                // Маска скругленного прямоугольника
                float mask = roundedBox(uv, float2(0, 0), float2(0.5, 0.5), _Radius);
                
                // Получаем цвет текстуры
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Применяем маску
                col.a *= mask;
                
                // Смешиваем с фоновым цветом
                col.rgb = lerp(_BackgroundColor.rgb, col.rgb, col.a);
                
                return col;
            }
            ENDCG
        }
    }
}