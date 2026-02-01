Shader "Custom/WallCutout"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CutoutRadius ("Cutout Radius", Float) = 2.0
        _Falloff ("Falloff", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float _CutoutRadius;
            float _Falloff;
            // Essa variável global será preenchida pelo C#
            uniform float3 _VectorPlayerPos; 

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float dist = distance(i.worldPos, _VectorPlayerPos);
                // A mágica: se a distância for menor que o raio, o pixel é descartado
                float alpha = smoothstep(_CutoutRadius, _CutoutRadius + _Falloff, dist);
                
                fixed4 col = tex2D(_MainTex, i.uv);
                if (alpha < 0.5) discard; // "Fura" o objeto
                
                return col;
            }
            ENDCG
        }
    }
}