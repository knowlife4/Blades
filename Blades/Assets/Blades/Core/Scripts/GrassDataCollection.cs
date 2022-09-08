using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GrassData", menuName = "Blades/GrassDataAsset", order = 1)]
public class GrassDataCollection : ScriptableObject
{
    [SerializeField] GrassBlade[] blades;
    HashSet<GrassBlade> tempBlades = new();

    public int Count => tempBlades.Count;

    void OnEnable ()
    {
        tempBlades = new(blades);
    }

    void OnDisable ()
    {
        blades = ToArray();
    }

    public void Add (GrassBlade blade)
    {
        if(!tempBlades.Contains(blade)) tempBlades.Add(blade);
    }

    public void Remove (GrassBlade blade)
    {
        if(tempBlades.Contains(blade)) tempBlades.Remove(blade);
    }

    public GrassBlade[] ToArray() 
    {
        GrassBlade[] bladesArray = new GrassBlade[tempBlades.Count];
        tempBlades.CopyTo(bladesArray);

        return bladesArray;
    }

    public void Replace (GrassBlade oldBlade, GrassBlade newBlade)
    {
        Remove(oldBlade);
        Add(newBlade);
    }

    public void Clear ()
    {
        tempBlades.Clear();
    }
}

[System.Serializable]
public struct GrassBlade
{
    public GrassBlade (Vector3 position, Quaternion rotation, Color color, float height = 1) : this(position, Matrix4x4.Rotate(rotation), new Vector3(color.r, color.g, color.b), height) {}

    public GrassBlade (Vector3 position, Matrix4x4 rotation, Vector3 color, float height = 1)
    {
        Position = position;
        Height = height;
        Color = color;
        Rotation = rotation;
    }

    public Vector3 Position;
    public Matrix4x4 Rotation;
    public float Height;
    public Vector3 Color;

    public static int Size =>  (/*Color*/sizeof(float) * 3) + (/*Position*/sizeof(float) * 3) + (/*Rotation*/sizeof(float) * 16) + (/*Height*/sizeof(float));
}

