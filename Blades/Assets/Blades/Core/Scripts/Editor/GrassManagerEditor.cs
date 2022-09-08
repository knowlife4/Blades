using UnityEngine;
using UnityEditor;
using Blades.Rendering;

namespace Blades.UnityEditor
{
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

            bool scrolling = e.type == EventType.ScrollWheel;
            float delta = -e.delta.y;

            if(interacting || scrolling) Event.current.Use();

            if(scrolling)
            {
                if(e.shift)
                {
                    modeSelector.AddBrushHardness(delta/10);
                }
                else
                {
                    modeSelector.AddBrushSize(delta/10);
                }
            }

            modeSelector.Use(interacting);

            if(e.type == EventType.MouseDown) modeSelector.UseStart();
            if(e.type == EventType.MouseUp) modeSelector.UseEnd();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            manager.sceneSettings.ViewDistance = EditorGUILayout.FloatField("View Distance", manager.sceneSettings.ViewDistance);
            manager.ChangeViewDistance(manager.sceneSettings.ViewDistance);

            UndoRedo();

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

        public void UndoRedo () 
        {
            GrassDataCollection lastUpdated = GrassDataCollection.LastUpdated;

            if(lastUpdated is null) return;

            GUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(!lastUpdated.CanUndo);
                    if(GUILayout.Button("Undo"))
                    {
                        lastUpdated.Undo();
                        manager.TypeCollection.Reload();
                    }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(!lastUpdated.CanRedo);
                    if(GUILayout.Button("Redo")) 
                    {
                        lastUpdated.Redo();
                        manager.TypeCollection.Reload();
                    }
                EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
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

        public Vector2 BrushSizeMinMax { get; set; } = new (.25f, 5f);

        public float Density { get; set; } = 1;

        public float NormalLimit { get; set; } = 180;

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
            EditorGUILayout.MinMaxSlider("Brush Size", ref brushSize.x, ref brushSize.y, BrushSizeMinMax.x, BrushSizeMinMax.y);
            BrushSize = brushSize;

            Density = EditorGUILayout.Slider("Grass Density", Density, .01f, 5f);
            NormalLimit = EditorGUILayout.Slider("Normal Limit", NormalLimit, 1f, 180f);
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
}