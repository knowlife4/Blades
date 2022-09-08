using Blades.Interaction;
using Blades.Rendering;
using UnityEngine;

namespace Blades
{
    [ExecuteAlways]
    public class GrassManager : MonoBehaviour
    {
        public static InteractorManager InteractorManager { get; } = new();

        [SerializeField] int ignoreRate;
        [SerializeField] ComputeShader cullingShader;
        [SerializeField] GrassTypeCollection grassTypeCollection;

        #if UNITY_EDITOR
        [SerializeField] [HideInInspector] public GrassSceneSettings sceneSettings;
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

        public GrassTypeCollection TypeCollection => grassTypeCollection;

        void OnEnable ()
        {
            //Create the interactionBuffer with a max of 32 interactors.
            interactionBuffer = new(32, sizeof(float) * 3, ComputeBufferType.Append);

            //Load all buffers for every grass type
            grassTypeCollection.LoadAllBuffers(cullingShader);
        
            mainCamera = Camera.main;
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
            if(interactionBuffer.IsValid() && interactionBuffer.count > 0)
            {
                grassTypeCollection.SetMaterialBuffer("_interactionBuffer", interactionBuffer);
                grassTypeCollection.SetMaterialInt("_interactorCount", InteractorManager.Length);
            }
            
            grassTypeCollection.Cull(mainCamera.transform, viewDistance, cameraHalfDiagonalFovDotProduct, ignoreRate);
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
            grassTypeCollection.Release();
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
    public struct GrassSceneSettings
    {
        public float ViewDistance;
    }
}