Shader "Unlit/GrassWindShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Height("Height",Range(0,1)) = .012
        _Intensity("Intensity",float) = .012
        _Color("Color",Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color: TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Height;
            float _Intensity;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                if(v.vertex.y>_Height){
                    v.color = _Color;
                    v.vertex.x += sin(_Time.y)*_Intensity*v.vertex.x;
                    v.vertex.z += sin(_Time.y)*_Intensity*pow(v.vertex.y*100,2);
                }
                else{
                    v.color = float4(0,0,0,1);
                }
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return i.color;
            }
            ENDCG
        }
    }
}
