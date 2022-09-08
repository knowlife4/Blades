using UnityEditor;
using UnityEngine;

public class UpdateEditMode : EditMode
{
    public UpdateEditMode (GrassManager manager, string name) : base(manager, name) 
    {
        properties = new(manager);
    }

    GrassProperties properties;
    bool updatePosition;

    public override void OnGUI()
    {
        properties.RenderGUI(true);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
            updatePosition = EditorGUILayout.Toggle(updatePosition, GUILayout.Width(20));
            EditorGUI.BeginDisabledGroup(!updatePosition);
                GUILayout.Label("Reproject");
            EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
    }

    public override void OnUse(bool interacting)
    {
        RaycastHit hit = Brush(properties.BrushSize) ?? new();
        if(hit.collider is null || !interacting) return;

        GrassType type = Manager.TypeCollection.GrassTypes[properties.Type];

        foreach (var blade in type.Collection.ToArray())
        {
            Update(type, blade, hit.point);
        }

        type.Reload();
    }

    public void Update (GrassType type, GrassBlade oldBlade, Vector3 hitPoint)
    {
        float distanceFromOuterCircle = DistanceFromCircle(oldBlade.Position, hitPoint, properties.BrushSize.y);
        float distanceFromInnerCircle = DistanceFromCircle(oldBlade.Position, hitPoint, properties.BrushSize.x);
        
        float ratio = distanceFromInnerCircle / (properties.BrushSize.y - properties.BrushSize.x);
        if (distanceFromOuterCircle > 0) return;
        
        Color oldBladeColor = new(oldBlade.Color.x, oldBlade.Color.y, oldBlade.Color.z);
        Color transparencyColor = Color.Lerp(oldBladeColor, properties.Color, properties.Color.a);
        Color newBladeColor = properties.UseColor ? Color.Lerp(transparencyColor, oldBladeColor, ratio) : oldBladeColor;

        float newBladeHeight = properties.UseHeight ? Mathf.Lerp(properties.Height, oldBlade.Height, ratio) : oldBlade.Height;

        Vector3 newPosition = oldBlade.Position;
        if(updatePosition)
        {
            Ray grassRay = new(oldBlade.Position + new Vector3(0, hitPoint.y + 1f, 0), Vector3.down);
            if (Physics.Raycast(grassRay, out RaycastHit hit)) newPosition = hit.point;
        }

        GrassBlade newBlade = new(newPosition, oldBlade.Rotation, new(newBladeColor.r, newBladeColor.g, newBladeColor.b), newBladeHeight);

        type.Collection.Replace(oldBlade, newBlade);
        return;
    }
}
