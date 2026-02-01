Shader "Custom/WallCutout_Tiling"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        // Criamos essa propriedade para controlar o Tiling e Offset no Inspector
        [Header(Surface Input)]
        _TilingX ("Tiling X", Float) = 1.0
        
        [Header(Cutout Settings)]
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
            float _TilingX; // Declaramos a variável aqui para o código usar
            float _CutoutRadius;
            float _Falloff;
            uniform float3 _VectorPlayerPos; 

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // Aplicamos o Tiling X apenas na coordenada U (x) do UV
                o.uv = v.uv;
                o.uv.x *= _TilingX; 
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float dist = distance(i.worldPos, _VectorPlayerPos);
                float alpha = smoothstep(_CutoutRadius, _CutoutRadius + _Falloff, dist);
                
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Se a distância for menor que o raio (alpha baixo), "fura" o pixel
                if (alpha < 0.5) discard; 
                
                return col;
            }
            ENDCG
        }
    }
}