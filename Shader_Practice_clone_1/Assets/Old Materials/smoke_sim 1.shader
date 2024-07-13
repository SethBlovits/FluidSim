Shader "CustomRenderTexture/smoke_sim 1"
{
    Properties
    {
        
        _Dissipation("Dissipation",float) = 1.0
        _Minimum("Minimum",float) = .10
        _S2("PhaseVelocity^2", Range(0.0, 0.5)) = 0.2
        [PowerSlider(0.01)]
        _Atten("Attenuation", Range(0.0, 1.0)) = 0.999
        _DeltaUV("Delta UV", Float) = 3
        }

    CGINCLUDE
    #include "UnityCustomRenderTexture.cginc"

    //float _MainTex;
    struct fieldData
        {
            float2 velocity : TEXCOORD0;
            
        };
    float _Dissipation;
    float _Minimum;
    half _S2;
    half _Atten;
    float _DeltaUV;
    float2 Velocity[1000];
    float4 diffusionNaive(v2f_customrendertexture IN){
        float2 uv = IN.globalTexcoord;// current coordinates that the shader is operating on

        float du = 1.0 / _CustomRenderTextureWidth;//derivative step
        float dv = 1.0 / _CustomRenderTextureHeight;//derivative step
        float3 duv = float3(du, dv, 0) * _DeltaUV; // storing these to step changes along with a zero to swizzle later
        
        float2 c = tex2D(_SelfTexture2D, uv);//c is the current center pixel

        float p = (c.y + _Dissipation*(
        tex2D(_SelfTexture2D, uv - duv.zy).y +//j-1
        tex2D(_SelfTexture2D, uv + duv.zy).y +//j+1
        tex2D(_SelfTexture2D, uv - duv.xz).y +//i-1
        tex2D(_SelfTexture2D, uv + duv.xz).y - //i+1
        4*c.y));
        
        return float4(p, c.r, 0, 0);
    }
    float advectionNaive(float4 current, fieldData vel,v2f_customrendertexture IN){
        float2 uv = IN.globalTexcoord;// current coordinates that the shader is operating on

        float du = 1.0 / _CustomRenderTextureWidth;//derivative step
        float dv = 1.0 / _CustomRenderTextureHeight;//derivative step
        float3 duv = float3(du, dv, 0) * _DeltaUV; // storing these to step changes along with a zero to swizzle later
        
        float2 c = tex2D(_SelfTexture2D, uv);//c is the current center pixel

        //float p = 
    }
    float4 frag(v2f_customrendertexture IN) : SV_Target
    {
        float4 output = diffusionNaive(IN);
        return(output);
    }
    
    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        //you can set up different passes in this segment by calling some of the functions above
        Pass
        {
            Name "idle" //this could be changed to something like update or idle because this should be the normal behaviour
            CGPROGRAM
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            ENDCG
        }
    }
}
