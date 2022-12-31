using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ColorCast
{
    private static MeshRenderer[] rendererCache;

    private static RenderTexture castRenderTexture;

    public static RenderTexture CastRenderTexture 
    {
        get
        {
            if(castRenderTexture == null) castRenderTexture = new(16, 16, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            return castRenderTexture;
        }
    }

    private static Texture2D castTexture;

    public static Texture2D CastTexture
    {
        get
        {
            if(castTexture == null) castTexture = new(16, 16);
            return castTexture;
        }
    }

    private static RenderTexture depthRenderTexture;

    public static RenderTexture DepthRenderTexture 
    {
        get
        {
            if(depthRenderTexture == null) depthRenderTexture = new(16, 16, 32, RenderTextureFormat.Depth);
            return depthRenderTexture;
        }
    }

    public static void UpdateRendererCache () => rendererCache = Object.FindObjectsOfType<MeshRenderer>();

    public static bool Ray (Ray ray, Vector3 up, out RenderRaycastOut? rayOut)
    {
        if(rendererCache == null) UpdateRendererCache();

        rayOut = null;

        foreach (var renderer in rendererCache)
        {
            if(!renderer.isVisible) continue;
            if(renderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly) continue;
            if(!renderer.bounds.IntersectRay(ray)) continue;

            MeshFilter filter = renderer.GetComponent<MeshFilter>();

            var (color, depth) = GetRayTexture(new ColorCastInput(ray, filter, renderer, 100f), up);

            float depthWorld = depth.w;

            Vector3 hitPoint = ray.origin + (ray.direction * depthWorld);

            if(depthWorld == 0) continue;

            if(rayOut?.Length < depthWorld) continue;

            rayOut = new(renderer, filter, color, depthWorld, hitPoint, new(depth.x, depth.y, depth.z));
            if(rayOut != null) Debug.DrawRay(rayOut.Value.Point, rayOut.Value.Normal, rayOut.Value.Color);
        }

        return rayOut != null;
    }

    public static (Color color, Vector4 depth) GetRayTexture (ColorCastInput input, Vector3 up)
    {
        RenderTexture previousRT = RenderTexture.active;

        Graphics.SetRenderTarget(CastRenderTexture);

        CommandBuffer cb = new CommandBuffer();

        cb.SetRenderTarget(CastRenderTexture, DepthRenderTexture);

        var proj = Matrix4x4.Ortho(-.05f, .05f, -.05f, .05f, .01f, input.Max);

        var view = Matrix4x4.LookAt(input.Ray.origin, input.Ray.origin + input.Ray.direction, up);

        var scaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));

        var camMatrix = scaleMatrix * view.inverse;

        cb.SetViewProjectionMatrices(camMatrix, proj);

        var inverse = GL.GetGPUProjectionMatrix(proj, false);

        cb.ClearRenderTarget(true, true, Color.black);

        for (int i = 0; i < input.Mesh.subMeshCount; i++)
        {
            cb.DrawMesh(input.Mesh, input.Transform.localToWorldMatrix, input.Materials[i], i);
        }

        Graphics.ExecuteCommandBuffer(cb);

        CastTexture.ReadPixels(new Rect(0, 0, CastRenderTexture.width, CastRenderTexture.height), 0, 0);
        CastTexture.Apply();

        RenderTexture.active = previousRT;

        Vector4 depth = GetDepth(inverse, input.Max, .01f);

        return (CastTexture.GetPixel(0, 0), depth);
    }

    static Vector4 GetDepth (Matrix4x4 inverseCamMatrix, float farPlane, float nearPlane)
    {
        ComputeShader depthShader = (ComputeShader)Resources.Load("Shaders/ComputeShader/ColorCastDepth");

        int kernel = depthShader.FindKernel("DepthCalc");

        depthShader.SetTexture(kernel, "DepthTexture", DepthRenderTexture);
        depthShader.SetFloat("farPlane", farPlane);
        depthShader.SetFloat("nearPlane", nearPlane);
        depthShader.SetMatrix("cameraInvProjection", inverseCamMatrix);

        ComputeBuffer buffer = new(1, sizeof(float) * 4);
        depthShader.SetBuffer(kernel, "depth", buffer);

        depthShader.Dispatch(kernel, 1, 1, 1);

        Vector4[] output = new Vector4[1];
        buffer.GetData(output);
        buffer.Release();

        //Debug.Log(output[0]);

        return output[0];
    }

    public struct RenderRaycastOut
    {
        public RenderRaycastOut(MeshRenderer renderer, MeshFilter filter, Color color, float length, Vector3 point, Vector3 normal)
        {
            Renderer = renderer;
            Filter = filter;
            Color = color;
            Length = length;
            Point = point;
            Normal = normal;
        }

        public MeshRenderer Renderer { get; }
        public MeshFilter Filter { get; }
        public Color Color { get; }
        public Vector3 Point { get; }
        public Vector3 Normal { get; }
        public float Length { get; }
    }

    public struct ColorCastInput
    {
        public ColorCastInput (Ray ray, MeshFilter filter, MeshRenderer renderer, float max)
        {
            Ray = ray;
            Mesh = filter.sharedMesh;
            Materials = renderer.materials;
            Transform = renderer.transform;
            Max = max;
        }

        public Ray Ray { get; }
        public Mesh Mesh { get; }
        public Material[] Materials { get; }
        public Transform Transform { get; }
        public float Max { get; }
    }

    public static void DisposeCachedElements ()
    {
        if(CastRenderTexture != null) Object.DestroyImmediate(CastRenderTexture);
    }
}