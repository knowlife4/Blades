using UnityEditor;
using UnityEngine;

namespace Blades.UnityEditor
{
    public class UpdateEditMode : EditMode
    {
        public UpdateEditMode (BladesManager manager, string name) : base(manager, name) {}

        bool updatePosition;

        protected override void OnGUI()
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

        protected override void OnUse(bool interacting)
        {
            RaycastHit? hit = Brush();
            if(hit is null || !interacting) return;

            foreach (var blade in Type.Collection.ToArray())
            {
                Update(blade, hit.Value.point);
            }

            Type.Reload();
        }

        public void Update (BladesInstance oldDetail, Vector3 hitPoint)
        {
            float distanceFromOuterCircle = DistanceFromCircle(oldDetail.Position, hitPoint, Properties.BrushSize.y);
            float distanceFromInnerCircle = DistanceFromCircle(oldDetail.Position, hitPoint, Properties.BrushSize.x);
            
            float ratio = distanceFromInnerCircle / (Properties.BrushSize.y - Properties.BrushSize.x);
            if (distanceFromOuterCircle > 0) return;
            
            Color oldBladeColor = new(oldDetail.Color.x, oldDetail.Color.y, oldDetail.Color.z);
            Color transparencyColor = Color.Lerp(oldBladeColor, Properties.Color, Properties.Color.a);
            Color newBladeColor = Properties.UseColor ? Color.Lerp(transparencyColor, oldBladeColor, ratio) : oldBladeColor;

            float newBladeHeight = Properties.UseHeight ? Mathf.Lerp(Properties.Height, oldDetail.Height, ratio) : oldDetail.Height;

            Vector3 newPosition = oldDetail.Position;
            Matrix4x4 newRotation = oldDetail.Rotation;
            if(updatePosition)
            {
                Ray detailRay = new(oldDetail.Position + new Vector3(0, hitPoint.y + 1f, 0), Vector3.down);
                if (Physics.Raycast(detailRay, out RaycastHit hit))
                {
                    newPosition = hit.point;
                    newRotation = Matrix4x4.Rotate(Quaternion.LookRotation(hit.normal, Vector3.forward) * Quaternion.Euler(90f, 0, 0) * Quaternion.Euler(0, Random.Range(0f, 360f), 0));
                }
            }

            BladesInstance newDetail = new(newPosition, newRotation, new(newBladeColor.r, newBladeColor.g, newBladeColor.b), newBladeHeight);

            Type.Collection.Replace(oldDetail, newDetail);
            return;
        }

        protected override void OnUseEnd () 
        {
            if(Type.Collection is not null) Type.Collection.PushUndo();
        }
    }
}