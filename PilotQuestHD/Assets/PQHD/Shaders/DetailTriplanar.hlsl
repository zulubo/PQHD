void DetailTriplanar_float(float3 position, float3 normal, float tiling, out float2 uv, out float3 tangent, out float3 bitangent)
{
    float xn = abs(normal.x);
    float yn = abs(normal.y);
    float zn = abs(normal.z);

    if (xn > yn && xn > zn)
    {
        uv = position.zy * tiling;
        tangent = float3(0,1,0);
        bitangent = float3(0,0,1);
    }
    else if (yn > zn)
    {
        uv = position.xz * tiling;
        tangent = float3(0,0,1);
        bitangent = float3(1,0,0);
    }
    else
    {
        uv = position.xy * tiling;
        tangent = float3(0,1,0);
        bitangent = float3(1,0,0);
    }
}

void DetailTriplanar_half(half3 position, half3 normal, float tiling, out half2 uv, out half2 tangent, out half2 bitangent)
{
    half xn = abs(normal.x);
    half yn = abs(normal.y);
    half zn = abs(normal.z);

    if (xn > yn && xn > zn)
    {
        uv = position.zy * tiling;
        tangent = half3(0,1,0);
        bitangent = half3(0,0,1);
    }
    else if (yn > zn)
    {
        uv = position.xz * tiling;
        tangent = half3(0,0,1);
        bitangent = half3(1,0,0);
    }
    else
    {
        uv = position.xy * tiling;
        tangent = half3(0,1,0);
        bitangent = half3(1,0,0);
    }
}

void HeightBlend_float(float heightA, float heightB, float depth, float threshold, out float blend)
{
    float a2 = threshold;
    float a1 = 1-threshold;

    float ma = max(heightA + a1, heightB + a2) - depth;

    float b1 = max(heightA + a1 - ma, 0);
    float b2 = max(heightB + a2 - ma, 0);

    blend = b2 / (b1 + b2);
}