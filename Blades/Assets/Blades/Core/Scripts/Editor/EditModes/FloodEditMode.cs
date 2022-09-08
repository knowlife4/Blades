using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FloodEditMode : EditMode
{
    public FloodEditMode (GrassManager manager, string name) : base(manager, name)
    {
        properties = new(manager);
    }

    GrassProperties properties;

    public override void OnGUI()
    {
        properties.RenderGUI(true);

        GUILayout.Space(10);

        if(GUILayout.Button("Flood Fill?")) Flood();
    }
    
    public void Flood ()
    {
        GrassType type = Manager.TypeCollection.GrassTypes[properties.Type];
        if(type is null) return;
        
        Collider[] allColliders = Selection.gameObjects.Select(x => x.GetComponentInChildren<Collider>()).ToArray();

        float density = properties.Density;

        foreach (var collider in allColliders)
        {
            if(collider is null) continue;
            float yMax = collider.bounds.max.y + 1f;
            Vector3 scale = collider.bounds.size;
            for (int x = 0; x < (scale.x / density); x++)
            {
                for (int z = 0; z < (scale.z / density); z++)
                {
                    Ray ray = new((new Vector3 (x, yMax, z) - new Vector3 (scale.x * .5f, 0, scale.z * .5f) + collider.transform.position) * density, Vector3.down);

                    Debug.DrawRay(ray.origin, ray.direction);

                    if (!collider.Raycast(ray, out RaycastHit hit, scale.y + 1f)) continue;

                    Vector3 scatter = new Vector3(Random.Range(-density, density), 0, Random.Range(-density, density));
                    Vector3 bladePosition = hit.point + scatter;

                    if(Physics.OverlapSphere(bladePosition, 1f).Length > 1) continue;

                    GrassBlade? blade = CreateBlade(properties, Random.Range(0f, 360f), Vector3.down, bladePosition);
                    if (blade is not null) type.Collection.Add(blade.Value);
                }
            }
        }

        type.Reload();
    }

    public override void OnUse (bool interacting) {}
}