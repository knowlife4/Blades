using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GrassData", menuName = "Blades/GrassDataAsset", order = 1)]
public class GrassDataCollection : ScriptableObject
{
    public GrassBlade[] blades;
}

[System.Serializable]
public struct GrassBlade
{
    public GrassBlade (Vector3 position, float rotation, Color color, float height = 1)
    {
        Position = position;
        Height = height;
        Color = new Vector3(color.r, color.g, color.b);
        Rotation = rotation;
    }

    public Vector3 Position;
    public float Rotation;
    public float Height;
    public Vector3 Color;

    public static int Size =>  (/*Color*/sizeof(float) * 3) + (/*Position*/sizeof(float) * 3) + (/*Rotation*/sizeof(float)) + (/*Height*/sizeof(float));
}

