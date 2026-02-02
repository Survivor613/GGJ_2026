using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 关卡标题展示 UI - 空洞骑士风格
/// 进入关卡时显示全屏白字标题，渐入后停留几秒再渐出
/// </summary>
public class LevelTitleUI : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text titleText; // 使用 Unity 原生 Text 组件
    [SerializeField] private Image decorationTop;    // 上方装饰
    [SerializeField] private Image decorationBottom; // 下方装饰
    
    [Header("动画设置")]
    [SerializeField] private float fadeInDuration = 1.0f;   // 渐入时间
    [SerializeField] private float displayDuration = 2.5f;  // 显示时间
    [SerializeField] private float fadeOutDuration = 1.0f;  // 渐出时间
    [SerializeField] private float startDelay = 0.5f;       // 开始前延迟
    
    [Header("文字动画")]
    [SerializeField] private bool useTextScale = true;      // 是否使用文字缩放动画
    [SerializeField] private float textStartScale = 0.8f;   // 文字起始缩放
    [SerializeField] private float textEndScale = 1.0f;     // 文字结束缩放
    
    [Header("关卡标题配置")]
    [SerializeField] private string levelTitle = "";        // 当前关卡标题（可在 Inspector 设置）
    [SerializeField] private bool autoDetectLevel = true;   // 是否自动检测关卡
    
    // 关卡名称映射
    private static readonly System.Collections.Generic.Dictionary<string, string> LevelTitles = 
        new System.Collections.Generic.Dictionary<string, string>
    {
        { "Level_0", "虚睿之牢" },
        { "Level_1", "镜像之渊" },
        { "Level_2", "童真之堡" }
    };
    
    private RectTransform titleRectTransform;
    private bool isPlaying = false;

    private void Awake()
    {
        if (titleText != null)
        {
            titleRectTransform = titleText.GetComponent<RectTransform>();
        }
        
        // 初始隐藏
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        // 设置为父物体的最后一个子物体，确保最后渲染
        transform.SetAsLastSibling();
    }

    private void Start()
    {
        // 自动检测关卡并设置标题
        if (autoDetectLevel)
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (LevelTitles.TryGetValue(sceneName, out string title))
            {
                levelTitle = title;
            }
        }
        
        // 如果有标题，自动播放
        if (!string.IsNullOrEmpty(levelTitle))
        {
            PlayTitleAnimation(levelTitle);
        }
    }

    /// <summary>
    /// 播放标题动画
    /// </summary>
    public void PlayTitleAnimation(string title)
    {
        if (isPlaying) return;
        
        if (titleText != null)
        {
            titleText.text = title;
        }
        
        StartCoroutine(TitleAnimationCoroutine());
    }

    private IEnumerator TitleAnimationCoroutine()
    {
        isPlaying = true;
        
        // 初始状态
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        if (useTextScale && titleRectTransform != null)
        {
            titleRectTransform.localScale = Vector3.one * textStartScale;
        }
        
        // 开始前延迟
        yield return new WaitForSeconds(startDelay);
        
        Debug.Log($"<color=cyan>显示关卡标题: {titleText?.text}</color>");
        
        // 渐入动画
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeInDuration;
            float smoothT = SmoothStep(t);
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = smoothT;
            }
            
            if (useTextScale && titleRectTransform != null)
            {
                float scale = Mathf.Lerp(textStartScale, textEndScale, smoothT);
                titleRectTransform.localScale = Vector3.one * scale;
            }
            
            yield return null;
        }
        
        // 确保完全显示
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        
        if (useTextScale && titleRectTransform != null)
        {
            titleRectTransform.localScale = Vector3.one * textEndScale;
        }
        
        // 显示停留
        yield return new WaitForSeconds(displayDuration);
        
        // 渐出动画
        elapsedTime = 0f;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeOutDuration;
            float smoothT = SmoothStep(t);
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f - smoothT;
            }
            
            yield return null;
        }
        
        // 确保完全隐藏
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        Debug.Log("<color=cyan>关卡标题动画完成</color>");
        
        isPlaying = false;
        
        // 动画结束后可以销毁或隐藏
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 平滑插值函数 (ease in-out)
    /// </summary>
    private float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    /// <summary>
    /// 手动触发显示（供外部调用）
    /// </summary>
    public void ShowTitle(string title)
    {
        gameObject.SetActive(true);
        PlayTitleAnimation(title);
    }

    /// <summary>
    /// 设置标题文本（不播放动画）
    /// </summary>
    public void SetTitle(string title)
    {
        levelTitle = title;
        if (titleText != null)
        {
            titleText.text = title;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("预览动画")]
    private void PreviewAnimation()
    {
        if (Application.isPlaying && !string.IsNullOrEmpty(levelTitle))
        {
            gameObject.SetActive(true);
            PlayTitleAnimation(levelTitle);
        }
    }
#endif
}
