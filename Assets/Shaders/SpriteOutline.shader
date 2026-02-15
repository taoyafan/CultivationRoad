Shader "Custom/SpriteOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,1,0,1)
        _OutlineWidth ("Outline Width (pixels)", Range(1, 20)) = 3
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off Lighting Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
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
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            [loop]
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texCol = tex2D(_MainTex, i.uv);
                texCol.rgb *= _Color.rgb;
                texCol.a *= _Color.a;

                float2 ts = _MainTex_TexelSize.xy;
                float width = _OutlineWidth;

                float centerAlpha = texCol.a;

                if (centerAlpha > 0.5)
                {
                    return texCol;
                }

                float minDist = width + 1;
                float2 uv = i.uv;

                for (float d = 1; d <= 20; d += 1)
                {
                    float offsetX = ts.x * d;
                    float offsetY = ts.y * d;

                    float aR = tex2D(_MainTex, uv + float2(offsetX, 0)).a;
                    float aL = tex2D(_MainTex, uv - float2(offsetX, 0)).a;
                    float aU = tex2D(_MainTex, uv + float2(0, offsetY)).a;
                    float aD = tex2D(_MainTex, uv - float2(0, offsetY)).a;

                    float aRU = tex2D(_MainTex, uv + float2(offsetX, offsetY)).a;
                    float aRD = tex2D(_MainTex, uv + float2(offsetX, -offsetY)).a;
                    float aLU = tex2D(_MainTex, uv + float2(-offsetX, offsetY)).a;
                    float aLD = tex2D(_MainTex, uv + float2(-offsetX, -offsetY)).a;

                    if (aR > 0.5 || aL > 0.5 || aU > 0.5 || aD > 0.5 ||
                        aRU > 0.5 || aRD > 0.5 || aLU > 0.5 || aLD > 0.5)
                    {
                        minDist = d;
                        break;
                    }
                }

                fixed4 finalCol = texCol;

                if (minDist <= width)
                {
                    float fadeFactor = 1.0 - saturate(minDist / width);
                    finalCol.rgb = lerp(texCol.rgb, _OutlineColor.rgb, fadeFactor);
                    finalCol.a = max(texCol.a, _OutlineColor.a * fadeFactor);
                }

                return finalCol;
            }
            ENDCG
        }
    }
}
