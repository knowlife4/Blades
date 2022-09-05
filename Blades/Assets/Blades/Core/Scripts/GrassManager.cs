using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassManager : MonoBehaviour
{
    public static InteractorManager InteractorManager { get; } = new();

    [SerializeField] GrassTypeCollection grassTypeCollection;

    ComputeBuffer argsBuffer;
    
    ComputeBuffer interactionBuffer;

    int updateRate = 3;

    void Start ()
    {
        //Create Args Buffer
        argsBuffer = new(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

        //Create the interactionBuffer with a max of 32 interactors.
        interactionBuffer = new(32, sizeof(float) * 3, ComputeBufferType.Append);

        //Load all buffers for every grass type
        grassTypeCollection.LoadAllBuffers();
    }

    void Update ()
    {
        SlowUpdate();

        grassTypeCollection.Render(argsBuffer);
    }

    void SlowUpdate ()
    {
        if(Time.frameCount % updateRate != 0) return;

        //Update the interactionBuffer, and push this to our grass materials
        interactionBuffer.SetData(InteractorManager.Get());
        grassTypeCollection.SetMaterialBuffer("_interactionBuffer", interactionBuffer);
        grassTypeCollection.SetMaterialInt("_interactorCount", InteractorManager.Length);
    }

    void OnDestroy ()
    {
        Release();
    }

    void Release()
    {
        argsBuffer.Release();
        grassTypeCollection.Release();
        interactionBuffer.Release();
    }

    public static GrassBlade? CreateGrassBlade (Vector3 position, float rotation)
    {
        float distanceMargin = .2f;

        Ray colorRay = new(position + new Vector3(0, distanceMargin, 0), -Vector3.up);
        if (!Physics.Raycast(colorRay, out RaycastHit raycastHit)) return null;

        Transform root = raycastHit.collider.transform.root;
        Renderer renderer = root.GetComponentInChildren<Renderer>();

        Texture2D texture2D = renderer.material.mainTexture as Texture2D;

        Vector2 pCoord = raycastHit.textureCoord * new Vector2(texture2D.width, texture2D.height);
        Vector2 tiling = renderer.material.mainTextureScale;

        Color color = texture2D.GetPixel(Mathf.FloorToInt(pCoord.x * tiling.x) , Mathf.FloorToInt(pCoord.y * tiling.y));

        return new GrassBlade(position, rotation, color);
    }
}