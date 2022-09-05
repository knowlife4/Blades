using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractorManager
{
    public Dictionary<GameObject, Vector3> Bank { get; } = new();

    public int Length => Bank.Count;

    public void Add (GameObject gameObject)
    {
        if(Bank.ContainsKey(gameObject)) return;
        Bank.Add(gameObject, gameObject.transform.position);
    }

    public void Remove (GameObject gameObject)
    {
        if(!Bank.ContainsKey(gameObject)) return;
        Bank.Remove(gameObject);
    }

    public Vector3[] Get ()
    {
        Update();
        return Bank.Values.ToArray();
    }

    void Update ()
    {
        List<GameObject> keys = new(Bank.Keys);
        foreach (var gameObject in keys)
        {
            Bank[gameObject] = gameObject.transform.position;
        }
    }
}