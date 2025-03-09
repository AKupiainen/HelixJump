using UnityEngine;
using DG.Tweening;

public class SplashDestroyer : MonoBehaviour
{
    [SerializeField] private float _fadeDuration = 2f; // Duration of the fade
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        FadeOutAndDestroy();
    }

    private void FadeOutAndDestroy()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.DOFade(0f, _fadeDuration).OnComplete(DestroyObject);
        }
    }

    private void DestroyObject()
    {
        Destroy(gameObject);
    }
}