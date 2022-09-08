using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EraseEditMode : EditMode
{
    public EraseEditMode (GrassManager manager, string name) : base(manager, name) 
    {
        properties = new(manager);
    }

    GrassProperties properties;

    public override void OnGUI()
    {
        properties.RenderBrushGUI();

        if(GUILayout.Button("Erase All?")) EraseAll();
    }

    public override void OnUse(bool interacting)
    {
        RaycastHit hit = Brush(properties.BrushSize) ?? new();
        if(hit.collider is null || !interacting) return;

        GrassType type = Manager.TypeCollection.GrassTypes[properties.Type];

        foreach (var blade in type.Collection.ToArray())
        {
            Erase(type, blade, hit.point);
        }

        type.Reload();
    }

    public void Erase (GrassType type, GrassBlade blade, Vector3 hitPoint)
    {
        float distanceFromOuterCircle = DistanceFromCircle(blade.Position, hitPoint, properties.BrushSize.y);
        float distanceFromInnerCircle = DistanceFromCircle(blade.Position, hitPoint, properties.BrushSize.x);
        
        float ratio = Mathf.Abs((distanceFromInnerCircle / (properties.BrushSize.y - properties.BrushSize.x)) - 1);
        if (distanceFromOuterCircle > 0 || ratio < Random.Range(0f, 1f)) return;
        type.Collection.Remove(blade);
        return;
    }

    public void EraseAll ()
    {
        GrassType type = Manager.TypeCollection.GrassTypes[properties.Type];
        
        type.Collection.Clear();
    }
}
