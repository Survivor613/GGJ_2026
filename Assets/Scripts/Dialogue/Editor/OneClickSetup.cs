using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using DialogueSystem.Core;
using DialogueSystem.UI;
using DialogueSystem.Actors;
using DialogueSystem.Data;
using DialogueSystem.Effects;

/// <summary>
/// 终极一键搭建 - 从零到完成，全自动
/// 菜单：Tools/Dialogue/ONE CLICK SETUP (终极一键) ⚡⚡⚡
/// </summary>
public class OneClickSetup : EditorWindow
{
    [MenuItem("Tools/Dialogue/ONE CLICK SETUP (终极一键) ⚡⚡⚡")]
    static void OneClickSetupAll()
    {
        bool confirm = EditorUtility.DisplayDialog("终极一键搭建", 
            "此工具将完整搭建对话系统（原生Text版本）：\n\n" +
            "1. 清理旧场景\n" +
            "2. 创建测试数据\n" +
            "3. 搭建 UI 和组件\n" +
            "4. 转换到原生 Text\n" +
            "5. 清理 TMP 残留\n" +
            "6. 配置所有引用\n" +
            "7. 设置中文字体\n\n" +
            "完成后可以直接按 Play 测试！\n\n" +
            "确定继续？", 
            "确定", "取消");
            
        if (!confirm) return;
        
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>====== 终极一键搭建 - 开始执行 ======</color>");
        Debug.Log("<color=cyan>========================================</color>");
        
        try
        {
            // 步骤 1：清理旧场景
            Step1_CleanupOldScene();
            
            // 步骤 2：创建测试数据
            Step2_CreateTestData();
            
            // 步骤 3：创建 Prefabs
            Step3_CreatePrefabs();
            
            // 步骤 4：搭建场景
            Step4_SetupScene();
            
            // 步骤 5：转换到原生 Text
            Step5_ConvertToUnityText();
            
            // 步骤 6：清理和迁移
            Step6_CleanupAndMigrate();
            
            // 步骤 7：设置字体
            Step7_SetupFont();
            
            // 步骤 8：最终验证
            Step8_Verify();
            
            Debug.Log("<color=green>========================================</color>");
            Debug.Log("<color=green>====== ✓ 搭建完成！按 Play 测试！ ======</color>");
            Debug.Log("<color=green>========================================</color>");
            
            EditorUtility.DisplayDialog("搭建完成", 
                "对话系统搭建完成！\n\n" +
                "现在按 Play 按钮即可测试中文对话。\n\n" +
                "控制方式：\n" +
                "- 鼠标点击/空格：推进对话\n" +
                "- H键：历史面板\n" +
                "- R键：重新开始", 
                "太好了！");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"搭建过程出错：{e.Message}");
            EditorUtility.DisplayDialog("错误", $"搭建失败：\n{e.Message}", "确定");
        }
    }
    
    static void Step1_CleanupOldScene()
    {
        Debug.Log("\n<color=yellow>[步骤 1/8] 清理旧场景...</color>");
        
        // 删除旧的 DialogueSystem
        var oldSystems = GameObject.FindObjectsOfType<DialogueRunner>();
        foreach (var sys in oldSystems)
        {
            DestroyImmediate(sys.gameObject);
        }
        
        // 删除旧的 DialoguePanel
        var oldPanels = GameObject.FindObjectsOfType<DialogueView>();
        foreach (var panel in oldPanels)
        {
            DestroyImmediate(panel.gameObject);
        }
        
        var oldUniversalPanels = GameObject.FindObjectsOfType<DialogueViewUniversal>();
        foreach (var panel in oldUniversalPanels)
        {
            DestroyImmediate(panel.gameObject);
        }
        
        Debug.Log("✓ 清理完成");
    }
    
    static void Step2_CreateTestData()
    {
        Debug.Log("\n<color=yellow>[步骤 2/8] 创建测试数据...</color>");
        
        if (!AssetDatabase.LoadAssetAtPath<DialogueScriptSO>("Assets/Resources/TestDialogue.asset"))
        {
            DialogueTestDataCreator.CreateTestData();
        }
        else
        {
            Debug.Log("✓ 测试数据已存在");
        }
    }
    
    static void Step3_CreatePrefabs()
    {
        Debug.Log("\n<color=yellow>[步骤 3/8] 创建 Prefabs...</color>");
        
        if (!AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/ActorView.prefab"))
        {
            DialoguePrefabCreator.CreateActorPrefab();
        }
        
        string historyPath = "Assets/Prefab/HistoryEntry.prefab";
        GameObject historyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(historyPath);
        if (historyPrefab == null)
        {
            DialoguePrefabCreator.CreateHistoryEntryPrefab();
        }
        else
        {
            // 如果历史 Prefab 仍是 TMP，则重建为原生 Text 版本
            TMP_Text[] tmpTexts = historyPrefab.GetComponentsInChildren<TMP_Text>(true);
            if (tmpTexts != null && tmpTexts.Length > 0)
            {
                DialoguePrefabCreator.CreateHistoryEntryPrefab();
            }
        }
        
        Debug.Log("✓ Prefabs 准备完成");
    }
    
    static void Step4_SetupScene()
    {
        Debug.Log("\n<color=yellow>[步骤 4/8] 搭建场景...</color>");
        
        Canvas canvas = FindOrCreateCanvas();
        GameObject actorLayer = CreateActorLayer(canvas.transform);
        GameObject dialoguePanel = CreateDialoguePanel(canvas.transform);
        GameObject historyPanel = CreateHistoryPanel(canvas.transform);
        GameObject dialogueSystem = CreateDialogueSystem();
        
        AutoWireReferences(dialogueSystem, dialoguePanel, actorLayer, historyPanel);
        LoadTestData(dialogueSystem);
        
        Debug.Log("✓ 场景搭建完成");
    }
    
    static void Step5_ConvertToUnityText()
    {
        Debug.Log("\n<color=yellow>[步骤 5/8] 转换到原生 Text...</color>");
        
        TMP_Text[] tmpTexts = GameObject.FindObjectsOfType<TMP_Text>(true);
        
        foreach (var tmp in tmpTexts)
        {
            if (tmp.gameObject.name == "NameText" || tmp.gameObject.name == "BodyText")
            {
                GameObject go = tmp.gameObject;
                string originalText = tmp.text;
                Color originalColor = tmp.color;
                int originalFontSize = (int)tmp.fontSize;
                
                DestroyImmediate(tmp);
                
                Text unityText = go.AddComponent<Text>();
                unityText.text = originalText;
                unityText.color = originalColor;
                unityText.fontSize = originalFontSize;
                unityText.supportRichText = true;
                unityText.horizontalOverflow = HorizontalWrapMode.Wrap;
                unityText.alignment = TextAnchor.UpperLeft;
                
                EditorUtility.SetDirty(go);
            }
        }
        
        Debug.Log("✓ 转换到原生 Text 完成");
    }
    
    static void Step6_CleanupAndMigrate()
    {
        Debug.Log("\n<color=yellow>[步骤 6/8] 清理和迁移...</color>");
        
        // 禁用 TextEffectController
        var effectControllers = GameObject.FindObjectsOfType<TextEffectController>(true);
        foreach (var controller in effectControllers)
        {
            controller.enabled = false;
        }
        
        // 替换 DialogueView 为 Universal
        DialogueView[] oldViews = GameObject.FindObjectsOfType<DialogueView>(true);
        foreach (var oldView in oldViews)
        {
            GameObject go = oldView.gameObject;
            SerializedObject so = new SerializedObject(oldView);
            GameObject panel = so.FindProperty("panel").objectReferenceValue as GameObject;
            GameObject continueIcon = so.FindProperty("continueIcon").objectReferenceValue as GameObject;
            
            DestroyImmediate(oldView);
            
            DialogueViewUniversal newView = go.AddComponent<DialogueViewUniversal>();
            
            SerializedObject newSO = new SerializedObject(newView);
            newSO.FindProperty("panel").objectReferenceValue = panel;
            newSO.FindProperty("continueIcon").objectReferenceValue = continueIcon;
            
            Text[] texts = go.GetComponentsInChildren<Text>(true);
            foreach (var text in texts)
            {
                if (text.gameObject.name == "NameText")
                    newSO.FindProperty("nameText").objectReferenceValue = text;
                if (text.gameObject.name == "BodyText")
                {
                    newSO.FindProperty("bodyText").objectReferenceValue = text;
                    
                    TypewriterEffectUniversal typewriter = text.gameObject.AddComponent<TypewriterEffectUniversal>();
                    SerializedObject twSO = new SerializedObject(typewriter);
                    twSO.FindProperty("textComponent").objectReferenceValue = text;
                    twSO.ApplyModifiedProperties();
                    
                    newSO.FindProperty("typewriter").objectReferenceValue = typewriter;
                }
            }
            
            newSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(go);
        }
        
        // 更新 DialogueRunner 引用
        DialogueRunner runner = GameObject.FindObjectOfType<DialogueRunner>();
        if (runner != null)
        {
            var dialogueViewUniversal = GameObject.FindObjectOfType<DialogueViewUniversal>();
            if (dialogueViewUniversal != null)
            {
                SerializedObject runnerSO = new SerializedObject(runner);
                runnerSO.FindProperty("dialogueViewComponent").objectReferenceValue = dialogueViewUniversal;
                runnerSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(runner);
            }
        }
        
        Debug.Log("✓ 清理迁移完成");
    }
    
    static void Step7_SetupFont()
    {
        Debug.Log("\n<color=yellow>[步骤 7/8] 设置字体...</color>");
        
        // 查找项目中的字体
        string[] guids = AssetDatabase.FindAssets("t:Font");
        Font chineseFont = null;
        
        // 优先查找思源/中文字体
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Font font = AssetDatabase.LoadAssetAtPath<Font>(path);
            if (font != null && (font.name.Contains("Source") || font.name.Contains("Noto") || 
                font.name.Contains("源") || font.name.Contains("黑") || font.name.Contains("雅")))
            {
                chineseFont = font;
                break;
            }
        }
        
        // 如果没找到，使用 Arial
        if (chineseFont == null)
        {
            chineseFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Debug.Log("✓ 使用 Arial 字体（系统默认）");
        }
        else
        {
            Debug.Log($"✓ 使用字体: {chineseFont.name}");
        }
        
        // 应用字体
        if (chineseFont != null)
        {
            Text[] allTexts = GameObject.FindObjectsOfType<Text>(true);
            foreach (var text in allTexts)
            {
                text.font = chineseFont;
                EditorUtility.SetDirty(text);
            }
        }

        ApplyFontToHistoryPrefab(chineseFont);
        
        Debug.Log("✓ 字体设置完成");
    }

    static void ApplyFontToHistoryPrefab(Font font)
    {
        if (font == null) return;

        string historyPath = "Assets/Prefab/HistoryEntry.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(historyPath);
        if (prefab == null) return;

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(historyPath);
        Text[] texts = prefabRoot.GetComponentsInChildren<Text>(true);
        foreach (var text in texts)
        {
            text.font = font;
            EditorUtility.SetDirty(text);
        }
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, historyPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);
    }
    
    static void Step8_Verify()
    {
        Debug.Log("\n<color=yellow>[步骤 8/8] 验证配置...</color>");
        
        DialogueRunner runner = GameObject.FindObjectOfType<DialogueRunner>();
        if (runner == null)
        {
            Debug.LogError("❌ 未找到 DialogueRunner");
            return;
        }
        
        SerializedObject so = new SerializedObject(runner);
        
        bool hasView = so.FindProperty("dialogueViewComponent").objectReferenceValue != null;
        bool hasActor = so.FindProperty("actorController").objectReferenceValue != null;
        
        Debug.Log($"  DialogueView: {(hasView ? "✓" : "❌")}");
        Debug.Log($"  ActorController: {(hasActor ? "✓" : "❌")}");
        
        if (!hasView || !hasActor)
        {
            Debug.LogWarning("⚠ 部分引用未设置，可能需要手动检查");
        }
        else
        {
            Debug.Log("✓ 所有核心引用已设置");
        }
    }
    
    // 辅助方法 - 从 DialogueSystemAutoSetup 复制
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
        return actorLayer;
    }
    
    static GameObject CreateDialoguePanel(Transform parent)
    {
        GameObject panel = new GameObject("DialoguePanel");
        panel.transform.SetParent(parent, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0f);
        panelRect.anchorMax = new Vector2(0.9f, 0.3f);
        panelRect.sizeDelta = Vector2.zero;
        
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        
        // NameText
        GameObject nameTextGO = new GameObject("NameText");
        nameTextGO.transform.SetParent(panel.transform, false);
        RectTransform nameRect = nameTextGO.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(0, 1);
        nameRect.pivot = new Vector2(0, 1);
        nameRect.anchoredPosition = new Vector2(20, -10);
        nameRect.sizeDelta = new Vector2(300, 40);
        
        Text nameText = nameTextGO.AddComponent<Text>();
        nameText.text = "角色名";
        nameText.fontSize = 28;
        nameText.fontStyle = FontStyle.Bold;
        nameText.color = Color.yellow;
        
        // BodyText
        GameObject bodyTextGO = new GameObject("BodyText");
        bodyTextGO.transform.SetParent(panel.transform, false);
        RectTransform bodyRect = bodyTextGO.AddComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0, 0);
        bodyRect.anchorMax = new Vector2(1, 1);
        bodyRect.sizeDelta = new Vector2(-40, -60);
        bodyRect.anchoredPosition = new Vector2(0, -10);
        
        Text bodyText = bodyTextGO.AddComponent<Text>();
        bodyText.text = "对话内容";
        bodyText.fontSize = 24;
        bodyText.color = Color.white;
        bodyText.supportRichText = true;
        bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
        
        // TypewriterEffectUniversal
        TypewriterEffectUniversal typewriter = bodyTextGO.AddComponent<TypewriterEffectUniversal>();
        SerializedObject twSO = new SerializedObject(typewriter);
        twSO.FindProperty("textComponent").objectReferenceValue = bodyText;
        twSO.ApplyModifiedProperties();
        
        // ContinueIcon
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
        
        // DialogueViewUniversal
        DialogueViewUniversal dialogueView = panel.AddComponent<DialogueViewUniversal>();
        SerializedObject dvSO = new SerializedObject(dialogueView);
        dvSO.FindProperty("panel").objectReferenceValue = panel;
        dvSO.FindProperty("nameText").objectReferenceValue = nameText;
        dvSO.FindProperty("bodyText").objectReferenceValue = bodyText;
        dvSO.FindProperty("continueIcon").objectReferenceValue = iconGO;
        dvSO.FindProperty("typewriter").objectReferenceValue = typewriter;
        dvSO.ApplyModifiedProperties();
        
        return panel;
    }
    
    static GameObject CreateHistoryPanel(Transform parent)
    {
        GameObject panel = new GameObject("HistoryPanel");
        panel.transform.SetParent(parent, false);
        panel.SetActive(false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.9f);
        
        GameObject scrollViewGO = new GameObject("ScrollView");
        scrollViewGO.transform.SetParent(panel.transform, false);
        RectTransform scrollRect = scrollViewGO.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.1f, 0.1f);
        scrollRect.anchorMax = new Vector2(0.9f, 0.9f);
        scrollRect.sizeDelta = Vector2.zero;
        
        ScrollRect scroll = scrollViewGO.AddComponent<ScrollRect>();
        scrollViewGO.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollViewGO.transform, false);
        RectTransform viewportRect = viewportGO.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportGO.AddComponent<Image>();
        viewportGO.AddComponent<Mask>().showMaskGraphic = false;
        
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
        
        scroll.content = contentRect;
        scroll.viewport = viewportRect;
        scroll.horizontal = false;
        scroll.vertical = true;
        
        HistoryView historyView = panel.AddComponent<HistoryView>();
        GameObject entryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/HistoryEntry.prefab");
        
        SerializedObject hvSO = new SerializedObject(historyView);
        hvSO.FindProperty("panel").objectReferenceValue = panel;
        hvSO.FindProperty("contentRoot").objectReferenceValue = contentGO.transform;
        hvSO.FindProperty("entryPrefab").objectReferenceValue = entryPrefab;
        hvSO.FindProperty("scrollRect").objectReferenceValue = scroll;
        hvSO.ApplyModifiedProperties();
        
        return panel;
    }
    
    static GameObject CreateDialogueSystem()
    {
        GameObject system = new GameObject("DialogueSystem");
        system.AddComponent<DialogueRunner>();
        system.AddComponent<ActorController>();
        system.AddComponent<DialogueInputHandler>();
        system.AddComponent<DialogueTest>();
        return system;
    }
    
    static void AutoWireReferences(GameObject system, GameObject dialoguePanel, GameObject actorLayer, GameObject historyPanel)
    {
        DialogueRunner runner = system.GetComponent<DialogueRunner>();
        SerializedObject runnerSO = new SerializedObject(runner);
        runnerSO.FindProperty("dialogueViewComponent").objectReferenceValue = dialoguePanel.GetComponent<DialogueViewUniversal>();
        runnerSO.FindProperty("actorController").objectReferenceValue = system.GetComponent<ActorController>();
        runnerSO.FindProperty("historyView").objectReferenceValue = historyPanel.GetComponent<HistoryView>();
        runnerSO.ApplyModifiedProperties();
        
        ActorController actorCtrl = system.GetComponent<ActorController>();
        GameObject actorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/ActorView.prefab");
        
        SerializedObject actorSO = new SerializedObject(actorCtrl);
        actorSO.FindProperty("actorPrefab").objectReferenceValue = actorPrefab;
        actorSO.FindProperty("actorLayer").objectReferenceValue = actorLayer.transform;
        actorSO.ApplyModifiedProperties();
    }
    
    static void LoadTestData(GameObject system)
    {
        var testScript = AssetDatabase.LoadAssetAtPath<DialogueScriptSO>("Assets/Resources/TestDialogue.asset");
        var actorDef = AssetDatabase.LoadAssetAtPath<ActorDefinitionSO>("Assets/Resources/Actor_Alice.asset");
        
        if (testScript != null)
        {
            DialogueTest test = system.GetComponent<DialogueTest>();
            SerializedObject testSO = new SerializedObject(test);
            testSO.FindProperty("testScript").objectReferenceValue = testScript;
            testSO.ApplyModifiedProperties();
        }
        
        if (actorDef != null)
        {
            ActorController actorCtrl = system.GetComponent<ActorController>();
            SerializedObject actorSO = new SerializedObject(actorCtrl);
            SerializedProperty defsProp = actorSO.FindProperty("actorDefinitions");
            defsProp.arraySize = 1;
            defsProp.GetArrayElementAtIndex(0).objectReferenceValue = actorDef;
            actorSO.ApplyModifiedProperties();
        }
    }
}
