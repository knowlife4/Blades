struct GrassBlade
{
    float3 Position;
    float Rotation;
    float Height;
    float3 Color;
};

StructuredBuffer<GrassBlade> _bladeBuffer; 
StructuredBuffer<float3> _interactionBuffer;

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

float3 _bladePosition;
float _bladeHeight;
float3 _bladeColor;
float _bladeRotation;
int _interactorCount;

#if UNITY_ANY_INSTANCING_ENABLED

    void ConfigureProcedural()
    {
        _bladePosition = _bladeBuffer[unity_InstanceID].Position;
        _bladeHeight = _bladeBuffer[unity_InstanceID].Height;
        _bladeColor = _bladeBuffer[unity_InstanceID].Color;
        _bladeRotation = _bladeBuffer[unity_InstanceID].Rotation;
    }

#endif

void InteractionForce_half (float strength, float pushRadius, float3 wsPos, out float3 Force)
{
    float3 force = float3(0, 0, 0);
    for(uint i = 0; i < _interactorCount; i++)
    {
        uint3 id2 = uint3(i,0,0);
        float3 position = _interactionBuffer[id2.x];
        float3 dis = distance(wsPos, position);
        if(length(dis) > pushRadius * 3) continue;
        
        float radius = 1 - saturate(dis / pushRadius);
        float3 sphereDisp = (wsPos - position) * radius;

        sphereDisp = clamp(sphereDisp * strength, -0.8, 0.8);

        force += sphereDisp;
    }

    Force = force;
}

void InstancePosition_half (out float3 position)
{
    position = _bladePosition;
}

void InstanceRotation_half (out float rotation)
{
    rotation = _bladeRotation;
}

void InstanceHeight_half (out float height)
{
    height = _bladeHeight;
}

void InstanceColor_half (out float3 color)
{
    color = _bladeColor;
}

void Instancing_float (float3 Position, out float3 Out)
{
	Out = Position;
}