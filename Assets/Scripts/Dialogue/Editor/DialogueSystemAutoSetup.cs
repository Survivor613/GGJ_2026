using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using DialogueSystem.Core;
using DialogueSystem.UI;
using DialogueSystem.Actors;
using DialogueSystem.Effects;

/// <summary>
/// 对话系统一键自动搭建工具
/// 菜单：Tools/Dialogue/Auto Setup Scene (一键搭建)
/// </summary>
public class DialogueSystemAutoSetup : EditorWindow
{
    [MenuItem("Tools/Dialogue/Auto Setup Scene (一键搭建) ⚡")]
    static void AutoSetupScene()
    {
        Debug.Log("<color=cyan>========== 开始自动搭建对话系统 ==========</color>");
        
        // 1. 确保 Prefab 存在
        EnsurePrefabsExist();
        
        // 2. 创建或获取 Canvas
        Canvas canvas = FindOrCreateCanvas();
        
        // 3. 创建 ActorLayer
        GameObject actorLayer = CreateActorLayer(canvas.transform);
        
        // 4. 创建 DialoguePanel
        GameObject dialoguePanel = CreateDialoguePanel(canvas.transform);
        
        // 5. 创建 HistoryPanel
        GameObject historyPanel = CreateHistoryPanel(canvas.transform);
        
        // 6. 创建 DialogueSystem 控制器
        GameObject dialogueSystem = CreateDialogueSystem();
        
        // 7. 自动连接所有引用
        AutoWireReferences(dialogueSystem, dialoguePanel, actorLayer, historyPanel);
        
        // 8. 加载测试数据
        LoadTestData(dialogueSystem);
        
        Debug.Log("<color=green>========== ✓ 对话系统搭建完成！ ==========</color>");
        Debug.Log("<color=yellow>提示：按 Play 即可测试对话系统！</color>");
        
        // 选中 DialogueSystem
        Selection.activeGameObject = dialogueSystem;
        EditorGUIUtility.PingObject(dialogueSystem);
    }
    
    static void EnsurePrefabsExist()
    {
        if (!AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/ActorView.prefab"))
        {
            Debug.Log("检测到缺少 ActorView Prefab，正在创建...");
            DialoguePrefabCreator.CreateActorPrefab();
        }
        
        if (!AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/HistoryEntry.prefab"))
        {
            Debug.Log("检测到缺少 HistoryEntry Prefab，正在创建...");
            DialoguePrefabCreator.CreateHistoryEntryPrefab();
        }
    }
    
    static Canvas FindOrCreateCanvas()
    {
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            Debug.Log("✓ 创建 Canvas");
        }
        else
        {
            Debug.Log("✓ 使用现有 Canvas");
        }
        return canvas;
    }
    
    static GameObject CreateActorLayer(Transform parent)
    {
        GameObject actorLayer = new GameObject("ActorLayer");
        actorLayer.transform.SetParent(parent, false);
        
        RectTransform rect = actorLayer.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        
        Debug.Log("✓ 创建 ActorLayer");
        return actorLayer;
    }
    
    static GameObject CreateDialoguePanel(Transform parent)
    {
        GameObject panel = new GameObject("DialoguePanel");
        panel.transform.SetParent(parent, false);
        
        // 设置 RectTransform
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0f);
        panelRect.anchorMax = new Vector2(0.9f, 0.3f);
        panelRect.sizeDelta = Vector2.zero;
        
        // 添加背景
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        
        // 创建 NameText
        GameObject nameTextGO = new GameObject("NameText");
        nameTextGO.transform.SetParent(panel.transform, false);
        RectTransform nameRect = nameTextGO.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(0, 1);
        nameRect.pivot = new Vector2(0, 1);
        nameRect.anchoredPosition = new Vector2(20, -10);
        nameRect.sizeDelta = new Vector2(300, 40);
        
        TextMeshProUGUI nameText = nameTextGO.AddComponent<TextMeshProUGUI>();
        nameText.text = "角色名";
        nameText.fontSize = 28;
        nameText.fontStyle = FontStyles.Bold;
        nameText.color = Color.yellow;
        TryApplyChineseFont(nameText);
        
        // 创建 BodyText
        GameObject bodyTextGO = new GameObject("BodyText");
        bodyTextGO.transform.SetParent(panel.transform, false);
        RectTransform bodyRect = bodyTextGO.AddComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0, 0);
        bodyRect.anchorMax = new Vector2(1, 1);
        bodyRect.sizeDelta = new Vector2(-40, -60);
        bodyRect.anchoredPosition = new Vector2(0, -10);
        
        TextMeshProUGUI bodyText = bodyTextGO.AddComponent<TextMeshProUGUI>();
        bodyText.text = "对话内容会显示在这里...";
        bodyText.fontSize = 24;
        bodyText.color = Color.white;
        bodyText.enableWordWrapping = true;
        TryApplyChineseFont(bodyText);
        
        // 挂载 TypewriterEffect 和 TextEffectController
        TypewriterEffect typewriter = bodyTextGO.AddComponent<TypewriterEffect>();
        bodyTextGO.AddComponent<TextEffectController>();
        
        // 使用反射设置私有字段
        SerializedObject so = new SerializedObject(typewriter);
        so.FindProperty("textComponent").objectReferenceValue = bodyText;
        so.ApplyModifiedProperties();
        
        // 创建 ContinueIcon
        GameObject iconGO = new GameObject("ContinueIcon");
        iconGO.transform.SetParent(panel.transform, false);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(1, 0);
        iconRect.anchorMax = new Vector2(1, 0);
        iconRect.pivot = new Vector2(1, 0);
        iconRect.anchoredPosition = new Vector2(-20, 20);
        iconRect.sizeDelta = new Vector2(30, 30);
        
        Image iconImage = iconGO.AddComponent<Image>();
        iconImage.color = Color.white;
        iconGO.SetActive(false);
        
        // 添加 DialogueView 组件
        DialogueView dialogueView = panel.AddComponent<DialogueView>();
        
        // 自动设置引用
        SerializedObject dvSO = new SerializedObject(dialogueView);
        dvSO.FindProperty("panel").objectReferenceValue = panel;
        dvSO.FindProperty("nameText").objectReferenceValue = nameText;
        dvSO.FindProperty("bodyText").objectReferenceValue = bodyText;
        dvSO.FindProperty("continueIcon").objectReferenceValue = iconGO;
        dvSO.FindProperty("typewriter").objectReferenceValue = typewriter;
        dvSO.ApplyModifiedProperties();
        
        Debug.Log("✓ 创建 DialoguePanel 并自动挂载组件");
        return panel;
    }
    
    static GameObject CreateHistoryPanel(Transform parent)
    {
        GameObject panel = new GameObject("HistoryPanel");
        panel.transform.SetParent(parent, false);
        panel.SetActive(false); // 默认隐藏
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.9f);
        
        // 创建 ScrollView
        GameObject scrollViewGO = new GameObject("ScrollView");
        scrollViewGO.transform.SetParent(panel.transform, false);
        RectTransform scrollRect = scrollViewGO.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.1f, 0.1f);
        scrollRect.anchorMax = new Vector2(0.9f, 0.9f);
        scrollRect.sizeDelta = Vector2.zero;
        
        ScrollRect scroll = scrollViewGO.AddComponent<ScrollRect>();
        scrollViewGO.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        // Viewport
        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollViewGO.transform, false);
        RectTransform viewportRect = viewportGO.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportGO.AddComponent<Image>();
        viewportGO.AddComponent<Mask>().showMaskGraphic = false;
        
        // Content
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        RectTransform contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);
        
        VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.spacing = 5;
        
        ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // 配置 ScrollRect
        scroll.content = contentRect;
        scroll.viewport = viewportRect;
        scroll.horizontal = false;
        scroll.vertical = true;
        
        // 添加 HistoryView 组件
        HistoryView historyView = panel.AddComponent<HistoryView>();
        GameObject entryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/HistoryEntry.prefab");
        
        SerializedObject hvSO = new SerializedObject(historyView);
        hvSO.FindProperty("panel").objectReferenceValue = panel;
        hvSO.FindProperty("contentRoot").objectReferenceValue = contentGO.transform;
        hvSO.FindProperty("entryPrefab").objectReferenceValue = entryPrefab;
        hvSO.FindProperty("scrollRect").objectReferenceValue = scroll;
        hvSO.ApplyModifiedProperties();
        
        Debug.Log("✓ 创建 HistoryPanel 并自动挂载组件");
        return panel;
    }
    
    static GameObject CreateDialogueSystem()
    {
        GameObject system = new GameObject("DialogueSystem");
        
        // 添加所有必要组件
        system.AddComponent<DialogueRunner>();
        system.AddComponent<ActorController>();
        system.AddComponent<DialogueInputHandler>();
        system.AddComponent<DialogueTest>();
        
        Debug.Log("✓ 创建 DialogueSystem 控制器");
        return system;
    }
    
    static void AutoWireReferences(GameObject system, GameObject dialoguePanel, GameObject actorLayer, GameObject historyPanel)
    {
        // DialogueRunner
        DialogueRunner runner = system.GetComponent<DialogueRunner>();
        SerializedObject runnerSO = new SerializedObject(runner);
        runnerSO.FindProperty("dialogueView").objectReferenceValue = dialoguePanel.GetComponent<DialogueView>();
        runnerSO.FindProperty("actorController").objectReferenceValue = system.GetComponent<ActorController>();
        runnerSO.FindProperty("historyView").objectReferenceValue = historyPanel.GetComponent<HistoryView>();
        runnerSO.ApplyModifiedProperties();
        
        // ActorController
        ActorController actorCtrl = system.GetComponent<ActorController>();
        GameObject actorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/ActorView.prefab");
        
        SerializedObject actorSO = new SerializedObject(actorCtrl);
        actorSO.FindProperty("actorPrefab").objectReferenceValue = actorPrefab;
        actorSO.FindProperty("actorLayer").objectReferenceValue = actorLayer.transform;
        actorSO.ApplyModifiedProperties();
        
        Debug.Log("✓ 自动连接所有组件引用");
    }
    
    static void LoadTestData(GameObject system)
    {
        // 尝试加载测试数据
        var testScript = AssetDatabase.LoadAssetAtPath<DialogueSystem.Data.DialogueScriptSO>("Assets/Resources/TestDialogue.asset");
        var actorDef = AssetDatabase.LoadAssetAtPath<DialogueSystem.Data.ActorDefinitionSO>("Assets/Resources/Actor_Alice.asset");
        
        if (testScript != null)
        {
            DialogueTest test = system.GetComponent<DialogueTest>();
            SerializedObject testSO = new SerializedObject(test);
            testSO.FindProperty("testScript").objectReferenceValue = testScript;
            testSO.ApplyModifiedProperties();
            Debug.Log("✓ 自动加载测试对话脚本");
        }
        else
        {
            Debug.LogWarning("⚠ 未找到测试对话脚本，请先运行 Tools → Dialogue → Create Test Data");
        }
        
        if (actorDef != null)
        {
            ActorController actorCtrl = system.GetComponent<ActorController>();
            SerializedObject actorSO = new SerializedObject(actorCtrl);
            SerializedProperty defsProp = actorSO.FindProperty("actorDefinitions");
            defsProp.arraySize = 1;
            defsProp.GetArrayElementAtIndex(0).objectReferenceValue = actorDef;
            actorSO.ApplyModifiedProperties();
            Debug.Log("✓ 自动加载角色定义");
        }
    }
    
    static void TryApplyChineseFont(TMP_Text text)
    {
        // 尝试加载中文字体资源
        TMP_FontAsset chineseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/ChineseFont SDF.asset");
        
        if (chineseFont != null)
        {
            text.font = chineseFont;
        }
        else
        {
            Debug.LogWarning("⚠ 未找到中文字体，文本可能显示为口口口。请运行: Tools → Dialogue → Fix Chinese Font");
        }
    }
}
