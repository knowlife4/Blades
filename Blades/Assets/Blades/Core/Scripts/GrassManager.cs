using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassManager : MonoBehaviour
{
    public static InteractorManager InteractorManager { get; } = new();

    [SerializeField] int ignoreRate;
    [SerializeField] float viewDistance;
    [SerializeField] ComputeShader cullingShader;
    [SerializeField] GrassTypeCollection grassTypeCollection;

    ComputeBuffer interactionBuffer;

    Camera camera;

    int updateRate = 3;

    float previousFOV;
    float cameraHalfDiagonalFovDotProduct;

    void Start ()
    {
        //Create the interactionBuffer with a max of 32 interactors.
        interactionBuffer = new(32, sizeof(float) * 3, ComputeBufferType.Append);

        //Load all buffers for every grass type
        grassTypeCollection.LoadAllBuffers(cullingShader);

        camera = Camera.main;
    }

    void Update ()
    {
        SlowUpdate();

        grassTypeCollection.Render();
    }

    void SlowUpdate ()
    {
        if(Time.frameCount % updateRate != 0) return;

        UpdateFOV();

        //Update the interactionBuffer, and push this to our grass materials
        interactionBuffer.SetData(InteractorManager.Get());
        grassTypeCollection.SetMaterialBuffer("_interactionBuffer", interactionBuffer);
        grassTypeCollection.SetMaterialInt("_interactorCount", InteractorManager.Length);
        
        grassTypeCollection.Cull(camera.transform, viewDistance, cameraHalfDiagonalFovDotProduct, ignoreRate);
    }

    void UpdateFOV () 
    {
        if (camera.fieldOfView == previousFOV) return;
        
        previousFOV = camera.fieldOfView;
        cameraHalfDiagonalFovDotProduct = GetCameraHalfDiagonalFovDotProduct(camera);
    }

    void OnDestroy ()
    {
        Release();
    }

    void Release()
    {
        //Manually release all buffers to prevent warnings.
        grassTypeCollection.Release();
        interactionBuffer.Release();
    }

    public static GrassBlade? CreateGrassBlade (Vector3 position, float rotation)
    {

        // ! Move all pixel detection to the tool once it's started

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

    public static float GetCameraHalfDiagonalFovDotProduct(Camera cam)
    {
        float ratio = (float)Screen.width / Screen.height;
        float camVerticalFov = cam.fieldOfView * Mathf.Deg2Rad;
        float camFarDistance = cam.farClipPlane;
        float camFarHeight = camFarDistance * Mathf.Tan(camVerticalFov * 0.5f) * 2;
        float camFarWidth = camFarHeight * (ratio + 2);
        float camFarDiagonal = Mathf.Sqrt(camFarHeight * camFarHeight + camFarWidth * camFarWidth);
        float camFarDiagonalHalf = camFarDiagonal * 0.5f;
        float camHypotenuse = Mathf.Sqrt(camFarDistance * camFarDistance + camFarDiagonalHalf * camFarDiagonalHalf);
        float cosHalfCamDiagonalFov = camFarDistance / camHypotenuse;
        return cosHalfCamDiagonalFov;
    }
}