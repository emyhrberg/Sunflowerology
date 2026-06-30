sampler uImage0 : register(s0);
float uTime;
float uProgress;
float uOpacity;
float2 uScreenResolution;
float2 uWaveCentre;


float3 uWaveParams;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float CurrentTime = uProgress;

    float ratio = uScreenResolution.y / uScreenResolution.x;

    // WaveCentre is provided in normalized screen-space (0..1) to match ScreenShaderData coords.
    float2 center = uWaveCentre.xy / uScreenResolution.xy;
    center.y *= ratio;

    float2 warpedCoord = coords;
    warpedCoord.y *= ratio;
    float Dist = distance(warpedCoord, center)*15;

    // Always sample the texture in original UV space. Scaling UVs here causes
    // the whole scene to look zoomed/cropped.
    float2 sampleCoord = coords;
    float4 color = tex2D(uImage0, sampleCoord);

    if ((Dist <= (CurrentTime + uWaveParams.z)) &&
        (Dist >= (CurrentTime - uWaveParams.z)))
    {
        float Diff = (Dist - CurrentTime);
        float ScaleDiff = (1.0 - pow(abs(Diff * uWaveParams.x), uWaveParams.y));
        float DiffTime = (Diff * ScaleDiff);

        float2 DiffTexCoord = normalize(warpedCoord - center);

        float denom = max(CurrentTime * Dist * 40.0, 0.001);
        sampleCoord += ((DiffTexCoord * DiffTime) / denom);
        color = tex2D(uImage0, sampleCoord) * uOpacity + color * (1.0 - uOpacity);

        color += (color * ScaleDiff) / denom * uOpacity;
    }

    return color;
}

technique Technique1
{
    pass ShockwavePass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
