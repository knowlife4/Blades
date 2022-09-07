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
}
