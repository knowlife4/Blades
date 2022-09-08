using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintEditMode : EditMode
{
    public PaintEditMode (GrassManager manager, string name) : base(manager, name) 
    {
        properties = new(manager);
    }

    GrassProperties properties;
    
    public override void OnGUI()
    {
        properties.RenderGUI(true);
    }

    public override void OnUse (bool interacting)
    {
        RaycastHit hit = Brush(properties.BrushSize) ?? new();
        if(hit.collider is null || !interacting) return;

        float density = properties.Density;
        float scaleFactor = 1 / density;

        GrassType type = Manager.TypeCollection.GrassTypes[properties.Type];
        if(type is null) return;

        for (int x = 0; x <= scaleFactor; x++)
        {
            for (int z = 0; z <= scaleFactor; z++)
            {
                var point = (new Vector3 (x, 0, z) * density) - new Vector3(.5f, 0, .5f);
                var scatter = new Vector3(Random.Range(-density, density), 0, Random.Range(-density, density));
                var scatteredPoint = point + scatter;
                var finalPoint = hit.point + (scatteredPoint * properties.BrushSize.y);

                float distanceFromOuterCircle = DistanceFromCircle(finalPoint, hit.point, properties.BrushSize.y);
                float distanceFromInnerCircle = DistanceFromCircle(finalPoint, hit.point, properties.BrushSize.x);
        
                float ratio = Mathf.Abs((distanceFromInnerCircle / (properties.BrushSize.y - properties.BrushSize.x)) - 1);
                if (distanceFromOuterCircle > 0 || ratio < Random.Range(0f, 1f)) continue;

                GrassBlade? blade = CreateBlade(properties, Random.Range(0f, 360f), -hit.normal, finalPoint);
                if (blade is not null) type.Collection.Add(blade.Value);
            }
        }

        type.Reload();
    }
}
