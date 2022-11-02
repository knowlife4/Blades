using Blades.Rendering;
using UnityEditor;
using UnityEngine;

namespace Blades.UnityEditor
{
    public abstract class EditMode
    {
        public EditMode (BladesManager manager, string name) 
        {
            Manager = manager;
            Name = name;
            Properties = new(manager);
        }

        public string Name { get; }
        public BladesManager Manager { get; }
        public BladesProperties Properties { get; }

        public BladesType Type => Manager.BladesTypeCollection.BladesTypes.Length > 0 ? Manager.BladesTypeCollection.BladesTypes[Properties.Type] : null;

        public bool typeValid => Type is not null && Type.Collection is not null;

        public void GUI () { if(typeValid) OnGUI(); }
        protected abstract void OnGUI ();

        public void Use (bool interacting) { if(typeValid) OnUse(interacting); }
        protected abstract void OnUse (bool interacting);

        public void UseStart () { if(typeValid) OnUseStart(); }
        protected virtual void OnUseStart () {}

        public void UseEnd () { if(typeValid) OnUseEnd(); }
        protected virtual void OnUseEnd () {}

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

        protected static float DistanceFromCircle (Vector3 point, Vector3 origin, float radius)
        {
            return (point - origin).magnitude - radius;
        }

        protected BladesInstance? CreateSafeBlade (Collider collider, Vector3 normal, Vector3 position)
        {
            Ray ray = new(position + (normal * .5f), -normal);
            if(!collider.Raycast(ray, out RaycastHit hit, 2f)) return null;

            MeshRenderer renderer = collider.GetComponent<MeshRenderer>();
            if(renderer == null) renderer = collider.transform.parent.GetComponentInChildren<MeshRenderer>();
            if(renderer == null) return null;

            MeshFilter filter = renderer.GetComponent<MeshFilter>();

            Camera cam = SceneView.lastActiveSceneView.camera;

            Vector3 camUp = cam != null ? cam.transform.up : Vector3.up;
            
            Color color = Properties.UseColor ? Properties.Color : ColorCast.Ray(new(ray, filter, renderer), camUp).Color;

            return CreateBlade(normal, Random.Range(0f, 360f), position, color);
        }

        protected BladesInstance CreateBlade (Vector3 direction, float yRotation, Vector3 position, Color color)
        {
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.forward) * Quaternion.Euler(90f, 0, 0);
            rotation *= Quaternion.Euler(0, yRotation, 0);

            return new BladesInstance (position, rotation, color, Properties.UseHeight ? Properties.Height : 1);
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