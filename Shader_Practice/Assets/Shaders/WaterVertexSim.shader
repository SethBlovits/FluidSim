Shader "Unlit/WaterVertexSim"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SimTex ("WaterSimTexture",2D) = "white"{}
        _Gloss ("Gloss", Range(0,1)) = 1
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"
                "LightMode" = "UniversalForward" }
        LOD 100

        Pass
        {
            

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 wPos : TEXCOORD2;
                //float4 tangent : TEXCOORD3;
                
            };

            sampler2D _MainTex;
            sampler2D _SimTex;
            float4 _MainTex_ST;
            float _Gloss;
            float4 _Color;
            v2f vert (appdata v)
            {   
                fixed4 water = tex2Dlod(_SimTex,float4(v.uv,0,0));
                v2f o;

                v.vertex.z += water.r*.0003;
                v.vertex.z += cos((v.uv.y - _Time.y *.1)* 6.283*2 )*.0001;
                v.vertex.z += cos((v.uv.x - _Time.y *.1)* 6.283*2 )*.0001;
                // Calculate the tangent space matrix
                float3 posPlusTangent = v.vertex.xyz + v.tangent.xyz * 0.01;
                float3 bitangent = cross(v.normal, v.tangent.xyz);
                float3 posPlusBitangent = v.vertex.xyz + bitangent * 0.01;

                // Sample water simulation texture for tangent and bitangent perturbation
                fixed4 waterTangent = tex2Dlod(_SimTex, float4(v.uv + float2(0.01, 0), 0, 0));
                fixed4 waterBitangent = tex2Dlod(_SimTex, float4(v.uv + float2(0, 0.01), 0, 0));

                posPlusTangent.z = waterTangent.r * 0.001;
                posPlusBitangent.z = waterBitangent.r * 0.001;

                // Recalculate tangent, bitangent, and normal vectors
                float3 modifiedTangent = posPlusTangent - v.vertex.xyz;
                float3 modifiedBitangent = posPlusBitangent - v.vertex.xyz;
                float3 modifiedNormal = cross(modifiedTangent, modifiedBitangent);

                v.normal = normalize(modifiedNormal);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.wPos = mul(unity_ObjectToWorld, v.vertex);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {   
                float3 N = normalize(i.normal);
                float L = _WorldSpaceLightPos0.xyz;
                float3 lambert = saturate(dot(N,L));
                float3 diffuseLight = lambert *_LightColor0.xyz;
                float3 V = normalize(_WorldSpaceCameraPos - i.wPos);
                //float3 R = reflect(-L, N);
                float3 H = normalize(L+V);
                float3 specularLight = saturate(dot(H,N)) * (lambert > 0);
                float specularExponent = exp2(_Gloss*6)+1;
                specularLight = pow(specularLight,specularExponent);
                specularLight *= _LightColor0.xyz;
                //float4 light = saturate(diffuseLight.xxx,1);
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                float fresnel = (1-dot(V,N));
                if(i.normal.x>0 || i.normal.z>0){
                    //return float4(1,1,1,1);
                    return float4((i.normal.x*5+(diffuseLight * _Color*5 + specularLight)),1);
                }
                //return float4(((diffuseLight * _Color + specularLight)),1);
                else{
                    return float4(((diffuseLight * _Color*5 + specularLight)),1);
                }
                //return float4(i.normal.x,0,i.normal.z,1);
            }
            ENDCG
        }
    }
}
