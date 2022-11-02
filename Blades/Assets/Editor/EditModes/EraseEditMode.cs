using UnityEngine;

namespace Blades.UnityEditor
{
    public class EraseEditMode : EditMode
    {
        public EraseEditMode (BladesManager manager, string name) : base(manager, name) {}

        protected override void OnGUI()
        {
            Properties.RenderBrushGUI();

            if(GUILayout.Button("Erase All?")) EraseAll();
        }
        
        protected override void OnUse(bool interacting)
        {
            RaycastHit? hit = Brush();
            if(hit is null || !interacting) return;
            
            foreach (var blade in Type.Collection.ToArray())
            {
                Erase(blade, hit.Value.point);
            }

            Type.Reload();
        }

        public void Erase (BladesInstance blade, Vector3 hitPoint)
        {
            float distanceFromOuterCircle = DistanceFromCircle(blade.Position, hitPoint, Properties.BrushSize.y);
            if (distanceFromOuterCircle > 0) return;

            float distanceFromInnerCircle = DistanceFromCircle(blade.Position, hitPoint, Properties.BrushSize.x);
            
            float ratio = Mathf.Abs((distanceFromInnerCircle / (Properties.BrushSize.y - Properties.BrushSize.x)) - 1);
            if(ratio < Random.Range(0f, 1f)) return;

            if(Type.Collection is not null) Type.Collection.Remove(blade);
            return;
        }

        public void EraseAll ()
        {
            Type.Collection.Clear();
        }

        protected override void OnUseEnd () 
        {
            Type.Collection.PushUndo();
        }
    }
}