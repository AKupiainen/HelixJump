using System;
using UnityEngine;
using System.Collections.Generic;

namespace Volpi.Entertainment.SDK.Utilities
{
    [CreateAssetMenu(fileName = "MarketingArtConfig", menuName = "Marketing Art Config", order = 1)]
    public class MarketingArtConfig : ScriptableObject
    {
        [SerializeField]
        private Sprite[] _backgrounds;

        [Header("Localization Settings")]
        [SerializeField] private List<MarketingLocalizationData> _localizedTexts = new(); 
        public List<MarketingLocalizationData> LocalizedTexts => _localizedTexts;
        public Sprite[] Backgrounds => _backgrounds;
    }

    [Serializable]
    public class MarketingLocalizationData
    {
        [SerializeField] private SystemLanguage _systemLanguage;
        [SerializeField] private string _localizedValue;

        public SystemLanguage Language => _systemLanguage;
        public string LocalizedValue => _localizedValue;
    }
}