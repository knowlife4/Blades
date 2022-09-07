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
                float ratio = properties.BrushSize.x / Mathf.Abs(distanceFromOuterCircle) * 100;
                if (distanceFromOuterCircle > 0 || ratio < Random.Range(0f, 100f)) continue;

                GrassBlade? blade = CreateBlade(Random.Range(0f, 360f), -hit.normal, finalPoint);
                if (blade is not null) type.Collection.Add(blade.Value);
            }
        }

        type.Reload();
    }

    GrassBlade? CreateBlade (float yRotation, Vector3 direction, Vector3 position) 
    {
        Color color = properties.Color;

        Ray placementRay = new(position -direction, direction);
        if (!Physics.Raycast(placementRay, out RaycastHit raycastHit)) return null;

        if(!properties.UseColor)
        {
            Transform root = raycastHit.collider.transform.root;
            Renderer renderer = root.GetComponentInChildren<Renderer>();

            Texture2D texture2D = renderer.sharedMaterial.mainTexture as Texture2D;

            Vector2 pCoord = raycastHit.textureCoord * new Vector2(texture2D.width, texture2D.height);
            Vector2 tiling = renderer.sharedMaterial.mainTextureScale;
            
            color = texture2D.GetPixel(Mathf.FloorToInt(pCoord.x * tiling.x) , Mathf.FloorToInt(pCoord.y * tiling.y));
        }

        // TODO Add normal orientation!

        Quaternion rotation = Quaternion.LookRotation(raycastHit.normal, Vector3.forward) * Quaternion.Euler(90f, 0, 0);
        rotation *= Quaternion.Euler(0, yRotation, 0);

        return new GrassBlade (raycastHit.point, rotation, color, properties.UseHeight ? properties.Height : 1);
    }
}
