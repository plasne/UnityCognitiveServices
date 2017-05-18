using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(VisionText))]
[CanEditMultipleObjects]
public class VisionTextEditor : Editor
{

    SerializedProperty visionKeyProperty;
    SerializedProperty translationKeyProperty;
    SerializedProperty regionProperty;
    SerializedProperty translateToProperty;
    SerializedProperty textureProperty;

    void OnEnable()
    {
        visionKeyProperty = serializedObject.FindProperty("VisionKey");
        translationKeyProperty = serializedObject.FindProperty("TranslationKey");
        regionProperty = serializedObject.FindProperty("Region");
        translateToProperty = serializedObject.FindProperty("TranslateTo");
        textureProperty = serializedObject.FindProperty("texture");
    }

    //private Vector2 scrollPos;
    private string text;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // show the properties
        EditorGUILayout.PropertyField(visionKeyProperty, new GUIContent("Vision Key"));
        EditorGUILayout.PropertyField(translationKeyProperty, new GUIContent("Translation Key"));
        EditorGUILayout.PropertyField(regionProperty, new GUIContent("Region"));
        EditorGUILayout.PropertyField(translateToProperty, new GUIContent("Translate To"));
        EditorGUILayout.PropertyField(textureProperty, new GUIContent("Texture"));

        EditorGUILayout.Space();

        float width = EditorGUIUtility.currentViewWidth;

        if (GUILayout.Button("Read Text"))
        {
            VisionText VisionText = (VisionText) this.target;
            Texture2D texture = (Texture2D)textureProperty.objectReferenceValue;
            VisionText.Look(texture, (object source, Vision.Message message) => {
                text = string.Join("\n", message.lines.ToArray());
            });
        }

        Rect bounds = GUILayoutUtility.GetRect(0.0f, 100.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
        GUIStyle style = new GUIStyle(EditorStyles.textArea);
        style.wordWrap = true;
        text = EditorGUI.TextArea(bounds, text, style);

        serializedObject.ApplyModifiedProperties();
    }

}