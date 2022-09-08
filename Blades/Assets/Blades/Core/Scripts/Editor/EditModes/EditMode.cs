using Blades.Rendering;
using UnityEditor;
using UnityEngine;

namespace Blades.UnityEditor
{
    public abstract class EditMode
    {
        public EditMode (GrassManager manager, string name) 
        {
            Manager = manager;
            Name = name;
            Properties = new(manager);
        }

        public string Name { get; }
        public GrassManager Manager { get; }
        public GrassProperties Properties { get; }

        public GrassType Type => Manager.TypeCollection.GrassTypes[Properties.Type];

        public abstract void OnGUI ();
        public abstract void OnUse (bool interacting);

        public virtual void OnUseStart () {}

        public virtual void OnUseEnd () {}

        public RaycastHit? Brush ()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit)) return null;

            if(Vector3.Angle(Vector3.up, hit.normal) > Properties.NormalLimit) return null;

            Handles.color = Color.red;
            Handles.DrawWireDisc(hit.point, hit.normal, Properties.BrushSize.x);

            Handles.color = Color.white;
            Handles.DrawWireDisc(hit.point, hit.normal, Properties.BrushSize.y);

            return hit;
        }

        protected float DistanceFromCircle (Vector3 point, Vector3 origin, float radius)
        {
            return (point - origin).magnitude - radius;
        }

        protected GrassBlade? CreateBlade (GrassProperties properties, float yRotation, Vector3 direction, Vector3 position, Collider collider = null) 
        {
            Color color = properties.Color;

            Ray placementRay = new(position -direction, direction);

            RaycastHit raycastHit;
            bool raycast = collider is null ? Physics.Raycast(placementRay, out raycastHit) : collider.Raycast(placementRay, out raycastHit, 10f);

            if (!raycast) return null;

            if(!properties.UseColor)
            {
                Transform root = raycastHit.collider.transform.root;
                Renderer renderer = root.GetComponentInChildren<Renderer>();

                Texture2D texture2D = renderer.sharedMaterial.mainTexture as Texture2D;

                if(texture2D is not null)
                {
                    Vector2 pCoord = raycastHit.textureCoord * new Vector2(texture2D.width, texture2D.height);
                    Vector2 tiling = renderer.sharedMaterial.mainTextureScale;
                    
                    color = texture2D.GetPixel(Mathf.FloorToInt(pCoord.x * tiling.x) , Mathf.FloorToInt(pCoord.y * tiling.y));
                }
            }

            // TODO Add normal orientation!

            Quaternion rotation = Quaternion.LookRotation(raycastHit.normal, Vector3.forward) * Quaternion.Euler(90f, 0, 0);
            rotation *= Quaternion.Euler(0, yRotation, 0);

            return new GrassBlade (raycastHit.point, rotation, color, properties.UseHeight ? properties.Height : 1);
        }

        public void AddBrushSize (float scale)
        {
            Vector2 brushSize = Properties.BrushSize;

            float min = Properties.BrushSizeMinMax.x;
            float max = Properties.BrushSizeMinMax.y;

            Properties.BrushSize = new Vector2(brushSize.x, Mathf.Clamp(brushSize.y + scale, min, max));
        }

        public void AddBrushHardness (float scale)
        {
            Vector2 brushSize = Properties.BrushSize;

            float min = Properties.BrushSizeMinMax.x;
            float max = Properties.BrushSizeMinMax.y;

            Properties.BrushSize = new Vector2(Mathf.Clamp(brushSize.x + scale, min, max), brushSize.y);
        }
    }
}