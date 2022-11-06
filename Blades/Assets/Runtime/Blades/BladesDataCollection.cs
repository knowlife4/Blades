using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blades
{
    [CreateAssetMenu(fileName = "Blades Data", menuName = "Blades/Blades Data Asset", order = 1)]
    public class BladesDataCollection : ScriptableObject
    {
        [SerializeField] BladesInstance[] blades;
        HashSet<BladesInstance> tempBlades = new();

        List<BladesInstance[]> previous = new();

        public int Count => tempBlades.Count;

        public HashSet<BladesInstance> CurrentBlades => tempBlades;

        public bool CanUndo => currentUndo + 1 <= previous.Count - 1;

        public bool CanRedo => currentUndo - 1 >= 0 ;

        public static BladesDataCollection LastUpdated;

        int currentUndo;

        void OnEnable ()
        {
            Load();
        }

        void OnDisable ()
        {
            Save();
        }

        public void Add (BladesInstance blade)
        {
            if(!tempBlades.Contains(blade)) tempBlades.Add(blade);
        }

        public void Remove (BladesInstance blade)
        {
            if(tempBlades.Contains(blade)) tempBlades.Remove(blade);
        }

        public BladesInstance[] ToArray() 
        {
            BladesInstance[] bladesArray = new BladesInstance[tempBlades.Count];
            tempBlades.CopyTo(bladesArray);

            return bladesArray;
        }

        public void Replace (BladesInstance oldBlade, BladesInstance newBlade)
        {
            Remove(oldBlade);
            Add(newBlade);
        }

        public void Clear ()
        {
            tempBlades.Clear();
        }

        public void Redo ()
        {
            if (!CanRedo) return;

            currentUndo--;
            tempBlades = new(previous[currentUndo]);
        }

        public void Undo ()
        {
            if (!CanUndo) return;

            currentUndo++;
            tempBlades = new(previous[currentUndo]);
        }

        public void PushUndo ()
        {
            if(previous.Count > 5) previous.RemoveAt(previous.Count - 1);

            if(currentUndo > 0 && previous.Count > currentUndo) previous.RemoveRange(0, currentUndo);

            LastUpdated = this;
            currentUndo = 0;
            previous.Insert(0, ToArray());
        }

        public void Save ()
        {
            blades = ToArray();

            //! GET RID OF THIS FOR THE LOVE OF GOD AND ALL THAT IS HOLY
            #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssets();
            #endif
        }

        public void Load ()
        {
            if(blades is not null) tempBlades = new(blades);
        }
    }

    [System.Serializable]
    public struct BladesInstance
    {
        public BladesInstance (Vector3 position, Quaternion rotation, Color color, float height = 1) : this(position, Matrix4x4.Rotate(rotation), new Vector3(color.linear.r, color.linear.g, color.linear.b), height) {}

        public BladesInstance (Vector3 position, Matrix4x4 rotation, Vector3 color, float height = 1)
        {
            Position = position;
            Height = height;
            Color = color;
            Rotation = rotation;
        }

        public Vector3 Position;
        public Matrix4x4 Rotation;
        public float Height;
        public Vector3 Color;

        public static int Size =>  (/*Color*/sizeof(float) * 3) + (/*Position*/sizeof(float) * 3) + (/*Rotation*/sizeof(float) * 16) + (/*Height*/sizeof(float));
    }
}