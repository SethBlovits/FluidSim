Shader "Custom/WaterSurfaceShader"
{
    Properties
    {
        _Color ("Colour", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
 
        CGPROGRAM
        #pragma target 3.0
 
        #include "UnityCG.cginc"      
 
        //Physically based Standard lighting model, and enable shadows on all light types.
        #pragma surface surf Standard fullforwardshadows addshadow
        //Vertex needed to generate curve effect.
        #pragma vertex vert
 
        float3 _BendAmount;
        float3 _BendOrigin;
        float _BendFallOff;
        float _BendFallOffStr;
 
        /*void vert (inout appdata_full v)
        {
            //Getting world space location of a particular vertex.
            float4 world = mul(unity_ObjectToWorld, v.vertex);
            //Calculating the distance between the vertex position and where the _BendOrigin is.
            
            float4 vertex = mul(unity_WorldToObject, world);
 
            v.vertex = vertex;
        }*/
 
        sampler2D _MainTex;
 
        struct Input
        {
            float2 uv_MainTex;
        };
 
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
 
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by colour
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
 }
   

