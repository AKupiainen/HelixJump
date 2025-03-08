using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace Volpi.Entertaiment.SDK.Localization
{
    public class LocalizationJsonImporter : EditorWindow
    {
        private string _jsonFilePath;
        
        [MenuItem("Tools/Import Localizations from JSON")]
        public static void ShowWindow()
        {
            LocalizationJsonImporter window = GetWindow<LocalizationJsonImporter>(true, "Localization JSON Importer");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Localization JSON Importer", EditorStyles.boldLabel);
            _jsonFilePath = EditorGUILayout.TextField("JSON File Path:", _jsonFilePath);

            if (GUILayout.Button("Select JSON File"))
            {
                string selectedPath = EditorUtility.OpenFilePanel("Select Localization JSON", "Assets", "json");
                
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    _jsonFilePath = selectedPath.Replace(Application.dataPath, "Assets");
                }
            }

            if (GUILayout.Button("Import Localizations") && !string.IsNullOrEmpty(_jsonFilePath))
            {
                ImportLocalizations(_jsonFilePath);
            }
        }

        private static void ImportLocalizations(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
            {
                Debug.LogError("Localization JSON file not found: " + jsonFilePath);
                return;
            }

            string json = File.ReadAllText(jsonFilePath);
            Dictionary<string, Dictionary<string, string>> localizationData = 
                JsonConvert.DeserializeObject<LocalizationDictionary>(json).Data;

            string[] guids = AssetDatabase.FindAssets("t:LocalizationData");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                LocalizationData localizationAsset = AssetDatabase.LoadAssetAtPath<LocalizationData>(path);
                SystemLanguage language = localizationAsset.LocalizationLanguage;
                string langKey = language.ToString();
                
                if (localizationData.TryGetValue(langKey, out Dictionary<string, string> value))
                {
                    UpdateLocalizationAsset(localizationAsset, value);
                    EditorUtility.SetDirty(localizationAsset);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Localization import completed successfully!");
        }
        
        private static void UpdateLocalizationAsset(LocalizationData localizationAsset, Dictionary<string, string> localizedStrings)
        {
            HashSet<string> existingKeys = new();
            
            foreach (LocalizedString entry in localizationAsset.LocalizedStrings)
            {
                existingKeys.Add(entry.Key);
            }

            foreach (KeyValuePair<string, string> kvp in localizedStrings)
            {
                if (!existingKeys.Contains(kvp.Key))
                {
                    localizationAsset.LocalizedStrings.Add(new LocalizedString { Key = kvp.Key, Value = kvp.Value });
                    Debug.Log($"Added new localization: {kvp.Key} -> {kvp.Value}");
                }
            }
        }

        [Serializable]
        private class LocalizationDictionary
        {
            public Dictionary<string, Dictionary<string, string>> Data;
        }
    }
}