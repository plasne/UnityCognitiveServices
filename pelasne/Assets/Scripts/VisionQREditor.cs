using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Linq;

// Custom Editor using SerializedProperties.
// Automatic handling of multi-object editing, undo, and prefab overrides.
[CustomEditor(typeof(VisionQR))]
[CanEditMultipleObjects]
public class VisionQREditor : Editor
{
    SerializedProperty textureProperty;

    private string text;

    void OnEnable()
    {
        // Setup the SerializedProperties.
        textureProperty = serializedObject.FindProperty("texture");
    }

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

        if (GUILayout.Button("Read QR"))
        {
            VisionQR VisionQR = (VisionQR)this.target;
            Texture2D texture = (Texture2D)textureProperty.objectReferenceValue;
            VisionQR.Look(texture, (object source, Vision.Message message) => {
                text = message.lines.First();
            });
        }

        //scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(150), GUILayout.ExpandHeight(false));
        Rect bounds = GUILayoutUtility.GetRect(0.0f, 100.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
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