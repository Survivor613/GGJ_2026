using System;
using System.Collections.Generic;
using DialogueSystem.Data;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(DialogueScriptSO))]
public class DialogueScriptSOEditor : Editor
{
    private ReorderableList nodesList;
    private SerializedProperty nodesProperty;

    private void OnEnable()
    {
        nodesProperty = serializedObject.FindProperty("nodes");
        nodesList = new ReorderableList(serializedObject, nodesProperty, true, true, true, true);

        nodesList.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Nodes");
        };

        nodesList.elementHeightCallback = index =>
        {
            var element = nodesProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true) + 6f;
        };

        nodesList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = nodesProperty.GetArrayElementAtIndex(index);
            rect.y += 2f;
            EditorGUI.PropertyField(rect, element, new GUIContent($"Node {index}"), true);
        };

        nodesList.onAddDropdownCallback = (rect, list) =>
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Line Node"), false, () => AddNode(typeof(LineNode)));
            menu.AddItem(new GUIContent("Command Node"), false, () => AddNode(typeof(CommandNode)));
            menu.ShowAsContext();
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        nodesList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    private void AddNode(Type type)
    {
        serializedObject.Update();

        int index = nodesProperty.arraySize;
        nodesProperty.arraySize++;
        var element = nodesProperty.GetArrayElementAtIndex(index);

        if (type == typeof(LineNode))
        {
            var node = new LineNode
            {
                id = GetUniqueId("line_"),
                nextId = string.Empty
            };
            element.managedReferenceValue = node;
        }
        else if (type == typeof(CommandNode))
        {
            var node = new CommandNode
            {
                id = GetUniqueId("cmd_"),
                nextId = string.Empty,
                command = string.Empty
            };
            element.managedReferenceValue = node;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private string GetUniqueId(string prefix)
    {
        var existing = new HashSet<string>();
        for (int i = 0; i < nodesProperty.arraySize; i++)
        {
            var element = nodesProperty.GetArrayElementAtIndex(i);
            if (element.managedReferenceValue is DialogueNode node && !string.IsNullOrEmpty(node.id))
            {
                existing.Add(node.id);
            }
        }

        int counter = 1;
        string candidate = $"{prefix}{counter}";
        while (existing.Contains(candidate))
        {
            counter++;
            candidate = $"{prefix}{counter}";
        }

        return candidate;
    }
}
