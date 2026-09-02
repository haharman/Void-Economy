using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ListLabelAttribute))]
public class ListLabelDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = (ListLabelAttribute)attribute;
        var nameProp = property.FindPropertyRelative(attr.FieldName);
        string displayName = (nameProp != null && !string.IsNullOrEmpty(nameProp.stringValue))
            ? nameProp.stringValue : label.text;

        EditorGUI.PropertyField(position, property, new GUIContent(displayName), true);
    }
}