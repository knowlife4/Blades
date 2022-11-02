using Blades.Interaction;
using Blades.Rendering;
using UnityEngine;

namespace Blades
{
    [ExecuteAlways]
    public class BladesManager : MonoBehaviour
    {
        static readonly InteractorManager interactorManager;
        public static InteractorManager InteractorManager => interactorManager ?? new();

        [SerializeField] int ignoreRate;
        [SerializeField] ComputeShader cullingShader;
        
        [SerializeField] BladesTypeCollection bladesTypeCollection;
        public BladesTypeCollection BladesTypeCollection => bladesTypeCollection;

        #if UNITY_EDITOR
        [SerializeField] [HideInInspector] public BladesSceneSettings sceneSettings;
        #endif

        ComputeBuffer interactionBuffer;

        Camera mainCamera;

        int updateRate = 5;
        float viewDistance = 150;

        float previousFOV;
        float cameraHalfDiagonalFovDotProduct;

        public void ChangeCamera (Camera newCamera) 
        {
            mainCamera = newCamera;
        }

        public void ChangeUpdateRate (int newUpdateRate) 
        {
            updateRate = newUpdateRate;
        }

        public void ChangeViewDistance (float newViewDistance) 
        {
            viewDistance = newViewDistance;
        }

        public void Setup () 
        {
            //Create the interactionBuffer with a max of 32 interactors.
            interactionBuffer = new(32, sizeof(float) * 3, ComputeBufferType.Append);

            //Load all buffers for every blades type
            if(BladesTypeCollection is not null) BladesTypeCollection.LoadAllBuffers(cullingShader);
        
            mainCamera = Camera.main;
        }

        void OnEnable ()
        {
            Setup();
        }

        void Update ()
        {
            SlowUpdate();

            BladesTypeCollection.Render();
        }

        void SlowUpdate ()
        {
            if(Time.frameCount % updateRate != 0) return;

            UpdateFOV();

            if(interactionBuffer is null) Setup();

            //Update the interactionBuffer, and push this to our instance's materials
            interactionBuffer.SetData(InteractorManager.Get());
            BladesTypeCollection.SetMaterialBuffer("_interactionBuffer", interactionBuffer);
            BladesTypeCollection.SetMaterialInt("_interactorCount", InteractorManager.Length);
            
            BladesTypeCollection.Cull(mainCamera.transform, viewDistance, cameraHalfDiagonalFovDotProduct, ignoreRate);
        }

        void UpdateFOV () 
        {
            if (mainCamera.fieldOfView == previousFOV) return;
            
            previousFOV = mainCamera.fieldOfView;
            cameraHalfDiagonalFovDotProduct = GetCameraHalfDiagonalFovDotProduct(mainCamera);
        }

        void OnDisable ()
        {
            Release();
        }

        void Release()
        {
            //Manually release all buffers to prevent warnings.
            BladesTypeCollection.Release();
            interactionBuffer.Release();
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

    [System.Serializable]
    public struct BladesSceneSettings
    {
        public float ViewDistance;
    }
}