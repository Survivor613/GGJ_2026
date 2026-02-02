using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 关卡标题 UI 快速创建工具
/// </summary>
public class LevelTitleUIBuilder : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/UI/Level Title UI (空洞骑士风格)", false, 10)]
    static void CreateLevelTitleUI()
    {
        // 查找或创建 Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // 创建主物体
        GameObject levelTitleGO = new GameObject("LevelTitleUI");
        levelTitleGO.transform.SetParent(canvas.transform, false);
        levelTitleGO.layer = LayerMask.NameToLayer("UI");

        // 添加 RectTransform (全屏)
        RectTransform rootRect = levelTitleGO.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;
        rootRect.anchoredPosition = Vector2.zero;

        // 添加 CanvasGroup
        CanvasGroup canvasGroup = levelTitleGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // 创建标题容器
        GameObject titleContainerGO = new GameObject("TitleContainer");
        titleContainerGO.transform.SetParent(levelTitleGO.transform, false);
        RectTransform containerRect = titleContainerGO.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(800, 200);
        containerRect.anchoredPosition = Vector2.zero;

        // 创建标题文字
        GameObject titleTextGO = new GameObject("TitleText");
        titleTextGO.transform.SetParent(titleContainerGO.transform, false);
        RectTransform textRect = titleTextGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(600, 120);
        textRect.anchoredPosition = Vector2.zero;
        
        // 使用 Unity 原生 Text 组件
        Text titleText = titleTextGO.AddComponent<Text>();
        titleText.text = "关卡标题";
        titleText.fontSize = 72;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        titleText.raycastTarget = false;
        titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
        titleText.verticalOverflow = VerticalWrapMode.Overflow;
        
        // 添加黑色描边增加可读性
        Outline outline = titleTextGO.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.8f);
        outline.effectDistance = new Vector2(3, -3);
        
        // 尝试查找项目中的中文字体 (Source Han Sans SC-VF)
        string[] fontGuids = AssetDatabase.FindAssets("t:Font");
        foreach (string guid in fontGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("SourceHan") || path.Contains("Source Han"))
            {
                Font chineseFont = AssetDatabase.LoadAssetAtPath<Font>(path);
                if (chineseFont != null)
                {
                    titleText.font = chineseFont;
                    Debug.Log($"<color=green>已设置中文字体: {path}</color>");
                    break;
                }
            }
        }

        // 加载装饰图片
        Sprite upperSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Title/Upper_Decoration.png");
        Sprite lowerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Title/Lower_Decoration.png");

        // 创建上方装饰
        GameObject topDecoGO = new GameObject("DecorationTop");
        topDecoGO.transform.SetParent(titleContainerGO.transform, false);
        RectTransform topRect = topDecoGO.AddComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0.5f, 1f);
        topRect.anchorMax = new Vector2(0.5f, 1f);
        topRect.sizeDelta = new Vector2(400, 50);
        topRect.anchoredPosition = new Vector2(0, 30);
        Image topImage = topDecoGO.AddComponent<Image>();
        topImage.sprite = upperSprite;
        topImage.color = Color.white;
        topImage.raycastTarget = false;
        topImage.preserveAspect = true;

        // 创建下方装饰
        GameObject bottomDecoGO = new GameObject("DecorationBottom");
        bottomDecoGO.transform.SetParent(titleContainerGO.transform, false);
        RectTransform bottomRect = bottomDecoGO.AddComponent<RectTransform>();
        bottomRect.anchorMin = new Vector2(0.5f, 0f);
        bottomRect.anchorMax = new Vector2(0.5f, 0f);
        bottomRect.sizeDelta = new Vector2(400, 50);
        bottomRect.anchoredPosition = new Vector2(0, -30);
        Image bottomImage = bottomDecoGO.AddComponent<Image>();
        bottomImage.sprite = lowerSprite;
        bottomImage.color = Color.white;
        bottomImage.raycastTarget = false;
        bottomImage.preserveAspect = true;

        // 添加 LevelTitleUI 脚本
        LevelTitleUI levelTitleUI = levelTitleGO.AddComponent<LevelTitleUI>();
        
        // 通过序列化属性设置引用
        SerializedObject so = new SerializedObject(levelTitleUI);
        so.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        so.FindProperty("titleText").objectReferenceValue = titleText;
        so.FindProperty("decorationTop").objectReferenceValue = topImage;
        so.FindProperty("decorationBottom").objectReferenceValue = bottomImage;
        so.ApplyModifiedProperties();

        // 选中新创建的物体
        Selection.activeGameObject = levelTitleGO;
        
        Debug.Log("<color=green>已创建 LevelTitleUI，请将其保存为 Prefab 并放置到各关卡场景中</color>");
    }

    [MenuItem("Tools/Level Title/创建 LevelTitle Prefab")]
    static void CreateLevelTitlePrefab()
    {
        CreateLevelTitleUI();
    }
#endif
}
