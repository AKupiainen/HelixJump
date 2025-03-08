using System;
using System.Collections.Generic;
using UnityEngine;

namespace Volpi.Entertaiment.SDK.Localization
{
    [Serializable]
    public class LocalizedString
    {
        [SerializeField] private string _key;
        [SerializeField] private string _value;

        public string Key
        {
            get => _key;
            set => _key = value;
        }

        public string Value
        {
            get => _value;
            set => _value = value;
        }
    }

    [CreateAssetMenu(fileName = "LocalizationData", menuName = "Localization/Localization Data")]
    public class LocalizationData : ScriptableObject
    {
        [SerializeField] private SystemLanguage _localizationLanguage;
        [SerializeField] private List<LocalizedString> _localizedStrings;

        public List<LocalizedString> LocalizedStrings => _localizedStrings;
        public SystemLanguage LocalizationLanguage => _localizationLanguage;
    }
}