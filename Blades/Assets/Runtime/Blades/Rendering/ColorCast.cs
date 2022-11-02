using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ColorCast
{
    private static Camera castCamera;

    public static Camera CastCamera
    { 
        get
        {
            if(castCamera == null) castCamera = CreateCastCamera();
            return castCamera;
        }

        set => castCamera = value;
    }

    private static RenderTexture castTexture;

    public static RenderTexture CastTexture 
    {
        get
        {
            if(castTexture == null) castTexture = new(256, 256, 1, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            return castTexture;
        }
    }

    private static Texture2D returnTex;

    public static Texture2D ReturnTex 
    {
        get
        {
            if(returnTex == null) returnTex = new(1, 1);
            return returnTex;
        }
    }

    public static ColorCastHit SimpleRay(ColorCastInput input)
    {
        RenderTexture previousRT = RenderTexture.active;

        Graphics.SetRenderTarget(CastTexture);

        Camera previousCamera =  Camera.current;
        Camera.SetupCurrent(null);
        GL.Clear(true, true, Color.black);
        GL.PushMatrix();
        
        GL.LoadOrtho();
        
        if(input.Material.SetPass(0))
        {
            Graphics.DrawMeshNow(input.Mesh, Matrix4x4.TRS(input.Transform.position + input.Ray.origin, input.Transform.rotation, input.Transform.localScale));
        }

        GL.PopMatrix();

        Camera.SetupCurrent(previousCamera);

        ReturnTex.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
        ReturnTex.Apply();

        RenderTexture.active = previousRT;
        
        return new(ReturnTex.GetPixel(0, 0));
    }

    public static ColorCastHit Ray(ColorCastInput input, Vector3 up)
    {
        return new(GetRayTexture(input, up).GetPixel(0, 0));
    }

    public static Texture2D GetRayTexture (ColorCastInput input, Vector3 up)
    {
        RenderTexture previousRT = RenderTexture.active;

        Graphics.SetRenderTarget(CastTexture);

        CommandBuffer cb = new CommandBuffer();

        cb.SetRenderTarget(CastTexture);

        var proj = Matrix4x4.Ortho(-.001f, .001f, -.001f, .001f, -1, 100);

        var view = Matrix4x4.LookAt(input.Ray.origin, input.Ray.origin + input.Ray.direction, up);

        var scaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));

        cb.SetViewProjectionMatrices(scaleMatrix * view.inverse, proj);

        cb.ClearRenderTarget(true, true, Color.black);

        cb.DrawMesh(input.Mesh, input.Transform.localToWorldMatrix, input.Material);

        Graphics.ExecuteCommandBuffer(cb);

        ReturnTex.ReadPixels(new Rect(0, 0, CastTexture.width, CastTexture.height), 0, 0);
        ReturnTex.Apply();

        RenderTexture.active = previousRT;

        return ReturnTex;
    }

    public struct ColorCastHit
    {
        public ColorCastHit (Color color)
        {
            Color = color;
        }

        public Color Color { get; }

        public void Dispose ()
        {
            DisposeCachedElements();
        }
    }

    public struct ColorCastInput
    {
        public ColorCastInput (Ray ray, MeshFilter filter, MeshRenderer renderer)
        {
            Ray = ray;
            Mesh = filter.sharedMesh;
            Material = renderer.sharedMaterial;
            Transform = renderer.transform;
        }

        public Ray Ray { get; }
        public Mesh Mesh { get; }
        public Material Material { get; }
        public Transform Transform { get; }
    }

    public static void DisposeCachedElements ()
    {
        if(CastCamera != null) Object.DestroyImmediate(CastCamera.gameObject);
        if(CastTexture != null) Object.DestroyImmediate(CastTexture);
    }

    static void UpdateCastCamera (Vector3 position, Vector3 direction)
    {
        CastCamera.transform.position = position;
        CastCamera.transform.forward = direction;
    }

    static Camera CreateCastCamera () 
    {
        GameObject camObj = new("castCam");
        Camera camera = camObj.AddComponent<Camera>();

        camera.orthographic = true;
        camera.orthographicSize = .01f;
        camera.enabled = false;
        camera.targetTexture = CastTexture;

        return camera;
    }
}
