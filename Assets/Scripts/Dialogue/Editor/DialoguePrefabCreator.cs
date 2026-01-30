using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using DialogueSystem.UI;
using DialogueSystem.Actors;

/// <summary>
/// Unity Editor 工具：快速创建对话系统所需的 Prefab
/// 菜单：Tools/Dialogue/Create Prefabs
/// </summary>
public class DialoguePrefabCreator : EditorWindow
{
    // [MenuItem("Tools/Dialogue/Create Actor Prefab")] // 已禁用：Auto Setup 会自动创建
    public static void CreateActorPrefab()
    {
        // 创建根物体
        GameObject actorGO = new GameObject("ActorView");
        
        // 添加 RectTransform
        RectTransform rectTransform = actorGO.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(400, 600);
        
        // 添加 CanvasGroup
        CanvasGroup canvasGroup = actorGO.AddComponent<CanvasGroup>();
        
        // 添加 Image
        Image image = actorGO.AddComponent<Image>();
        image.raycastTarget = false;
        
        // 添加 ActorView 脚本
        ActorView actorView = actorGO.AddComponent<ActorView>();
        
        // 使用反射设置私有字段
        SerializedObject so = new SerializedObject(actorView);
        so.FindProperty("portraitImage").objectReferenceValue = image;
        so.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        so.ApplyModifiedProperties();
        
        // 保存为 Prefab
        string path = "Assets/Prefab/ActorView.prefab";
        
        // 确保目录存在
        if (!AssetDatabase.IsValidFolder("Assets/Prefab"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefab");
        }
        
        PrefabUtility.SaveAsPrefabAsset(actorGO, path);
        DestroyImmediate(actorGO);
        
        Debug.Log($"<color=green>✓ 创建 ActorView Prefab: {path}</color>");
        
        // 选中并高亮
        Object prefab = AssetDatabase.LoadAssetAtPath<Object>(path);
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }
    
    // [MenuItem("Tools/Dialogue/Create History Entry Prefab")] // 已禁用：Auto Setup 会自动创建
    public static void CreateHistoryEntryPrefab()
    {
        // 创建根物体
        GameObject entryGO = new GameObject("HistoryEntry");
        RectTransform rectTransform = entryGO.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(500, 80);
        
        // 添加 Layout Element
        LayoutElement layoutElement = entryGO.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 80;
        layoutElement.flexibleHeight = -1;
        
        // 添加背景（可选）
        Image bg = entryGO.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        
        // 创建 NameText
        GameObject nameTextGO = new GameObject("NameText");
        nameTextGO.transform.SetParent(entryGO.transform);
        RectTransform nameRect = nameTextGO.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.anchoredPosition = new Vector2(0, -5);
        nameRect.sizeDelta = new Vector2(-10, 25);
        
        Text nameText = nameTextGO.AddComponent<Text>();
        nameText.text = "角色名";
        nameText.fontSize = 18;
        nameText.fontStyle = FontStyle.Bold;
        nameText.color = Color.yellow;
        nameText.supportRichText = true;
        nameText.horizontalOverflow = HorizontalWrapMode.Wrap;
        nameText.verticalOverflow = VerticalWrapMode.Truncate;
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        // 创建 ContentText
        GameObject contentTextGO = new GameObject("ContentText");
        contentTextGO.transform.SetParent(entryGO.transform);
        RectTransform contentRect = contentTextGO.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = new Vector2(0, -30);
        contentRect.sizeDelta = new Vector2(-10, -35);
        
        Text contentText = contentTextGO.AddComponent<Text>();
        contentText.text = "对话内容会显示在这里...";
        contentText.fontSize = 16;
        contentText.color = Color.white;
        contentText.supportRichText = true;
        contentText.horizontalOverflow = HorizontalWrapMode.Wrap;
        contentText.verticalOverflow = VerticalWrapMode.Truncate;
        contentText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        // 添加 HistoryEntryView 脚本
        HistoryEntryView entryView = entryGO.AddComponent<HistoryEntryView>();
        
        // 使用反射设置私有字段（因为它们是 SerializeField）
        SerializedObject so = new SerializedObject(entryView);
        so.FindProperty("nameText").objectReferenceValue = nameText;
        so.FindProperty("contentText").objectReferenceValue = contentText;
        so.ApplyModifiedProperties();
        
        // 保存为 Prefab
        string path = "Assets/Prefab/HistoryEntry.prefab";
        
        if (!AssetDatabase.IsValidFolder("Assets/Prefab"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefab");
        }
        
        PrefabUtility.SaveAsPrefabAsset(entryGO, path);
        DestroyImmediate(entryGO);
        
        Debug.Log($"<color=green>✓ 创建 HistoryEntry Prefab: {path}</color>");
        
        Object prefab = AssetDatabase.LoadAssetAtPath<Object>(path);
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }
    
    // [MenuItem("Tools/Dialogue/Create All Prefabs")] // 已禁用：Auto Setup 会自动创建
    static void CreateAllPrefabs()
    {
        CreateActorPrefab();
        CreateHistoryEntryPrefab();
        Debug.Log("<color=cyan>✓ 所有 Prefab 创建完成！</color>");
    }
}
