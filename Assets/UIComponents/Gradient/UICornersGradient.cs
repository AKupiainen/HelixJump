using UnityEngine;
using UnityEngine.UI;

namespace UiComponents.Gradient
{
    [AddComponentMenu("UI/Effects/4 Corners Gradient")]
    public class UICornersGradient : BaseMeshEffect 
    {
        [SerializeField] private Color _topLeftColor = Color.white;
        [SerializeField] private Color _topRightColor = Color.white;
        [SerializeField] private Color _bottomRightColor = Color.white;
        [SerializeField] private Color _bottomLeftColor = Color.white;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
            {
                return;
            }

            Rect rect = graphic.rectTransform.rect;
            UIGradientUtils.Matrix2X3 localPositionMatrix = UIGradientUtils.LocalPositionMatrix(rect, Vector2.right);

            UIVertex vertex = default;

            for (int i = 0; i < vh.currentVertCount; i++) 
            {
                vh.PopulateUIVertex(ref vertex, i);
                Vector2 normalizedPosition = localPositionMatrix * vertex.position;
                vertex.color *= UIGradientUtils.Bilerp(_bottomLeftColor, _bottomRightColor, _topLeftColor, _topRightColor, normalizedPosition);
                vh.SetUIVertex(vertex, i);
            }
        }
    }
}