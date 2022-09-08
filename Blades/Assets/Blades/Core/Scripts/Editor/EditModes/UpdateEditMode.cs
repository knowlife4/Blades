using UnityEditor;
using UnityEngine;

namespace Blades.UnityEditor
{
    public class UpdateEditMode : EditMode
    {
        public UpdateEditMode (GrassManager manager, string name) : base(manager, name) {}

        bool updatePosition;

        public override void OnGUI()
        {
            Properties.RenderGUI(true);

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
            RaycastHit? hit = Brush();
            if(hit is null || !interacting) return;

            foreach (var blade in Type.Collection.ToArray())
            {
                Update(blade, hit.Value.point);
            }

            Type.Reload();
        }

        public void Update (GrassBlade oldBlade, Vector3 hitPoint)
        {
            float distanceFromOuterCircle = DistanceFromCircle(oldBlade.Position, hitPoint, Properties.BrushSize.y);
            float distanceFromInnerCircle = DistanceFromCircle(oldBlade.Position, hitPoint, Properties.BrushSize.x);
            
            float ratio = distanceFromInnerCircle / (Properties.BrushSize.y - Properties.BrushSize.x);
            if (distanceFromOuterCircle > 0) return;
            
            Color oldBladeColor = new(oldBlade.Color.x, oldBlade.Color.y, oldBlade.Color.z);
            Color transparencyColor = Color.Lerp(oldBladeColor, Properties.Color, Properties.Color.a);
            Color newBladeColor = Properties.UseColor ? Color.Lerp(transparencyColor, oldBladeColor, ratio) : oldBladeColor;

            float newBladeHeight = Properties.UseHeight ? Mathf.Lerp(Properties.Height, oldBlade.Height, ratio) : oldBlade.Height;

            Vector3 newPosition = oldBlade.Position;
            Matrix4x4 newRotation = oldBlade.Rotation;
            if(updatePosition)
            {
                Ray grassRay = new(oldBlade.Position + new Vector3(0, hitPoint.y + 1f, 0), Vector3.down);
                if (Physics.Raycast(grassRay, out RaycastHit hit))
                {
                    newPosition = hit.point;
                    newRotation = Matrix4x4.Rotate(Quaternion.LookRotation(hit.normal, Vector3.forward) * Quaternion.Euler(90f, 0, 0) * Quaternion.Euler(0, Random.Range(0f, 360f), 0));
                }
            }

            GrassBlade newBlade = new(newPosition, newRotation, new(newBladeColor.r, newBladeColor.g, newBladeColor.b), newBladeHeight);

            Type.Collection.Replace(oldBlade, newBlade);
            return;
        }

        public override void OnUseEnd () 
        {
            Type.Collection.PushUndo();
        }
    }
}