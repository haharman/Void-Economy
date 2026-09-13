using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ListLabelAttribute))]
public class ListLabelDrawer : PropertyDrawer
{
    private GUIContent GetLabel(SerializedProperty property, GUIContent label)
    {
        var attr = (ListLabelAttribute)attribute;
        var nameProp = property.FindPropertyRelative(attr.FieldName);
        return (nameProp != null && !string.IsNullOrEmpty(nameProp.stringValue))
            ? new GUIContent(nameProp.stringValue)
            : label;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUI.GetPropertyHeight(property, GetLabel(property, label), true);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        => EditorGUI.PropertyField(position, property, GetLabel(property, label), true);
}