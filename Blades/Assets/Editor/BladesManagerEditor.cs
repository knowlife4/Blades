using UnityEngine;
using UnityEditor;
using Blades.Rendering;
using UnityEngine.SceneManagement;
using UnityEditorInternal;

namespace Blades.UnityEditor
{
    [CustomEditor(typeof(BladesManager))]
    public class BladesManagerEditor : Editor
    {
        BladesManager manager;

        EditModeSelector modeSelector;

        void OnEnable ()
        {
            manager = (BladesManager)target;
            
            if(modeSelector == null)
            {
                modeSelector = new
                (
                    new PaintEditMode(manager, "Paint"),
                    new EraseEditMode(manager, "Erase"),
                    new UpdateEditMode(manager, "Update"),
                    new FloodEditMode(manager, "Flood")
                );
            }
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

            if(GUILayout.Button("Save"))
            {
                foreach (var type in manager.BladesTypeCollection.BladesTypes)
                {
                    type.Collection.Save();
                }
            }

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
            BladesDataCollection lastUpdated = BladesDataCollection.LastUpdated;

            if(lastUpdated is null) return;

            GUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(!lastUpdated.CanUndo);
                    if(GUILayout.Button("Undo"))
                    {
                        lastUpdated.Undo();
                        manager.BladesTypeCollection.Reload();
                    }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(!lastUpdated.CanRedo);
                    if(GUILayout.Button("Redo")) 
                    {
                        lastUpdated.Redo();
                        manager.BladesTypeCollection.Reload();
                    }
                EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }
    }

    public class BladesProperties
    {
        public BladesProperties (BladesManager manager)
        {
            Manager = manager;
        }

        public BladesManager Manager { get; }

        public int Type { get; set; }

        public Vector2 BrushSize { get; set; } = new (.25f, 5f);

        public Vector2 BrushSizeMinMax { get; set; } = new (.25f, 5f);

        public float Density { get; set; } = 1;

        public float NormalLimit { get; set; } = 180;

        public int Layers { get; set; } = ~0;

        public bool UseHeight { get; set; }
        public float Height { get; set; }

        public bool UseColor { get; set; }
        public Color Color { get; set; }

        public void RenderGUI (bool showUse)
        {
            RenderBrushGUI();
            GUILayout.Space(10);
            RenderBladesGUI(showUse);
        }

        public void RenderBrushGUI ()
        {
            GUILayout.Label("Brush Properties:");

            BladesType[] BladesTypes = Manager.BladesTypeCollection.BladesTypes;
            Type = EditorGUILayout.IntSlider("Brush Type", Type, 0, BladesTypes.Length - 1);

            Vector2 brushSize = BrushSize; 
            EditorGUILayout.MinMaxSlider("Brush Size", ref brushSize.x, ref brushSize.y, BrushSizeMinMax.x, BrushSizeMinMax.y);
            BrushSize = brushSize;

            Density = EditorGUILayout.Slider("Brush Density", Density, .01f, 5f);
            NormalLimit = EditorGUILayout.Slider("Brush Limit", NormalLimit, 1f, 180f);
            Layers = EditorGUILayout.MaskField("Paint Layers", Layers, InternalEditorUtility.layers);
        }

        public void RenderBladesGUI (bool showUse)
        {
            GUILayout.Label("Blades Properties:");

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