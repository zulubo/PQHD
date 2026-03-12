void GeometricSpecularAA_float(float SmoothnessIn, float3 Normal, out float SmoothnessOut)
{
    //SmoothnessOut = GeometricNormalFiltering(SmoothnessIn, Normal, 1, 0.5);
    float3 normalDdx = ddx(Normal);
    float3 normalDdy = ddy(Normal);
    float geometricRoughness = pow(saturate(max(dot(normalDdx, normalDdx), dot(normalDdy, normalDdy))), 0.333);
    SmoothnessOut = dot(Normal, Normal) > 1.1 ? SmoothnessIn : min(SmoothnessIn, 1 - geometricRoughness);
}