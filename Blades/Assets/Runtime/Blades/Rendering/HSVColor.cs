using UnityEngine;

namespace Blades
{
    public struct HSVColor
    {
        public HSVColor (float h, float s, float v, float a = 1)
        {
            H = h;
            S = s;
            V = v;
            A = a;
        }

        public HSVColor (Color color)
        {
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);

            H = h;
            S = s;
            V = v;
            A = color.a;
        }

        public float H { get; set; }
        public float S { get; set; }
        public float V { get; set; }
        public float A { get; set; }

        public static HSVColor Lerp (HSVColor a, HSVColor b, float t)
        {
            HSVColor hsvColor = new()
            {
                H = Mathf.LerpAngle(a.H, b.H, t),
                S = Mathf.Lerp(a.S, b.S, t),
                V = Mathf.Lerp(a.V, b.V, t),
                A = Mathf.Lerp(a.A, b.A, t)
            };

            return hsvColor;
        }

        public Color ToColor()
        {
            Color color = Color.HSVToRGB(H, S, V, true);
            color.a = A;
            return color;
        }
    }
}