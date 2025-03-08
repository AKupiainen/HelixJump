namespace UiComponents.Gradient
{
    using UnityEngine;

    public static class UIGradientUtils
    {
        public struct Matrix2X3
        {
            public float M00, M01, M02;
            public float M10, M11, M12;

            public static Vector2 operator *(Matrix2X3 m, Vector2 v)
            {
                return new Vector2(
                    m.M00 * v.x + m.M01 * v.y + m.M02,
                    m.M10 * v.x + m.M11 * v.y + m.M12
                );
            }
        }
        
        static readonly Vector2[] _verticesPositions = { Vector2.up, Vector2.one, Vector2.right, Vector2.zero };
        public static Vector2[] VerticePositions => _verticesPositions;

        public static Color Bilerp(Color bottomLeft, Color bottomRight, Color topLeft, Color topRight, Vector2 position)
        {
            Color top = Color.Lerp(topLeft, topRight, position.x);
            Color bottom = Color.Lerp(bottomLeft, bottomRight, position.x);
            return Color.Lerp(bottom, top, position.y);
        }

        public static Matrix2X3 LocalPositionMatrix(Rect rect, Vector2 direction)
        {
            float cos = direction.x;
            float sin = direction.y;

            float rectX = rect.xMin;
            float rectY = rect.yMin;
            float width = rect.width;
            float height = rect.height;

            Matrix2X3 result;
            result.M00 = cos / width;
            result.M01 = -sin / height;
            result.M02 = (rectX * sin - rectY * cos) / width;

            result.M10 = sin / width;
            result.M11 = cos / height;
            result.M12 = (-rectX * cos - rectY * sin) / height;

            return result;
        }
        
        public static Vector2 RotationDir(float angle)
        {
            float radians = angle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        // Compensates for the aspect ratio of the rectTransform
        public static Vector2 CompensateAspectRatio(Rect rect, Vector2 direction)
        {
            float aspectRatio = rect.width / rect.height;
            return new Vector2(direction.x / aspectRatio, direction.y);
        }
    }
}