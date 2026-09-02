using UnityEngine;

public class ListLabelAttribute : PropertyAttribute
{
    public string FieldName;
    public ListLabelAttribute(string fieldName) => FieldName = fieldName;
}