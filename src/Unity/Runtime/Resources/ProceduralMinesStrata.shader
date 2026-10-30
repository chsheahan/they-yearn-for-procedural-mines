Shader "ProceduralMines/Strata"
{
    Properties
    {
        _Color ("Rock", Color) = (0.37, 0.34, 0.30, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Lambert fullforwardshadows
        #pragma target 3.0
        fixed4 _Color;
        struct Input { float3 worldPos; float3 worldNormal; };
        void surf(Input IN, inout SurfaceOutput o)
        {
            float strata = sin(IN.worldPos.y * 3.6 + sin(IN.worldPos.x * 0.16) + sin(IN.worldPos.z * 0.2));
            float grain = sin(IN.worldPos.x * 7.1) * sin(IN.worldPos.z * 8.3) * sin(IN.worldPos.y * 6.7);
            float floorMask = saturate((IN.worldNormal.y - 0.5) * 3);
            o.Albedo = _Color.rgb * (0.82 + 0.12 * strata + 0.06 * grain);
            o.Albedo = lerp(o.Albedo, float3(0.28, 0.29, 0.27), floorMask * 0.55);
            o.Alpha = 1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
