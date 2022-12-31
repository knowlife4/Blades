using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Blades.UnityEditor
{
    public class FloodEditMode : EditMode
    {
        public FloodEditMode (BladesManager manager, string name) : base(manager, name) {}
        protected override void OnGUI()
        {
            Properties.RenderGUI(true);

            GUILayout.Space(10);

            if(GUILayout.Button("Flood Fill?")) Flood();
        }
        
        public void Flood ()
        {
            Collider[] allColliders = Selection.gameObjects.Select(x => x.GetComponentInChildren<Collider>()).ToArray();

            float density = Properties.Density;

            foreach (var collider in allColliders)
            {
                if(collider is null) continue;
                float yMax = collider.bounds.max.y - collider.transform.position.y + 1f;
                Vector3 scale = collider.bounds.size;
                for (int x = 0; x < (scale.x / density); x++)
                {
                    for (int z = 0; z < (scale.z / density); z++)
                    {
                        Ray ray = new((new Vector3 (x, yMax, z) - new Vector3 (scale.x * .5f, -.5f, scale.z * .5f) + collider.transform.position) * density, Vector3.down);

                        if (!collider.Raycast(ray, out RaycastHit hit, scale.y + 10f)) continue;

                        Vector3 scatter = new Vector3(Random.Range(-density, density), 0, Random.Range(-density, density));
                        Vector3 bladePosition = hit.point + scatter;

                        if(Physics.OverlapSphere(bladePosition + new Vector3(0, .5f, 0), .75f, Properties.Layers).Length > 1) continue;
                        
                        BladesInstance? blade = CreateSafeBlade(hit.normal, bladePosition);

                        if(blade is not null) Type.Collection.Add(blade.Value);
                    }
                }
            }

            Type.Collection.PushUndo();
            Type.Reload();
        }

        protected override void OnUse (bool interacting) {}
    }
}