using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(GrassManager))]
public class GrassManagerEditor : Editor
{
    GrassManager manager;

    EditModeSelector modeSelector;

    void OnEnable ()
    {
        manager = (GrassManager)target;
        modeSelector = new
        (
            new PaintEditMode(manager, "Paint"),
            new EraseEditMode(manager, "Erase"),
            new UpdateEditMode(manager, "Update"),
            new FloodEditMode(manager, "Flood")
        );
    }

    void OnSceneGUI () 
    {
        Event e = Event.current;
        if(!e.alt) return;
        
        bool interacting = e.isMouse && e.type == EventType.MouseDrag && e.button == 1;

        if(interacting) Event.current.Use();

        modeSelector.Use(interacting);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        manager.sceneSettings.ViewDistance = EditorGUILayout.FloatField("View Distance", manager.sceneSettings.ViewDistance);
        manager.ChangeViewDistance(manager.sceneSettings.ViewDistance);

        SceneView lastScene = SceneView.lastActiveSceneView;
        if(lastScene is null) return;

        if(!Application.isPlaying)
        {
            manager.ChangeCamera(lastScene.camera);
            manager.ChangeUpdateRate(1);
        }

        modeSelector.RenderGUI();

        SceneView.RepaintAll();
    }
}

public class GrassProperties
{
    public GrassProperties (GrassManager manager)
    {
        Manager = manager;
    }

    public GrassManager Manager { get; }

    public int Type { get; set; }

    public Vector2 BrushSize { get; set; } = new (.25f, 5f);

    public float Density { get; set; } = 1;

    public bool UseHeight { get; set; }
    public float Height { get; set; }

    public bool UseColor { get; set; }
    public Color Color { get; set; }

    public void RenderGUI (bool showUse)
    {
        RenderBrushGUI();
        GUILayout.Space(10);
        RenderGrassGUI(showUse);
    }

    public void RenderBrushGUI ()
    {
        GUILayout.Label("Brush Properties:");

        GrassType[] grassTypes = Manager.TypeCollection.GrassTypes;
        Type = EditorGUILayout.IntSlider("Grass Type", Type, 0, grassTypes.Length - 1);

        Vector2 brushSize = BrushSize; 
        EditorGUILayout.MinMaxSlider("Brush Size", ref brushSize.x, ref brushSize.y, .25f, 5f);
        BrushSize = brushSize;

        Density = EditorGUILayout.Slider("Grass Density", Density, .01f, 5f);
    }

    public void RenderGrassGUI (bool showUse)
    {
        GUILayout.Label("Grass Properties:");

        EditorGUIUtility.labelWidth = 100;
        GUILayout.BeginHorizontal();
            if(showUse) UseHeight = EditorGUILayout.Toggle(UseHeight, GUILayout.Width(20));
            EditorGUI.BeginDisabledGroup(!UseHeight);
                EditorGUILayout.PrefixLabel("Height:");
                Height = EditorGUILayout.FloatField(Height, GUILayout.ExpandWidth(true));
            EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
            if(showUse) UseColor = EditorGUILayout.Toggle(UseColor, GUILayout.Width(20));
            EditorGUI.BeginDisabledGroup(!UseColor);
                EditorGUILayout.PrefixLabel("Color:");
                Color = EditorGUILayout.ColorField(Color, GUILayout.ExpandWidth(true));
            EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
    }
}