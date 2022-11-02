using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blades.Rendering
{
    [CreateAssetMenu(fileName = "Blades Type", menuName = "Blades/Blades Type Asset", order = 1)]
    public class BladesTypeAsset : ScriptableObject
    {
        public Texture2D Texture;
        public Mesh Mesh;
        public Material Material;
    }
}