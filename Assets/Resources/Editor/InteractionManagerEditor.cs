using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(InteractionManager))]
public class InteractionManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        InteractionManager interactionManager = (InteractionManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Player Block Relations", EditorStyles.boldLabel);

        if (interactionManager.playerBlockRelations != null)
        {
            foreach (KeyValuePair<GameObject, GameObject> relation in interactionManager.playerBlockRelations)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField("key", relation.Key, typeof(GameObject), true);
                EditorGUILayout.ObjectField("value", relation.Value, typeof(GameObject), true);
                EditorGUILayout.EndHorizontal();
            }
        }

        if (GUILayout.Button("Refresh"))
        {
            EditorUtility.SetDirty(target);
        }
    }
}
