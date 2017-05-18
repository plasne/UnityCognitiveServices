using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Linq;

[CustomEditor(typeof(VisionQR))]
[CanEditMultipleObjects]
public class VisionQREditor : Editor
{
    SerializedProperty textureProperty;

    private string text;

    void OnEnable()
    {
        textureProperty = serializedObject.FindProperty("texture");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(textureProperty, new GUIContent("Texture"));

        if (GUILayout.Button("Read QR"))
        {
            VisionQR VisionQR = (VisionQR)this.target;
            Texture2D texture = (Texture2D)textureProperty.objectReferenceValue;
            VisionQR.Look(texture, (object source, Vision.Message message) => {
                text = message.lines.First();
            });
        }

        Rect bounds = GUILayoutUtility.GetRect(0.0f, 100.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
        GUIStyle style = new GUIStyle(EditorStyles.textArea);
        style.wordWrap = true;
        text = EditorGUI.TextArea(bounds, text, style);

        serializedObject.ApplyModifiedProperties();
    }

}