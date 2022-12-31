using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class Test : MonoBehaviour
{
    public RawImage color;
    public RawImage depth;

    public void Update ()
    {
        if(ColorCast.CastRenderTexture != null) color.texture = ColorCast.CastRenderTexture;
        if(ColorCast.DepthRenderTexture != null) depth.texture = ColorCast.DepthRenderTexture;
    }
}