using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GrassType", menuName = "Blades/GrassType", order = 1)]
public class GrassTypeAsset : ScriptableObject
{
    public Texture2D Texture;
    public Mesh Mesh;
    public Material Material;
}