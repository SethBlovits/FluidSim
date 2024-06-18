Shader "Unlit/WaterSylizedShader"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _DepthGradientShallow("Depth Gradient Shallow", Color) = (0.325, 0.807, 0.971, 0.725)
        _DepthGradientDeep("Depth Gradient Deep", Color) = (0.086, 0.407, 1, 0.749)
        _DepthMaxDistance("Depth Maximum Distance", Float) = 1
        _SurfaceNoise("Surface Noise",2D)  = "white"{}
        _SurfaceNoiseCutoff("Surface Noise Cutoff", Range(0, 1)) = 0.777
        _FoamDistance("Foam Distance", Float) = 0.4
        _SurfaceNoiseScroll("Surface Noise Scroll Amount",Vector) = (0.03,0.03,0,0)
        _SurfaceDistortion("Surface Distortion",2D) = "white"{}
        _SurfaceDistortionAmount("Surface Distortion Amount" , Range(0,1)) = 0.27
        _SimTex ("WaterSimTexture",2D) = "white"{}
        _FoamAlpha("Foam Alpha" , float) = 1
        _VertexCutoff("Vertex Cutoff" , Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Zwrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            float4 alphaBlend(float4 top, float4 bottom){
                float3 color = (top.rgb * top.a) + (bottom.rgb * (1-top.a));
                float alpha = top.a + bottom.a * (1-top.a);

                return float4(color,alpha);
            }
            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 color:COLOR;
            };

            struct v2f
            {
                float2 noiseUV : TEXCOORD0;
                float2 distortUV : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 screenPosition : TEXCOORD2;
                float4 color : TEXCOORD3;
            };

            //sampler2D _MainTex;
            //float4 _MainTex_ST;

            sampler2D _SimTex;

            sampler2D _SurfaceNoise;
            float4 _SurfaceNoise_ST;

            sampler2D _SurfaceDistortion;
            float4 _SurfaceDistortion_ST;

            float _SurfaceDistortionAmount;

            float4 _DepthGradientShallow;
            float4 _DepthGradientDeep;
            float _DepthMaxDistance;
            float _SurfaceNoiseCutoff;
            float2 _SurfaceNoiseScroll;
            float _FoamDistance;
            float _FoamAlpha;
            float _VertexCutoff;
            sampler2D _CameraDepthTexture;

            v2f vert (appdata v)
            {
                v2f o;
                fixed4 water = tex2Dlod(_SimTex,float4(v.uv));
                v.vertex.z += water.r*.0003;
                if(v.vertex.z>_VertexCutoff){
                    v.color = float4(1,1,1,_FoamAlpha);
                }
                else{
                    v.color = float4(0,0,0,0);
                }
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPosition = ComputeScreenPos(o.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.noiseUV = TRANSFORM_TEX(v.uv,_SurfaceNoise);
                o.distortUV = TRANSFORM_TEX(v.uv, _SurfaceDistortion);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float existingDepth01 = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPosition)).r;
                float existingDepthLinear = LinearEyeDepth(existingDepth01);
                //fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                float depthDifference = existingDepthLinear - i.screenPosition.w;
                float waterDepthDifference01 = saturate(depthDifference/_DepthMaxDistance);
                float4 waterColor = lerp(_DepthGradientShallow,_DepthGradientDeep,waterDepthDifference01);

                float foamDepthDifference01 = saturate(depthDifference /_FoamDistance);
                float surfaceNoiseCutoff = foamDepthDifference01 * _SurfaceNoiseCutoff;
                
                float2 distortSample = (tex2D(_SurfaceDistortion, i.distortUV).xy * 2 - 1) * _SurfaceDistortionAmount;
                float2 noiseUV = float2((i.noiseUV.x + _Time.y * _SurfaceNoiseScroll.x)+distortSample.x,(i.noiseUV.y + _Time.y*_SurfaceNoiseScroll.y)+distortSample.y);
                
                float surfaceNoiseSample = tex2D(_SurfaceNoise,noiseUV).r;
                float surfaceNoise = surfaceNoiseSample > surfaceNoiseCutoff ? 1 : 0;

                float4 finalColor = alphaBlend(surfaceNoise,waterColor);
                return alphaBlend(i.color,finalColor);
                //return col;
            }
            ENDCG
        }
    }
}
