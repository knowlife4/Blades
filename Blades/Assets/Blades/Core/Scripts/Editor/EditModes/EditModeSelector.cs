using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        EditModes[Selected].OnUse(interacting);
    }

    public void RenderGUI ()
    {
        Selected = GUILayout.Toolbar(Selected, editNames);

        EditModes[Selected].OnGUI();
    }
}
