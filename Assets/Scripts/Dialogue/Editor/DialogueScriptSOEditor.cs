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

            // 右键菜单：在当前节点后插入
            if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Insert Line Node After"), false, () => InsertNodeAfter(index, typeof(LineNode)));
                menu.AddItem(new GUIContent("Insert Command Node After"), false, () => InsertNodeAfter(index, typeof(CommandNode)));
                menu.ShowAsContext();
                Event.current.Use();
            }
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
        AssignNodeValue(element, type);

        serializedObject.ApplyModifiedProperties();
    }

    private void InsertNodeAfter(int index, Type type)
    {
        serializedObject.Update();

        int insertIndex = Mathf.Clamp(index + 1, 0, nodesProperty.arraySize);
        nodesProperty.InsertArrayElementAtIndex(insertIndex);
        var element = nodesProperty.GetArrayElementAtIndex(insertIndex);
        AssignNodeValue(element, type);

        serializedObject.ApplyModifiedProperties();
    }

    private void AssignNodeValue(SerializedProperty element, Type type)
    {
        if (type == typeof(LineNode))
        {
            element.managedReferenceValue = new LineNode();
        }
        else if (type == typeof(CommandNode))
        {
            element.managedReferenceValue = new CommandNode { command = string.Empty };
        }
    }

}
