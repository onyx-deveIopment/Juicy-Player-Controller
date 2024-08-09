#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace ONYX
{
    [CustomPropertyDrawer(typeof(NoteAttribute))]
    public class NoteDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.LabelField(position, property.stringValue, EditorStyles.wordWrappedLabel);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorStyles.wordWrappedLabel.CalcHeight(new GUIContent(property.stringValue), EditorGUIUtility.currentViewWidth - 30f);
        }
    }
}

#endif
