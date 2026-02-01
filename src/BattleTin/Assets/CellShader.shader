Shader "Hidden/CelShadingPostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeThreshold ("Edge Threshold", Range(0, 0.1)) = 0.01
        _EdgeColor ("Edge Color", Color) = (0, 0, 0, 1)
        _ColorBands ("Color Bands", Range(1, 10)) = 3
        _DepthSensitivity ("Depth Sensitivity", Range(0, 1)) = 0.5
        _NormalSensitivity ("Normal Sensitivity", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        
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
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            sampler2D _MainTex;
            sampler2D _CameraDepthNormalsTexture;
            float _EdgeThreshold;
            float4 _EdgeColor;
            float _ColorBands;
            float _DepthSensitivity;
            float _NormalSensitivity;
            
            float CheckSame(float2 centerNormal, float centerDepth, float4 sample)
            {
                float sampleDepth;
                float3 sampleNormal;
                DecodeDepthNormal(sample, sampleDepth, sampleNormal);
                
                float diffDepth = abs(centerDepth - sampleDepth) * _DepthSensitivity;
                float depthCheck = diffDepth > _EdgeThreshold;
                
                float diffNormal = distance(centerNormal, sampleNormal.xy) * _NormalSensitivity;
                float normalCheck = diffNormal > _EdgeThreshold;
                
                return depthCheck || normalCheck;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Основной цвет
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Получаем глубину и нормали из текстуры
                float4 center = tex2D(_CameraDepthNormalsTexture, i.uv);
                float centerDepth;
                float3 centerNormal;
                DecodeDepthNormal(center, centerDepth, centerNormal);
                
                // Определяем соседние пиксели для проверки границ
                float2 texelSize = 1.0 / _ScreenParams.xy;
                
                float4 left = tex2D(_CameraDepthNormalsTexture, i.uv + float2(-texelSize.x, 0));
                float4 right = tex2D(_CameraDepthNormalsTexture, i.uv + float2(texelSize.x, 0));
                float4 top = tex2D(_CameraDepthNormalsTexture, i.uv + float2(0, texelSize.y));
                float4 bottom = tex2D(_CameraDepthNormalsTexture, i.uv + float2(0, -texelSize.y));
                
                // Проверяем границы
                float edge = 0;
                edge += CheckSame(centerNormal.xy, centerDepth, left);
                edge += CheckSame(centerNormal.xy, centerDepth, right);
                edge += CheckSame(centerNormal.xy, centerDepth, top);
                edge += CheckSame(centerNormal.xy, centerDepth, bottom);
                
                // Применяем квантование цвета (cel shading)
                float luminance = dot(col.rgb, float3(0.299, 0.587, 0.114));
                float quantized = floor(luminance * _ColorBands) / _ColorBands;
                float3 celColor = col.rgb * (quantized / max(luminance, 0.001));
                
                // Если обнаружена граница, используем цвет границы
                if (edge > 0.5)
                {
                    celColor = _EdgeColor.rgb;
                }
                
                return fixed4(celColor, col.a);
            }
            ENDCG
        }
    }
}