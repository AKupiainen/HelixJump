using System;

namespace Volpi.Entertaiment.SDK.Localization
{
    public class LocString
    {
        private string Key { get; }

        public LocString(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            Key = key;
        }

        public static implicit operator string(LocString locString)
        {
            if (locString == null)
            {
                return null;
            }

            return LocalizationService.Instance.GetLocalizedValue(locString.Key);
        }

        public override string ToString()
        {
            return LocalizationService.Instance.GetLocalizedValue(Key);
        }
    }
}