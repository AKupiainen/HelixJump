using UnityEngine;
using DG.Tweening;

public class BallSquashStretch : MonoBehaviour
{
    [SerializeField] private float _animationDuration = 0.6f;
    [SerializeField] private float _squashAmount = 0.5f;
    [SerializeField] private float _stretchAmount = 1.5f;

    private Vector3 _originalScale;

    private void Start()
    {
        _originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        DOTween.Kill(this);
    }

    public void AnimateScale()
    {
        Sequence scaleSequence = DOTween.Sequence();

        scaleSequence.Append(transform.DOScale(new Vector3(_originalScale.x * _stretchAmount, _originalScale.y * _squashAmount, _originalScale.z), _animationDuration * 0.3f)
            .SetEase(Ease.OutQuad));

        scaleSequence.Append(transform.DOScale(new Vector3(_originalScale.x * _squashAmount, _originalScale.y * _stretchAmount, _originalScale.z), _animationDuration * 0.4f)
            .SetEase(Ease.InOutQuad));

        scaleSequence.Append(transform.DOScale(_originalScale, _animationDuration * 0.3f)
            .SetEase(Ease.InQuad));

        scaleSequence.SetLoops(-1);
    }
}