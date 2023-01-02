using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blades.Rendering
{
    [System.Serializable]
    public class BladesTypeCollection
    {
        [SerializeField] BladesType[] bladesTypes;
        public BladesType[] BladesTypes => bladesTypes ?? System.Array.Empty<BladesType>();

        public void LoadAllBuffers (ComputeShader shader)
        {
            foreach (var type in BladesTypes)
            {
                type.LoadBladesBuffer(shader);
            }
        }

        public void Render ()
        {
            foreach (var type in BladesTypes)
            {
                type.Render();
            }
        }

        public void SetMaterialBuffer (string bufferName, ComputeBuffer buffer)
        {
            foreach (var type in BladesTypes)
            {
                type.UpdateMaterialBuffer(bufferName, buffer);
            }
        }

        public void SetMaterialInt (string bufferName, int integer)
        {
            foreach (var type in BladesTypes)
            {
                type.UpdateMaterialInt(bufferName, integer);
            }
        }

        public void Release ()
        {
            foreach (var type in BladesTypes)
            {
                type.Release();
            }
        }

        public void Cull (Transform camTransform, float distance, float cameraHalfDiagonalFovDotProduct, int ignoreRate)
        {
            foreach (var type in BladesTypes)
            {
                if(type.Collection != null || type.Collection.Count != 0) type.Cull(camTransform, distance, cameraHalfDiagonalFovDotProduct, ignoreRate);
            }
        }

        public void Reload () 
        {
            foreach (var type in BladesTypes)
            {
                type.Reload();
            }
        }
    }

    [System.Serializable]
    public class BladesType
    {
        public BladesTypeAsset TypeAsset;
        public BladesDataCollection Collection;

        public Material MaterialInstance { get; private set; }

        ComputeShader cullingShader;
        ComputeBuffer instanceBuffer;
        ComputeBuffer instanceBufferRender;
        ComputeBuffer argsBuffer;

        int kernel;

        uint threadX;

        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

        bool canLoad => TypeAsset != null && Collection != null && Collection.Count > 0;

        bool execute
        {
            get
            {   bool result = canLoad && instanceBuffer is not null && MaterialInstance is not null && cullingShader is not null;
                if(!result) LoadBladesBuffer(cullingShader);
                return result;
            }
        }

        public void LoadBladesBuffer (ComputeShader shader)
        {
            if(!canLoad || shader is null) return;
            instanceBuffer = new(Collection.Count, BladesInstance.Size, ComputeBufferType.Structured);
            instanceBuffer.SetData(Collection.ToArray());

            instanceBufferRender = new(Collection.Count, BladesInstance.Size, ComputeBufferType.Append);

            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

            cullingShader = Object.Instantiate(shader);

            kernel = cullingShader.FindKernel("Cull");

            cullingShader.SetBuffer(kernel, "_bladeBuffer", instanceBuffer);
            cullingShader.SetBuffer(kernel, "_bladeBufferRender", instanceBufferRender);
            cullingShader.GetKernelThreadGroupSizes(kernel, out threadX, out _, out _);

            MaterialInstance = Object.Instantiate(TypeAsset.Material);
            if(TypeAsset.Texture is not null) MaterialInstance.SetTexture("_MainTex", TypeAsset.Texture);
        }

        public void Cull (Transform camTransform, float distance, float cameraHalfDiagonalFovDotProduct, int ignoreRate) 
        {
            if(!execute || instanceBufferRender == null) return;
            instanceBufferRender.SetCounterValue(0);

            cullingShader.SetVector("_cameraPosition", camTransform.position);
            cullingShader.SetVector("_cameraForward", camTransform.forward);
            cullingShader.SetFloat("_cameraHalfDiagonalFovDotProduct", cameraHalfDiagonalFovDotProduct);
            cullingShader.SetFloat("_distance", distance);
            cullingShader.SetInt("_ignoreRate", ignoreRate);
            
            int xThreadCount = Mathf.CeilToInt((float)Collection.Count / threadX);
            if(xThreadCount > 0) cullingShader.Dispatch(kernel, xThreadCount, 1, 1);

            MaterialInstance.SetBuffer("_bladeBuffer", instanceBufferRender);
        }

        public void Render ()
        {
            if(!execute) return;

            Mesh mesh = TypeAsset.Mesh;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                args[0] = mesh.GetIndexCount(i);
                argsBuffer.SetData( args );
                ComputeBuffer.CopyCount(instanceBufferRender, argsBuffer,sizeof(uint));

                Graphics.DrawMeshInstancedIndirect
                (
                    mesh, i, MaterialInstance,
                    new Bounds(Vector3.zero, Vector3.one * 1000),
                    argsBuffer
                );
            }
        }

        public void Release ()
        {
            if(instanceBuffer is not null) instanceBuffer.Release();
            if(instanceBufferRender is not null) instanceBufferRender.Release();
            if(argsBuffer is not null) argsBuffer.Release();
        }

        public void Reload () 
        {
            Release();
            LoadBladesBuffer(cullingShader);
        }

        public void UpdateMaterialBuffer (string bufferName, ComputeBuffer buffer) 
        {
            if(execute) MaterialInstance.SetBuffer(bufferName, buffer);
        }

        public void UpdateMaterialInt (string intName, int value)
        {
            if(execute) MaterialInstance.SetInt(intName, value);
        }
    }
}