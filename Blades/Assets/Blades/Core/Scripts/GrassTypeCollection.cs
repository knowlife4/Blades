using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GrassTypeCollection
{
    [SerializeField] GrassType[] grassTypes;

    public GrassType[] GrassTypes => grassTypes;

    public void LoadAllBuffers (ComputeShader shader)
    {
        foreach (var type in grassTypes)
        {
            type.LoadGrassBuffer(shader);
        }
    }

    public void Render ()
    {
        foreach (var type in grassTypes)
        {
            type.Render();
        }
    }

    public void SetMaterialBuffer (string bufferName, ComputeBuffer buffer)
    {
        foreach (var type in grassTypes)
        {
            type.Material.SetBuffer(bufferName, buffer);
        }
    }

    public void SetMaterialInt (string bufferName, int integer)
    {
        foreach (var type in grassTypes)
        {
            type.Material.SetInt(bufferName, integer);
        }
    }

    public void Release ()
    {
        foreach (var type in grassTypes)
        {
            type.Release();
        }
    }

    public void Cull (Transform camTransform, float distance, float cameraHalfDiagonalFovDotProduct, int ignoreRate)
    {
        foreach (var type in grassTypes)
        {
            type.Cull(camTransform, distance, cameraHalfDiagonalFovDotProduct, ignoreRate);
        }
    }
}

[System.Serializable]
public class GrassType
{
    public Mesh Mesh;
    public Material Material;
    public GrassDataCollection Collection;

    ComputeShader cullingShader;
    ComputeBuffer grassBuffer;
    ComputeBuffer grassBufferRender;
    ComputeBuffer argsBuffer;

    int kernel;

    uint threadX;

    uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    public void LoadGrassBuffer (ComputeShader shader)
    {
        if(Collection.Count == 0) return;
        grassBuffer = new(Collection.Count, GrassBlade.Size, ComputeBufferType.Structured);
        grassBuffer.SetData(Collection.ToArray());

        grassBufferRender = new(Collection.Count, GrassBlade.Size, ComputeBufferType.Append);

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        cullingShader = Object.Instantiate(shader);

        kernel = cullingShader.FindKernel("Cull");

        cullingShader.SetBuffer(kernel, "_bladeBuffer", grassBuffer);
        cullingShader.SetBuffer(kernel, "_bladeBufferRender", grassBufferRender);
        cullingShader.GetKernelThreadGroupSizes(kernel, out threadX, out _, out _);
    }

    public void Cull (Transform camTransform, float distance, float cameraHalfDiagonalFovDotProduct, int ignoreRate) 
    {
        if(Collection.Count == 0) return;
        grassBufferRender.SetCounterValue(0);

        cullingShader.SetVector("_cameraPosition", camTransform.position);
        cullingShader.SetVector("_cameraForward", camTransform.forward);
        cullingShader.SetFloat("_cameraHalfDiagonalFovDotProduct", cameraHalfDiagonalFovDotProduct);
        cullingShader.SetFloat("_distance", distance);
        cullingShader.SetInt("_ignoreRate", ignoreRate);
        
        int xThreadCount = (int)(Collection.Count / threadX);
        if(xThreadCount > 0) cullingShader.Dispatch(kernel, xThreadCount, 1, 1);

        Material.SetBuffer("_bladeBuffer", grassBufferRender);
    }

    public void Render ()
    {
        if(Collection.Count == 0) return;
        for (int i = 0; i < Mesh.subMeshCount; i++)
        {
            args[0] = Mesh.GetIndexCount(i);
            argsBuffer.SetData( args );
            ComputeBuffer.CopyCount(grassBufferRender, argsBuffer,sizeof(uint));

            Graphics.DrawMeshInstancedIndirect
            (
                Mesh, i, Material,
                new Bounds(Vector3.zero, Vector3.one * 1000),
                argsBuffer
            );
        }
    }

    public void Release ()
    {
        if(grassBuffer is not null) grassBuffer.Release();
        if(grassBufferRender is not null) grassBufferRender.Release();
        if(argsBuffer is not null) argsBuffer.Release();
    }

    public void Reload () 
    {
        Release();
        LoadGrassBuffer(cullingShader);
    }
}