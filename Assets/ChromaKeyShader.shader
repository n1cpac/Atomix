Shader "Custom/ChromaReplaceGreenWithBlack"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ChromaColor ("Chroma Key Color", Color) = (0,1,0,1)
        _Tolerance ("Tolerance", Range(0,1)) = 0.3
    }
    SubShader
    {
        Tags {"Queue"="Transparent"}
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
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
            float4 _ChromaColor;
            float _Tolerance;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float diff = distance(col.rgb, _ChromaColor.rgb);

                if (diff < _Tolerance)
                {
                    col.rgb = float3(0.0, 0.0, 0.0); // reemplaza por negro
                }

                return col;
            }
            ENDCG
        }
    }
}
