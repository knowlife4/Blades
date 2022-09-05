using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladesInteractor : MonoBehaviour
{
    public void OnEnable()
    {
        GrassManager.InteractorManager.Add(gameObject);
    }

    public void OnDisable()
    {
        GrassManager.InteractorManager.Remove(gameObject);
    }
}
