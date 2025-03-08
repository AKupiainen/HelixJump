using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Volpi.Entertainment.SDK.Utilities.Editor
{
    public class TextToTextMeshProConverter
    {
        [MenuItem("Tools/Replace Text Component With Text Mesh Pro", true)]
        static bool TextSelectedValidation()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            
            if (selectedObjects.Length == 0)
            {
                return false;
            }

            foreach (GameObject selectedObject in selectedObjects)
            {
                Text text = selectedObject.GetComponent<Text>();
                
                if (!text)
                {
                    return false;
                }
            }

            return true;
        }

        [MenuItem("Tools/Replace Text Component With Text Mesh Pro")]
        static void ReplaceSelectedObjects()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            Undo.RecordObjects(selectedObjects, "Replace Text Component with Text Mesh Pro Component");

            foreach (GameObject selectedObject in selectedObjects)
            {
                Text textComp = selectedObject.GetComponent<Text>();
                Vector2 textSizeDelta = textComp.rectTransform.sizeDelta;
                Undo.DestroyObjectImmediate(textComp);

                TextMeshProUGUI temp = Undo.AddComponent<TextMeshProUGUI>(selectedObject);

                temp.text = textComp.text;
                temp.fontSize = textComp.fontSize;

                FontStyle fontStyle = textComp.fontStyle;
                
                switch (fontStyle)
                {
                    case FontStyle.Normal:
                        temp.fontStyle = FontStyles.Normal;
                        break;
                    case FontStyle.Bold:
                        temp.fontStyle = FontStyles.Bold;
                        break;
                    case FontStyle.Italic:
                        temp.fontStyle = FontStyles.Italic;
                        break;
                    case FontStyle.BoldAndItalic:
                        temp.fontStyle = FontStyles.Bold | FontStyles.Italic;
                        break;
                }

                temp.enableAutoSizing = textComp.resizeTextForBestFit;
                temp.fontSizeMin = textComp.resizeTextMinSize;
                temp.fontSizeMax = textComp.resizeTextMaxSize;

                TextAnchor alignment = textComp.alignment;
                
                switch (alignment)
                {
                    case TextAnchor.UpperLeft:
                        temp.alignment = TextAlignmentOptions.TopLeft;
                        break;
                    case TextAnchor.UpperCenter:
                        temp.alignment = TextAlignmentOptions.Top;
                        break;
                    case TextAnchor.UpperRight:
                        temp.alignment = TextAlignmentOptions.TopRight;
                        break;
                    case TextAnchor.MiddleLeft:
                        temp.alignment = TextAlignmentOptions.MidlineLeft;
                        break;
                    case TextAnchor.MiddleCenter:
                        temp.alignment = TextAlignmentOptions.Midline;
                        break;
                    case TextAnchor.MiddleRight:
                        temp.alignment = TextAlignmentOptions.MidlineRight;
                        break;
                    case TextAnchor.LowerLeft:
                        temp.alignment = TextAlignmentOptions.BottomLeft;
                        break;
                    case TextAnchor.LowerCenter:
                        temp.alignment = TextAlignmentOptions.Bottom;
                        break;
                    case TextAnchor.LowerRight:
                        temp.alignment = TextAlignmentOptions.BottomRight;
                        break;
                }

                temp.enableWordWrapping = textComp.horizontalOverflow == HorizontalWrapMode.Wrap;

                temp.color = textComp.color;
                temp.raycastTarget = textComp.raycastTarget;
                temp.richText = textComp.supportRichText;
                temp.rectTransform.sizeDelta = textSizeDelta;
            }
        }
    }
}