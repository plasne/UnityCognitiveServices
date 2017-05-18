using UnityEditor;
using UnityEngine;
using System.Collections;

// Custom Editor using SerializedProperties.
// Automatic handling of multi-object editing, undo, and prefab overrides.
[CustomEditor(typeof(VisionText))]
[CanEditMultipleObjects]
public class VisionTextEditor : Editor
{
    SerializedProperty textureProperty;

    void OnEnable()
    {
        // Setup the SerializedProperties.
        textureProperty = serializedObject.FindProperty("texture");
    }

    private Vector2 scrollPos;
    private string text;

    public override void OnInspectorGUI()
    {
        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
        serializedObject.Update();

        /*
        // Show the custom GUI controls.
        EditorGUILayout.IntSlider(damageProp, 0, 100, new GUIContent("Damage"));

        // Only show the damage progress bar if all the objects have the same damage value:
        if (!damageProp.hasMultipleDifferentValues)
            ProgressBar(damageProp.intValue / 100.0f, "Damage");

        EditorGUILayout.IntSlider(armorProp, 0, 100, new GUIContent("Armor"));

        // Only show the armor progress bar if all the objects have the same armor value:
        if (!armorProp.hasMultipleDifferentValues)
            ProgressBar(armorProp.intValue / 100.0f, "Armor");

    */

        EditorGUILayout.PropertyField(textureProperty, new GUIContent("Texture"));


        float width = EditorGUIUtility.currentViewWidth;

        /*
        Rect rect = GUILayoutUtility.GetRect(18, 18, "TextField");
        EditorGUI.ProgressBar(rect, value, label);
        EditorGUILayout.Space();
        */


        if (GUILayout.Button("Read Text"))
        {
            VisionText VisionText = (VisionText) this.target;
            Texture2D texture = (Texture2D)textureProperty.objectReferenceValue;
            VisionText.Look(texture, (object source, Vision.Message message) => {
                text = string.Join("\n", message.lines.ToArray());
            });
        }


        //scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(150), GUILayout.ExpandHeight(false));
        //Rect bounds = GUILayoutUtility.GetRect(0.0f, 100.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
        Rect bounds = new Rect(0.0f, 0.0f, width, 50.0f);
        GUIStyle style = new GUIStyle(EditorStyles.textArea);
        style.wordWrap = true;
        text = EditorGUI.TextArea(bounds, text, style);
        //EditorGUILayout.EndScrollView();


        // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties();
    }

    // Custom GUILayout progress bar.
    void ProgressBar(float value, string label)
    {
        // Get a rect for the progress bar using the same margins as a textfield:
        Rect rect = GUILayoutUtility.GetRect(18, 18, "TextField");
        EditorGUI.ProgressBar(rect, value, label);
        EditorGUILayout.Space();
    }
}