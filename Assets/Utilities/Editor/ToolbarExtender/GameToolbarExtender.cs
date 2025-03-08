using System;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Volpi.Entertainment.SDK.Utilities.Editor
{
    [InitializeOnLoad]
    public class GameToolbarExtender
    {
        private static string _restoreEditingLevelAfterPlayMode;
        private static string[] _sceneNames;
        private static string[] _scenePaths;
        private static int _previousSceneIndex = -1;
        private static bool _saveDataCleared;
        
        private static RecorderController _recorderController;

        static GameToolbarExtender()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnRightToolbarGUI);
            ToolbarExtender.LeftToolbarGUI.Add(OnLeftToolbarGUI);

            EditorBuildSettings.sceneListChanged += UpdateSceneList;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;

            UpdateSceneList();
        }

        private static void OnRightToolbarGUI()
        {
            using (new EditorGUI.DisabledScope(_saveDataCleared))
            {
                if (GUILayout.Button("Clear Save"))
                {
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.Save();
                    
                    _saveDataCleared = true;
                }
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Open Scene", GUILayout.ExpandWidth(false));

            if (GUILayout.Button(
                    _sceneNames.Length > 0
                        ? GetActiveSceneName()
                        : "No Scenes",
                    EditorStyles.popup,
                    GUILayout.MaxWidth(150f)))
            {
                GUIContent[] sceneOptions = _sceneNames.Select(name => new GUIContent(name)).ToArray();

                EditorUtility.DisplayCustomMenu(new Rect(Event.current.mousePosition, Vector2.zero),
                    sceneOptions,
                    GetActiveSceneIndex(),
                    (_, _, selected) =>
                    {
                        if (selected != -1 && selected != _previousSceneIndex)
                        {
                            OpenSceneInEditor(_scenePaths[selected]);
                            _previousSceneIndex = selected;
                        }
                    },
                    null);
            }

            EditorGUILayout.EndHorizontal();
        }
        
        private static string GetActiveSceneName()
        {
            string activeScenePath = SceneManager.GetActiveScene().path;
            int activeSceneIndex = Array.IndexOf(_scenePaths, activeScenePath);

            if (activeSceneIndex >= 0 && activeSceneIndex < _sceneNames.Length)
            {
                return _sceneNames[activeSceneIndex];
            }

            return "Unknown Scene";
        }

        private static int GetActiveSceneIndex()
        {
            string activeScenePath = SceneManager.GetActiveScene().path;
            int activeSceneIndex = Array.IndexOf(_scenePaths, activeScenePath);

            if (activeSceneIndex >= 0 && activeSceneIndex < _scenePaths.Length)
            {
                return activeSceneIndex;
            }

            return -1;
        }

        private static void OnLeftToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            Texture recordTexture = GetRecordTexture();
            GUIContent recordButtonContent = new(recordTexture);

            using (new EditorGUI.DisabledScope(!Application.isPlaying || _recorderController != null && _recorderController.IsRecording()))
            {
                if (GUILayout.Button(recordButtonContent, GUILayout.MaxWidth(25)))
                {
                    StartRecording();
                }
            }

            if (GUILayout.Button("Compile"))
            {
                AssetDatabase.Refresh();
                CompilationPipeline.RequestScriptCompilation();
                EditorUtility.RequestScriptReload();
            }
        }

        private static Texture GetRecordTexture()
        {
            return EditorGUIUtility.IconContent("Animation.Record").image;
        }

        private static void PlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (_recorderController != null && _recorderController.IsRecording())
                {
                    _recorderController.StopRecording();
                }

                RestoreLevelAfterPlayMode();
            }
        }

        private static void RestoreLevelAfterPlayMode()
        {
            if (!string.IsNullOrEmpty(_restoreEditingLevelAfterPlayMode))
            {
                EditorSceneManager.OpenScene(_restoreEditingLevelAfterPlayMode, OpenSceneMode.Single);
            }

            _restoreEditingLevelAfterPlayMode = null;
        }

        private static void UpdateSceneList()
        {
            _scenePaths = EditorBuildSettings.scenes
                .Where(scene => scene.enabled && File.Exists(scene.path)) 
                .Select(scene => scene.path)
                .ToArray();

            _sceneNames = _scenePaths
                .Where(path => !string.IsNullOrEmpty(path)) 
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();
        }

        private static void OpenSceneInEditor(string scene)
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scene);
            }
        }

        private static void StartRecording()
        {
            RecorderControllerSettings controllerSettings =
                ScriptableObject.CreateInstance<RecorderControllerSettings>();
            
            _recorderController = new RecorderController(controllerSettings);
            
            DirectoryInfo mediaOutputFolder = new(Path.Combine(Application.dataPath, "..", "SampleRecordings"));

            MovieRecorderSettings movieRecorderSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            movieRecorderSettings.name = "My Video Recorder";
            movieRecorderSettings.Enabled = true;

            movieRecorderSettings.EncoderSettings = new CoreEncoderSettings
            {
                EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High,
                Codec = CoreEncoderSettings.OutputCodec.MP4
            };

            movieRecorderSettings.CaptureAlpha = true;
            
            PlayModeWindow.GetRenderingResolution(out var width, out var height);

            movieRecorderSettings.ImageInputSettings = new GameViewInputSettings
            {
                OutputWidth = (int)width,
                OutputHeight = (int)height
            };

            string currentDate = DateTime.Now.ToString("yyyy.MM.dd");
            string currentTime = DateTime.Now.ToString("HH.mm.ss");
            string outputFile = $"{mediaOutputFolder.FullName}/video_{currentDate}_{currentTime}";

            movieRecorderSettings.OutputFile = outputFile;

            controllerSettings.AddRecorderSettings(movieRecorderSettings);
            controllerSettings.SetRecordModeToManual();
            controllerSettings.FrameRate = 60.0f;

            RecorderOptions.VerboseMode = false;
            
            _recorderController.PrepareRecording();
            _recorderController.StartRecording();
        }
    }
}