namespace Volpi.Entertaiment.SDK.Localization
{
    using TMPro;
    using UnityEngine;

    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string _localizationKey;

        private TextMeshProUGUI _textComponent;

        private void Awake()
        {
            _textComponent = GetComponent<TextMeshProUGUI>();
            
            if (_textComponent == null)
            {
                Debug.LogError("LocalizedText script must be attached to a GameObject with a TextMeshProUGUI component.");
            }
        }

        private void OnEnable()
        {
            LocalizationService.OnLanguageChangedCallback += UpdateText;
            UpdateText(LocalizationService.Instance.GetCurrentLanguage);
        }

        private void OnDisable()
        {
            LocalizationService.OnLanguageChangedCallback -= UpdateText;
        }

        private void UpdateText(SystemLanguage language)
        {
            if (_textComponent != null)
            {
                string localizedValue = LocalizationService.Instance.GetLocalizedValue(_localizationKey);
                _textComponent.text = localizedValue;
            }
        }
    }
}

