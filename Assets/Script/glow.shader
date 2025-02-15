Shader "Custom/OutlineShader" {
    Properties{
        _MainTex("Base (RGB)", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth("Outline Width", Range(0.0, 0.05)) = 0.02
    }
        SubShader{
            Tags { "RenderType" = "Opaque" "Queue" = "Overlay" }
            // 剔除正面（只绘制反面）
            Cull Front
            Lighting Off
            ZWrite On
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

            Pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                sampler2D _MainTex;
                fixed4 _OutlineColor;
                float _OutlineWidth;

                struct appdata {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                    float2 uv : TEXCOORD0;
                };

                struct v2f {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                v2f vert(appdata v) {
                    v2f o;
                    // 将顶点沿法线方向膨胀
                    float3 norm = normalize(mul((float3x3)unity_WorldToObject, v.normal));
                    v.vertex.xyz += norm * _OutlineWidth;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target {
                    // 此处可直接返回描边颜色
                    return _OutlineColor;
                }
                ENDCG
            }
        }
            FallBack "Diffuse"
}