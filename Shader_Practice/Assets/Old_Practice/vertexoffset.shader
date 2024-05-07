Shader "Unlit/vertexoffset"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _Color("Color",Color) = (1,1,1,1)
        _Scale("UV Scale",Float) = 1
        _Offset("UV Offset", Float) = 0 
        _ColorA("ColorA" , Color) = (1,1,1,1)
        _ColorB("ColorB" , Color) = (1,1,1,1)
        _ColorStart("Color Start",Range(0,1)) = 1
        _ColorEnd("Color End", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent"
                "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Cull Off
            ZWrite Off
            Blend One One // additive rendering


            //Blend DstColor Zero //multiplicative rendering 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #define TAU 6.28318

            #include "UnityCG.cginc"
            float4 _Color;
            float4 _ColorA;
            float4 _ColorB;
            float _Scale;
            float _Offset;
            float _ColorStart;
            float _ColorEnd;
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normals : NORMAL;
                float4 uv0 : TEXCOORD0;
                //float2 uv : TEXCOORD0;
                
            };

            struct v2f
            {
                //float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            //sampler2D _MainTex;
            //float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normals);
                o.uv = v.uv0;//(v.uv0 + _Offset) * _Scale;
                //o.uv = TRANSFORM_TEX(v.uv, _Color);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            float InverseLerp(float a, float b, float v){
                return(v-a)/(b-a);
            }
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);

                //float4 outColor  = lerp(_ColorA,_ColorB, i.uv.x); // blending two colors based on x uv coordinate 0-1 
                //return outColor;
                //float t = saturate(InverseLerp(_ColorStart,_ColorEnd, i.uv.x));
                
                float xOffset = cos(i.uv.x * TAU * 8) *0.01;
                float t = cos((i.uv.y + xOffset - _Time.x) * TAU * 5) * 0.7 + 0.5;
                t*=1-i.uv.y;

                //remove top and bottom faces
                t*= abs(i.normal.y)<.999;
                //t = frac(t);
                return t;
                //float4 outColor  = lerp(_ColorA,_ColorB, t); // blending two colors based on x uv coordinate 0-1 
                //return outColor;
                //return i.uv.x;
            }
            ENDCG
        }
    }
}
