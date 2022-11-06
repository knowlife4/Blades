using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Blades.Interaction
{
    public class InteractorManager
    {
        Dictionary<GameObject, Vector3> bank;

        public Dictionary<GameObject, Vector3> Bank 
        {
            get
            {
                if(bank == null) bank = new();

                return bank;
            }
        }

        public int Length => Bank.Count;

        public void Add(GameObject gameObject)
        {
            if (Bank.ContainsKey(gameObject)) return;
            Bank.Add(gameObject, gameObject.transform.position);
        }

        public void Remove(GameObject gameObject)
        {
            if (!Bank.ContainsKey(gameObject)) return;
            Bank.Remove(gameObject);
        }

        public Vector3[] Get()
        {
            Update();
            return Bank.Values.ToArray();
        }

        void Update()
        {
            //Debug.Log(Length);

            List<GameObject> keys = new(Bank.Keys);
            foreach (var gameObject in keys)
            {
                Bank[gameObject] = gameObject.transform.position;
            }
        }
    }
}