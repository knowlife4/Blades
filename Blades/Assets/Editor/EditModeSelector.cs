using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Blades.UnityEditor
{
    public class EditModeSelector
    {
        public EditModeSelector (params EditMode[] editModes) 
        {
            EditModes = editModes;

            editNames = EditModes.Select( x => x.Name).ToArray();
        }

        public EditMode[] EditModes { get; }

        readonly string[] editNames;
        
        public int Selected { get; private set; }

        public Vector2 BrushSize { get; private set; }

        public void Use (bool interacting)
        {
            EditModes[Selected].Use(interacting);
        }

        public void UseStart () 
        {
            EditModes[Selected].UseStart();
        }

        public void UseEnd ()
        {
            EditModes[Selected].UseEnd();
        }

        public void RenderGUI ()
        {
            Selected = GUILayout.Toolbar(Selected, editNames);

            EditModes[Selected].GUI();
        }

        public void AddBrushSize (float scale)
        {
            Selected = GUILayout.Toolbar(Selected, editNames);

            EditModes[Selected].AddBrushSize(scale);
        }

        public void AddBrushHardness (float scale)
        {
            Selected = GUILayout.Toolbar(Selected, editNames);

            EditModes[Selected].AddBrushHardness(scale);
        }
    }
}