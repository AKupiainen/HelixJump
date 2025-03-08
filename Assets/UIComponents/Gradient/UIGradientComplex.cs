using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UiComponents.Gradient
{
    [AddComponentMenu("UI/Effects/UI ComplexGradient")]
    [RequireComponent(typeof(RectTransform))]
    public class UIGradientTexture : BaseMeshEffect
    {
        [SerializeField] private UIGradientBlendMode _blendMode = UIGradientBlendMode.Multiply;

        [SerializeField] [Range(0, 1)] private float _intensity = 1f;

        [SerializeField]
        private UnityEngine.Gradient _gradient = new();

        [SerializeField] [Range(0, 360)] private float _rotation;
        private RectTransform RectTransform => transform as RectTransform;
        
        public override void ModifyMesh(VertexHelper vh)
        {
            Rect rect = RectTransform.rect;
            UIVertex vert = new();
            
            CutMesh(vh);

            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vert, i);
                Vector2 normalizedPosition = ((Vector2)vert.position - rect.min) / (rect.max - rect.min);

                normalizedPosition = RotateNormalizedPosition(normalizedPosition, _rotation);

                Color gradientColor = _gradient.Evaluate(normalizedPosition.y);
                vert.color= BlendColor(vert.color, gradientColor, _blendMode, _intensity);
                vh.SetUIVertex(vert, i);
            }
        }

        private void CutMesh(VertexHelper vh)
        {
            List<UIVertex> tris = new();
            vh.GetUIVertexStream(tris);
            vh.Clear();

            List<UIVertex> list = new();
            Vector2 cutDirection = GetCutDirection();

            IEnumerable<float> cuts = _gradient.alphaKeys.Select(x => x.time).Union(_gradient.colorKeys.Select(x => x.time));

            foreach (float cut in cuts)
            {
                list.Clear();
                Vector2 cutOrigin = GetCutOrigin(cut);

                if (cut < 0.001f || cut > 0.999f)
                {
                    continue;
                }
                else
                {
                    for (int j = 0; j < tris.Count; j += 3)
                    {
                        CutTriangle(tris, j, list, cutDirection, cutOrigin);
                    }
                }
                tris.Clear();
                tris.AddRange(list);
            }
            vh.AddUIVertexTriangleStream(tris);
        }

        private UIVertex UIVertexLerp(UIVertex v1, UIVertex v2, float t)
        {
            UIVertex vert = new()
            {
                position = Vector3.Lerp(v1.position, v2.position, t),
                color = Color.Lerp(v1.color, v2.color, t),
                uv0 = Vector2.Lerp(v1.uv0, v2.uv0, t),
                uv1 = Vector2.Lerp(v1.uv1, v2.uv1, t),
                uv2 = Vector2.Lerp(v1.uv2, v2.uv2, t),
                uv3 = Vector2.Lerp(v1.uv3, v2.uv3, t)
            };
            return vert;
        }

        private Vector2 GetCutDirection()
        {
            Vector2 direction = Vector2.up.Rotate(-_rotation);
            Rect rect = RectTransform.rect;
            
            direction = new Vector2(direction.x / rect.size.x, direction.y / rect.size.y);
            return direction.Rotate(90);
        }

        private void CutTriangle(List<UIVertex> tris, int idx, List<UIVertex> list, Vector2 cutDirection, Vector2 cutOrigin)
        {
            UIVertex a = tris[idx];
            UIVertex b = tris[idx + 1];
            UIVertex c = tris[idx + 2];

            float bc = OnLine(b.position, c.position, cutOrigin, cutDirection);
            float ab = OnLine(a.position, b.position, cutOrigin, cutDirection);
            float ca = OnLine(c.position, a.position, cutOrigin, cutDirection);

            if (IsOnLine(ab))
            {
                if (IsOnLine(bc))
                {
                    UIVertex pab = UIVertexLerp(a, b, ab);
                    UIVertex pbc = UIVertexLerp(b, c, bc);
                    list.AddRange(new List<UIVertex> { a, pab, c, pab, pbc, c, pab, b, pbc });
                }
                else
                {
                    UIVertex pab = UIVertexLerp(a, b, ab);
                    UIVertex pca = UIVertexLerp(c, a, ca);
                    list.AddRange(new List<UIVertex> { c, pca, b, pca, pab, b, pca, a, pab });
                }
            }
            else if (IsOnLine(bc))
            {
                UIVertex pbc = UIVertexLerp(b, c, bc);
                UIVertex pca = UIVertexLerp(c, a, ca);
                list.AddRange(new List<UIVertex> { b, pbc, a, pbc, pca, a, pbc, c, pca });
            }
            else
            {
                list.AddRange(tris.GetRange(idx, 3));
            }
        }

        private bool IsOnLine(float t)
        {
            return t is <= 1f and > 0f;
        }
        
        private float OnLine(Vector2 p1, Vector2 p2, Vector2 o, Vector2 dir)
        {
            float tmp = (p2.x - p1.x) * dir.y - (p2.y - p1.y) * dir.x;
            if (tmp == 0)
            {
                return -1f;
            }
            float mu = ((o.x - p1.x) * dir.y - (o.y - p1.y) * dir.x) / tmp;
            return mu;
        }

        private Vector2 GetCutOrigin(float t)
        {
            Vector2 direction = Vector2.up.Rotate(-_rotation);
            Rect rect = RectTransform.rect;
            direction = new Vector2(direction.x / rect.size.x, direction.y / rect.size.y);

            Vector3 p1;
            Vector3 p2;

            if (_rotation % 180 < 90)
            {
                p1 = Vector3.Project(Vector2.Scale(RectTransform.rect.size, Vector2.down + Vector2.left) * 0.5f, direction);
                p2 = Vector3.Project(Vector2.Scale(RectTransform.rect.size, Vector2.up + Vector2.right) * 0.5f, direction);
            }
            else
            {
                p1 = Vector3.Project(Vector2.Scale(RectTransform.rect.size, Vector2.up + Vector2.left) * 0.5f, direction);
                p2 = Vector3.Project(Vector2.Scale(RectTransform.rect.size, Vector2.down + Vector2.right) * 0.5f, direction);
            }
            return _rotation % 360 >= 180
                ? Vector2.Lerp(p2, p1, t) + RectTransform.rect.center
                : Vector2.Lerp(p1, p2, t) + RectTransform.rect.center;
        }

        private Color BlendColor(Color c1, Color c2, UIGradientBlendMode mode, float intensity)
        {
            return mode switch
            {
                UIGradientBlendMode.Override => Color.Lerp(c1, c2, intensity),
                UIGradientBlendMode.Multiply => Color.Lerp(c1, c1 * c2, intensity),
                _ => c1
            };
        }

        private Vector2 RotateNormalizedPosition(Vector2 normalizedPosition, float angle)
        {
            float a = Mathf.Deg2Rad * (angle < 0 ? (angle % 90 + 90) : (angle % 90));
            float scale = Mathf.Sin(a) + Mathf.Cos(a);

            return (normalizedPosition - Vector2.one * 0.5f).Rotate(angle) / scale + Vector2.one * 0.5f;
        }

        private enum UIGradientBlendMode
        {
            Override,
            Multiply
        }
    }

    public static class Vector2Extension
    {
        public static Vector2 Rotate(this Vector2 v, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = v.x;
            float ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }
    }
}