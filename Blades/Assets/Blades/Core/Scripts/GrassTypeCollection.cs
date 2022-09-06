using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GrassTypeCollection
{
    // ! REMOVE BEFORE PUBLICATION
    [SerializeField] bool stressTest;
    [SerializeField] int stressTestDensity;
    [SerializeField] GrassType[] grassTypes;

    public void LoadAllBuffers (ComputeShader shader)
    {
        foreach (var type in grassTypes)
        {
            // ! REMOVE BEFORE PUBLICATION
            if(stressTest) GenerateStressTest(type.Collection);

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
    
    void GenerateStressTest (GrassDataCollection collection) 
    {
        float scaleFactor = (1f/stressTestDensity) * 100;
        List<GrassBlade> blades = new();
        for (int x = 0; x < stressTestDensity; x++)
        {
            for (int z = 0; z < stressTestDensity; z++)
            {
                float random = Random.Range(-.5f, .5f);
                GrassBlade? blade = GrassManager.CreateGrassBlade(new Vector3 (x + random, 0, z + random) * scaleFactor, Random.Range(0, 360));
                if(blade is not null) blades.Add(blade.Value); 
            }
        }

        collection.blades = blades.ToArray();
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
        grassBuffer = new(Collection.blades.Length, GrassBlade.Size, ComputeBufferType.Structured);
        grassBuffer.SetData(Collection.blades);

        grassBufferRender = new(Collection.blades.Length, GrassBlade.Size, ComputeBufferType.Append);

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        cullingShader = Object.Instantiate(shader);

        kernel = cullingShader.FindKernel("Cull");

        cullingShader.SetBuffer(kernel, "_bladeBuffer", grassBuffer);
        cullingShader.SetBuffer(kernel, "_bladeBufferRender", grassBufferRender);
        cullingShader.GetKernelThreadGroupSizes(kernel, out threadX, out _, out _);
    }

    public void Cull (Transform camTransform, float distance, float cameraHalfDiagonalFovDotProduct, int ignoreRate) 
    {
        grassBufferRender.SetCounterValue(0);

        cullingShader.SetVector("_cameraPosition", camTransform.position);
        cullingShader.SetVector("_cameraForward", camTransform.forward);
        cullingShader.SetFloat("_cameraHalfDiagonalFovDotProduct", cameraHalfDiagonalFovDotProduct);
        cullingShader.SetFloat("_distance", distance);
        cullingShader.SetInt("_ignoreRate", ignoreRate);

        cullingShader.Dispatch(kernel, (int)(Collection.blades.Length / threadX), 1, 1);

        Material.SetBuffer("_bladeBuffer", grassBufferRender);
    }

    public void Render ()
    {
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
        grassBuffer.Release();
        grassBufferRender.Release();
        argsBuffer.Release();
    }
}