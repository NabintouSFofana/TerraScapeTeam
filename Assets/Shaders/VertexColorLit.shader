// Shows a mesh's per-vertex colors with basic diffuse lighting.
// Used by the terrain so it can be tinted grass/rock/sand/snow by height and slope.
Shader "TerraScape/VertexColorLit"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Lambert
        struct Input { float4 color : COLOR; };   // auto-filled with the vertex color
        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = IN.color.rgb;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
