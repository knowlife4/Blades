using UnityEngine;

namespace Blades.UnityEditor
{
    public class PaintEditMode : EditMode
    {
        public PaintEditMode (GrassManager manager, string name) : base(manager, name) {}
        
        public override void OnGUI()
        {
            Properties.RenderGUI(true);
        }

        public override void OnUse(bool interacting)
        {
            RaycastHit? hit = Brush();
            if(hit is null || !interacting) return;

            Paint(hit.Value.point, hit.Value.normal);

            Type.Reload();
        }

        public void Paint (Vector3 hitPoint, Vector3 hitNormal)
        {
            float density = Properties.Density;
            float scaleFactor = 1 / density;

            if(Type is null) return;

            for (int x = 0; x <= scaleFactor; x++)
            {
                for (int z = 0; z <= scaleFactor; z++)
                {
                    var point = (new Vector3 (x, 0, z) * density) - new Vector3(.5f, 0, .5f);
                    var scatter = new Vector3(Random.Range(-density, density), 0, Random.Range(-density, density));
                    var scatteredPoint = point + scatter;
                    var finalPoint = hitPoint + (scatteredPoint * Properties.BrushSize.y);

                    float distanceFromOuterCircle = DistanceFromCircle(finalPoint, hitPoint, Properties.BrushSize.y);
                    float distanceFromInnerCircle = DistanceFromCircle(finalPoint, hitPoint, Properties.BrushSize.x);
            
                    float ratio = Mathf.Abs((distanceFromInnerCircle / (Properties.BrushSize.y - Properties.BrushSize.x)) - 1);
                    if (distanceFromOuterCircle > 0 || ratio < Random.Range(0f, 1f)) continue;

                    GrassBlade? blade = CreateBlade(Properties, Random.Range(0f, 360f), -hitNormal, finalPoint);
                    if (blade is not null) Type.Collection.Add(blade.Value);
                }
            }

            Type.Reload();
        }
        
        public override void OnUseEnd () 
        {
            Type.Collection.PushUndo();
        }
    }
}