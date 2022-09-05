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

    public void LoadAllBuffers ()
    {
        foreach (var type in grassTypes)
        {
            // ! REMOVE BEFORE PUBLICATION
            if(stressTest) GenerateStressTest(type.Collection);

            type.LoadGrassBuffer();
        }
    }

    public void Render (ComputeBuffer argsBuffer)
    {
        foreach (var type in grassTypes)
        {
            type.Render(argsBuffer);
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
    
    ComputeBuffer grassBuffer;

    public void LoadGrassBuffer ()
    {
        grassBuffer = new(Collection.blades.Length, GrassBlade.Size);
        grassBuffer.SetData(Collection.blades);

        Material.SetBuffer("_bladeBuffer", grassBuffer);
    }

    public void Render (ComputeBuffer argsBuffer)
    {
        for (int i = 0; i < Mesh.subMeshCount; i++)
        {
            argsBuffer.SetData( CreateArgsBuffer( Mesh.GetIndexCount(i), (uint)Collection.blades.Length) );
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
    }

    uint[] CreateArgsBuffer(uint meshIndex, uint count) => new uint[5] { meshIndex, count, 0, 0, 0 };
}