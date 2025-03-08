using System;

using UnityEngine;
using UnityEditor;
using System.Reflection;
using Object = UnityEngine.Object;

using UnityEngine.UIElements;

namespace Volpi.Entertainment.SDK.Utilities.Editor
{
    public static class ToolbarCallback
    {
        private static readonly Type _toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static ScriptableObject _currentToolbar;

        public static Action ToolbarGUI;
        public static Action ToolbarGUILeft;
        public static Action ToolbarGUIRight;

        static ToolbarCallback()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (_currentToolbar != null)
            {
                return;
            }

            Object[] toolbars = Resources.FindObjectsOfTypeAll(_toolbarType);
            _currentToolbar = toolbars.Length > 0 ? (ScriptableObject) toolbars[0] : null;

            if (_currentToolbar == null)
            {
                return;
            }

            FieldInfo root = _currentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (root != null)
            {
                object rawRoot = root.GetValue(_currentToolbar);
                VisualElement mRoot = rawRoot as VisualElement;
                    
                RegisterCallback(mRoot,"ToolbarZoneLeftAlign", ToolbarGUILeft);
                RegisterCallback(mRoot,"ToolbarZoneRightAlign", ToolbarGUIRight);
            }
        }

        private static void RegisterCallback(VisualElement mRoot, string root, Action cb)
        {
            VisualElement toolbarZone = mRoot.Q(root);

            VisualElement parent = new()
            {
                style = 
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row
                }
            };
            
            IMGUIContainer container = new();
            container.style.flexGrow = 1;
            container.onGUIHandler += () => 
            {
                cb?.Invoke();
            };
            parent.Add(container);
            toolbarZone.Add(parent);
        }

        // ReSharper disable UnusedMember.Local
        private static void OnGUI()
        {
            Action handler = ToolbarGUI;
            handler?.Invoke();
        }
        // ReSharper restore UnusedMember.Local
    }
}
