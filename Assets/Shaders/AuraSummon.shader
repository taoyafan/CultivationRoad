Shader "Custom/AuraSummon"
{

    Properties
    {
        _TintColor ("颜色", Color) = (0, 0.6, 1, 1)
        _Speed ("基础汇聚速度", Float) = 4.0
        _Progress ("汇聚进度", Range(0, 1)) = 0.0
        _Intensity ("亮度倍率", Float) = 1.0
        
        _BaseCoreSize ("光核基础大小", Range(0, 0.5)) = 0.05
        _CoreGrowth ("光核随进度增长量", Range(0, 0.5)) = 0.2
        
        _BaseAuraSize ("旋涡范围(固定)", Range(0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend One One 
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _TintColor;
            float _Speed, _Progress, _Intensity;
            float _BaseCoreSize, _CoreGrowth, _BaseAuraSize;

            float hash(float2 p) {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float noise(float2 p) {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 uv = i.uv - 0.5;
                float dist = length(uv);
                float angle = atan2(uv.y, uv.x);

                float angleSin = sin(angle);
                float angleCos = cos(angle);

                // --- 1. 汇聚逻辑：速度随进度提升 ---
                float currentSpeed = _Speed * (1.0 + _Progress * 2.0);
                float radialPos = dist * 5.0 + _Time.y * currentSpeed; 
                
                // 团块噪声
                float n1 = noise(float2(radialPos, angleSin * 1.5 + angleCos * 1.5));
                float n2 = noise(float2(dist * 10.0 + _Time.y * currentSpeed * 0.5, angleCos * 3.0));
                float clumpyNoise = pow(n1 * n2, 2.0) * 4.0;

                // --- 2. 固定旋涡范围 ---
                // 使用 smoothstep 锁定范围，不受 _Progress 影响
                float auraMask = smoothstep(_BaseAuraSize, _BaseAuraSize * 0.4, dist);
                
                fixed4 col = _TintColor * clumpyNoise * auraMask;
                
                // --- 3. 光核逻辑 ---
                // 初始大小 + (进度 * 增长量)
                float currentCoreSize = _BaseCoreSize + (_Progress * _CoreGrowth);
                float core = smoothstep(currentCoreSize, 0.0, dist);
                
                // 强化中心光核亮度
                col += _TintColor * core * 5.0 * (_Progress + 0.2);

                return col * _Intensity;
            }
            ENDCG
        }
    }
}