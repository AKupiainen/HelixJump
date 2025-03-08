using UnityEngine;
using UnityEngine.UI;

namespace UiComponents.Gradient 
{
    [AddComponentMenu("UI/Effects/Gradient")]
    public class UIGradient : BaseMeshEffect
    {
        [SerializeField] private Color _firstColor = Color.white;
        [SerializeField] private Color _secondColor = Color.white;
        [Range(-180f, 180f)]
        [SerializeField] private float _angle;
        [SerializeField] private bool _ignoreRatio = true;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
            {
                return;
            }

            Rect rect = graphic.rectTransform.rect;
            Vector2 dir = UIGradientUtils.RotationDir(_angle);

            if (!_ignoreRatio)
            {
                dir = UIGradientUtils.CompensateAspectRatio(rect, dir);
            }
            
            UIGradientUtils.Matrix2X3 localPositionMatrix = UIGradientUtils.LocalPositionMatrix(rect, dir);
            UIVertex vertex = default;
            
            for (int i = 0; i < vh.currentVertCount; i++) 
            {
                vh.PopulateUIVertex (ref vertex, i);
                Vector2 localPosition = localPositionMatrix * vertex.position;
                vertex.color *= Color.Lerp(_secondColor, _firstColor, localPosition.y);
                vh.SetUIVertex (vertex, i);
            }
        }
    }
}
