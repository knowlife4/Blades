using UnityEditor;
using UnityEngine;

public abstract class EditMode
{
    public EditMode (GrassManager manager, string name) 
    {
        Manager = manager;
        Name = name;
    }

    public string Name { get; }
    public GrassManager Manager { get; }
    public abstract void OnGUI ();
    public abstract void OnUse (bool interacting);

    public RaycastHit? Brush (Vector2 brushSize)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return null;

        Handles.color = Color.red;
        Handles.DrawWireDisc(hit.point, hit.normal, brushSize.x);

        Handles.color = Color.white;
        Handles.DrawWireDisc(hit.point, hit.normal, brushSize.y);

        return hit;
    }

    protected float DistanceFromCircle (Vector3 point, Vector3 origin, float radius)
    {
        return (point - origin).magnitude - radius;
    }

    protected GrassBlade? CreateBlade (GrassProperties properties, float yRotation, Vector3 direction, Vector3 position) 
    {
        Color color = properties.Color;

        Ray placementRay = new(position -direction, direction);
        if (!Physics.Raycast(placementRay, out RaycastHit raycastHit)) return null;

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
}
