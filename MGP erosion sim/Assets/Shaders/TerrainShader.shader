Shader "Custom/TerrainShader"
{
    Properties
    {
        _GrassColor("Grass", Color) = (0,1,0,1)
        _GrassTex("Base texture", 2D) = "white" {}
        _RockColor("Rock", Color) = (0.5, 0.5, 0.5, 1)
        _GrassSlopeThreshold("Grass Slope Threshold", Range(0,1)) = .5
        _GrassBlendAmount("Grass Blend Amount", Range(0,1)) = .5
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        half _MaxHeight;
        half _GrassSlopeThreshold;
        half _GrassBlendAmount;
        sampler2D _GrassTex;
        fixed4 _GrassColor;
        fixed4 _RockColor;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed3 baseColor = tex2D(_GrassTex, IN.uv_MainTex).rgb;
            float slope = 1 - IN.worldNormal.y;
            float grassBlendHeight = _GrassSlopeThreshold * (1 - _GrassBlendAmount);
            float grassWeight = 1 - saturate((slope - grassBlendHeight) / (_GrassSlopeThreshold - grassBlendHeight));
            o.Albedo = _GrassColor * grassWeight + _RockColor * (1 - grassWeight);
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = _GrassColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
