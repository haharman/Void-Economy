using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;

[CustomEditor(typeof(MonoBehaviour), true)]
[CanEditMultipleObjects]
public class ButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var methods = target.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttribute<ButtonAttribute>() != null);

        foreach (var method in methods)
        {
            if (GUILayout.Button(ObjectNames.NicifyVariableName(method.Name)))
            {
                foreach (var t in targets)
                    method.Invoke(t, null);
            }
        }
    }
}