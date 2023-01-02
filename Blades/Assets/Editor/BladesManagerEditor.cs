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
            
            bool startPress = e.isMouse && (e.type == EventType.MouseDown) && e.button == 1;
            bool interacting = e.isMouse && (e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && e.button == 1;

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

            if(startPress) modeSelector.UseStart();
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
        public Color ColorA { get; set; }
        public Color ColorB { get; set; }

        public Color Color => HSVColor.Lerp(new(ColorA), new(ColorB), Random.value).ToColor();

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

            UseHeight = RenderOption("Height", showUse, UseHeight, () => {Height = EditorGUILayout.FloatField(Height, GUILayout.ExpandWidth(true));});

            UseColor = RenderOption("Color", showUse, UseColor, () => {GUILayout.Space(50); ColorA = EditorGUILayout.ColorField(ColorA, GUILayout.ExpandWidth(true)); ColorB = EditorGUILayout.ColorField(ColorB, GUILayout.ExpandWidth(true));});
        }

        public static bool RenderOption (string name, bool showUse, bool use, System.Action guiCall)
        {
            bool toggleValue = false;
            GUILayout.BeginHorizontal();
                if(showUse) toggleValue = EditorGUILayout.Toggle(use, GUILayout.Width(20));
                EditorGUI.BeginDisabledGroup(!use);
                    GUILayout.Label($"{name}");
                    guiCall();
                EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            return toggleValue;
        }
    }
}