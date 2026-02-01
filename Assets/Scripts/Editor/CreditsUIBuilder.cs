using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GameUI;

/// <summary>
/// 一键创建制作人名单 UI
/// 菜单：Tools/UI/Create Credits UI (一键生成名单)
/// </summary>
public static class CreditsUIBuilder
{
    [MenuItem("Tools/UI/Create Credits UI (一键生成名单) 📜")]
    public static void CreateCreditsUI()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>(true);
        if (canvas == null)
        {
            var canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Panel (reuse if exists)
        var panel = GameObject.Find("CreditsPanel") ?? new GameObject("CreditsPanel");
        panel.transform.SetParent(canvas.transform, false);
        var panelRect = panel.GetComponent<RectTransform>() ?? panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        var panelImage = panel.GetComponent<Image>() ?? panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.6f);

        // ScrollRect (reuse if exists)
        var scrollGO = panel.transform.Find("CreditsScroll")?.gameObject ?? new GameObject("CreditsScroll");
        scrollGO.transform.SetParent(panel.transform, false);
        var scrollRect = scrollGO.GetComponent<ScrollRect>() ?? scrollGO.AddComponent<ScrollRect>();
        var scrollRectTransform = scrollGO.GetComponent<RectTransform>() ?? scrollGO.AddComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.1f, 0.1f);
        scrollRectTransform.anchorMax = new Vector2(0.9f, 0.9f);
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = Vector2.zero;

        // Viewport (reuse if exists)
        var viewportGO = scrollGO.transform.Find("Viewport")?.gameObject ?? new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollGO.transform, false);
        var viewportRect = viewportGO.GetComponent<RectTransform>() ?? viewportGO.AddComponent<RectTransform>();
        if (viewportRect == null)
        {
            Object.DestroyImmediate(viewportGO);
            viewportGO = new GameObject("Viewport", typeof(RectTransform));
            viewportGO.transform.SetParent(scrollGO.transform, false);
            viewportRect = viewportGO.GetComponent<RectTransform>();
        }
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        var viewportImage = viewportGO.GetComponent<Image>() ?? viewportGO.AddComponent<Image>();
        viewportImage.color = new Color(0, 0, 0, 0);
        var mask = viewportGO.GetComponent<Mask>() ?? viewportGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Content (reuse if exists)
        var contentGO = viewportGO.transform.Find("Content")?.gameObject ?? new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        var contentRect = contentGO.GetComponent<RectTransform>() ?? contentGO.AddComponent<RectTransform>();
        if (contentRect == null)
        {
            Object.DestroyImmediate(contentGO);
            contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(viewportGO.transform, false);
            contentRect = contentGO.GetComponent<RectTransform>();
        }
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 0);
        var layout = contentGO.GetComponent<VerticalLayoutGroup>() ?? contentGO.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        var fitter = contentGO.GetComponent<ContentSizeFitter>() ?? contentGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Text (reuse if exists)
        var textGO = contentGO.transform.Find("CreditsText")?.gameObject ?? new GameObject("CreditsText");
        textGO.transform.SetParent(contentGO.transform, false);
        var textRect = textGO.GetComponent<RectTransform>() ?? textGO.AddComponent<RectTransform>();
        if (textRect == null)
        {
            Object.DestroyImmediate(textGO);
            textGO = new GameObject("CreditsText", typeof(RectTransform));
            textGO.transform.SetParent(contentGO.transform, false);
            textRect = textGO.GetComponent<RectTransform>();
        }
        textRect.anchorMin = new Vector2(0, 1);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0.5f, 1);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(0, 0);
        var text = textGO.GetComponent<Text>() ?? textGO.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.UpperCenter;
        text.fontSize = 28;
        text.color = Color.white;
        text.supportRichText = true;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        var textFitter = textGO.GetComponent<ContentSizeFitter>() ?? textGO.AddComponent<ContentSizeFitter>();
        textFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Wire scroll rect
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Unrestricted; // 允许内容完全滚出
        scrollRect.scrollSensitivity = 20f;
        scrollRect.inertia = false; // 禁用惯性，避免干扰手动滚动

        // Add scroller
        var scroller = panel.GetComponent<CreditsScroller>() ?? panel.AddComponent<CreditsScroller>();
        var scrollerSO = new SerializedObject(scroller);
        scrollerSO.FindProperty("scrollRect").objectReferenceValue = scrollRect;
        scrollerSO.FindProperty("creditsText").objectReferenceValue = text;
        scrollerSO.FindProperty("autoScroll").boolValue = true;
        scrollerSO.FindProperty("scrollSpeed").floatValue = 30f;
        scrollerSO.FindProperty("loop").boolValue = false;
        scrollerSO.FindProperty("loadSceneOnComplete").boolValue = true;
        scrollerSO.FindProperty("targetSceneName").stringValue = "MainMenu";
        scrollerSO.FindProperty("delayBeforeLoad").floatValue = 1f;
        scrollerSO.FindProperty("creditsContent").stringValue =
            "\n\n\n\n\n\n\n\n\n\n" +
            "狐面心渊\n\n" +
            "—— 制作人名单 ——\n\n\n" +
            "策划\n" +
            "荷菜\n\n\n" +
            "程序\n" +
            "第三颗萝卜\n" +
            "仿生萝卜\n\n\n" +
            "美术\n" +
            "Luu\n\n\n\n\n" +
            "感谢游玩\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n";
        scrollerSO.ApplyModifiedProperties();

        // 强制刷新布局并重置位置
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        contentRect.anchoredPosition = Vector2.zero;
        
        // 确保文字可见
        text.enabled = true;
        textGO.SetActive(true);
        contentGO.SetActive(true);
        viewportGO.SetActive(true);

        Selection.activeGameObject = panel;
        EditorGUIUtility.PingObject(panel);
        Debug.Log("✓ 已创建制作人名单 UI - 文字应该可见了");
    }
}
