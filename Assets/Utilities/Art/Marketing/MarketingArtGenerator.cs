using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;

namespace Volpi.Entertainment.SDK.Utilities
{
    public class MarketingArtGenerator : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private MarketingArtConfig[] _marketingArtConfig;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text _localizedText;
        [SerializeField] private CanvasScaler _canvasScaler;
        [SerializeField] private Image _background;
        [SerializeField] private Image _foreground;
        [SerializeField] private float _textMargin = 50f;
        [SerializeField] private Vector2 _backgroundOffset = Vector2.zero;

        private void Start()
        {
            if (_marketingArtConfig == null || _marketingArtConfig.Length == 0 || _canvasScaler == null || _background == null)
            {
                Debug.LogError("Missing required components.");
                return;
            }

            StartCoroutine(GenerateMarketingArt());
        }

        private IEnumerator GenerateMarketingArt()
        {
            foreach (MarketingArtConfig config in _marketingArtConfig)
            {
                foreach (Sprite sprite in config.Backgrounds)
                {
                    Vector2Int spriteOriginSize = new((int)sprite.rect.size.x, (int)sprite.rect.size.y);
                    
                    ResizeCanvas(spriteOriginSize);
                    SetBackground(sprite);
                    SetForeground();

                    foreach (MarketingLocalizationData localizedText in config.LocalizedTexts)
                    {
                        SetLocalizedText(localizedText.LocalizedValue);

                        yield return new WaitForEndOfFrame();

                        CaptureScreenshot(spriteOriginSize, sprite, localizedText.Language.ToString());
                    }
                }
            }
        }
        
        private void SetForeground()
        {
            _foreground.rectTransform.anchoredPosition = new Vector2(0, -_textMargin);
        }

        private void SetLocalizedText(string localizedText)
        {
            _localizedText.text = localizedText;
            _localizedText.fontSize = Mathf.Clamp(50 * (_canvasScaler.referenceResolution.x / 1080f), 20, 60); 
        }

        private void SetBackground(Sprite background)
        {
            _background.sprite = background;
            _background.rectTransform.anchoredPosition = _backgroundOffset;
        }

        private void ResizeCanvas(Vector2Int resolution)
        {
            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvasScaler.referenceResolution = resolution;

            _background.rectTransform.sizeDelta = new Vector2(resolution.x, resolution.y);
        }

        private void CaptureScreenshot(Vector2Int resolution, Sprite sprite, string language)
        {
            string directoryPath = "MarketingArt";
            Directory.CreateDirectory(directoryPath);

            string spriteHashCode = sprite.GetHashCode().ToString("X");
            string fileName = $"MarketingArt_{resolution.x}x{resolution.y}_{spriteHashCode}_{language}.png";
            string filePath = Path.Combine(directoryPath, fileName);

            ScreenCapture.CaptureScreenshot(filePath);
            Debug.Log($"Screenshot saved: {filePath}");
        }
    }
}