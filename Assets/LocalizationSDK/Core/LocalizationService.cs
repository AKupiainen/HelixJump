using System;
using System.Collections.Generic;
using UnityEngine;
using Volpi.Entertaiment.SDK.Utilities;

namespace Volpi.Entertaiment.SDK.Localization
{
    public class LocalizationService : Singleton<LocalizationService>, ILocalizationService
    {
        public delegate void OnLanguageChangedDelegate(SystemLanguage localizationLanguage);
        public static event OnLanguageChangedDelegate OnLanguageChangedCallback;
        
        private readonly Dictionary<string, string> _localizedData = new();
        
        private SystemLanguage _currentLanguage;

        [SerializeField] private List<LocalizationData> _localizationDatas;

        public void Initialize(SystemLanguage localizationLanguage)
        {
            _localizedData.Clear();

            LocalizationData localizationData = _localizationDatas.Find(data => data.LocalizationLanguage == localizationLanguage);

            if (localizationData != null)
            {
                foreach (LocalizedString localizedString in localizationData.LocalizedStrings)
                {
                    _localizedData[localizedString.Key] = localizedString.Value;
                }
                
                OnLanguageChangedCallback?.Invoke(localizationLanguage);
            }
        }

        public string GetLocalizedValue(LocString key)
        {
            LocString keyString = key ?? throw new ArgumentNullException(nameof(key));
            return GetLocalizedValue(keyString);
        }
        
        public string FormatLocalizedValue(string key, params object[] args)
        {
            string localizedString = GetLocalizedValue(key);
            return string.Format(localizedString, args);
        }
        
        public string GetLocalizedValue(string key)
        {
            return _localizedData.GetValueOrDefault(key, key);
        }

        public void SetLanguage(SystemLanguage language)
        {
            _currentLanguage = GetSupportedLanguage(language, _localizationDatas);
            Initialize(language);

            static SystemLanguage GetSupportedLanguage(SystemLanguage language, List<LocalizationData> localizationDatas)
            {
                LocalizationData foundData = localizationDatas.Find(data => data.LocalizationLanguage == language);
                return foundData != null ? foundData.LocalizationLanguage : SystemLanguage.English;
            }
        }

        public List<LocalizationData> LocalizationDatas => _localizationDatas;

        public SystemLanguage GetCurrentLanguage => _currentLanguage;
    }
}