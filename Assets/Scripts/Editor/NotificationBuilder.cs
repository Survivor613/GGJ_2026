using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 操作提示 UI 快速创建工具
/// </summary>
public class NotificationBuilder : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/2D Object/Notification Trigger (操作提示)", false, 10)]
    static void CreateNotificationTrigger()
    {
        // 查找场景中的主 Canvas
        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("场景中没有 Canvas，请先创建一个 Canvas");
            return;
        }
        
        // 创建触发器物体
        GameObject triggerGO = new GameObject("NotificationTrigger");
        triggerGO.transform.position = GetSpawnPosition();
        
        // 添加 BoxCollider2D
        BoxCollider2D collider = triggerGO.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(3f, 2f);
        
        // 在主 Canvas 下创建 UI 元素
        GameObject uiGO = new GameObject("NotificationUI_" + triggerGO.GetInstanceID());
        uiGO.transform.SetParent(mainCanvas.transform, false);
        uiGO.layer = LayerMask.NameToLayer("UI");
        
        RectTransform uiRect = uiGO.AddComponent<RectTransform>();
        uiRect.anchorMin = new Vector2(0.5f, 0.3f); // 屏幕下方
        uiRect.anchorMax = new Vector2(0.5f, 0.3f);
        uiRect.sizeDelta = new Vector2(300, 60);
        uiRect.anchoredPosition = Vector2.zero;
        
        CanvasGroup canvasGroup = uiGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        
        // 创建背景
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(uiGO.transform, false);
        bgGO.layer = LayerMask.NameToLayer("UI");
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);
        bgImage.raycastTarget = false;
        
        // 创建文字
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(uiGO.transform, false);
        textGO.layer = LayerMask.NameToLayer("UI");
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = new Vector2(-20, -10);
        textRect.anchoredPosition = Vector2.zero;
        
        Text text = textGO.AddComponent<Text>();
        text.text = "按 E 互动";
        text.fontSize = 32;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        
        // 尝试设置中文字体
        string[] fontGuids = AssetDatabase.FindAssets("t:Font");
        foreach (string guid in fontGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("SourceHan") || path.Contains("Source Han"))
            {
                Font chineseFont = AssetDatabase.LoadAssetAtPath<Font>(path);
                if (chineseFont != null)
                {
                    text.font = chineseFont;
                    break;
                }
            }
        }
        
        // 添加 UI_Notification 脚本
        UI_Notification notification = triggerGO.AddComponent<UI_Notification>();
        
        // 设置引用
        SerializedObject so = new SerializedObject(notification);
        so.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        so.FindProperty("textComponent").objectReferenceValue = text;
        so.FindProperty("notificationText").stringValue = "按 E 互动";
        so.ApplyModifiedProperties();
        
        // 选中并聚焦
        Selection.activeGameObject = triggerGO;
        SceneView.lastActiveSceneView?.FrameSelected();
        
        Debug.Log($"<color=green>已创建 NotificationTrigger，UI 已放置在 {mainCanvas.name} 下</color>");
    }
    
    static Vector3 GetSpawnPosition()
    {
        // 尝试在场景视图中心位置创建
        if (SceneView.lastActiveSceneView != null)
        {
            Camera cam = SceneView.lastActiveSceneView.camera;
            if (cam != null)
            {
                return cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
            }
        }
        return Vector3.zero;
    }
    
    [MenuItem("Tools/Notification/创建操作提示触发器")]
    static void CreateFromToolsMenu()
    {
        CreateNotificationTrigger();
    }
#endif
}
