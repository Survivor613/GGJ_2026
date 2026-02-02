using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace GameUI
{
    /// <summary>
    /// 简单滚动制作人名单
    /// </summary>
    public class CreditsScroller : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Text creditsText;

        [Header("Content")]
        [TextArea(6, 20)]
        [SerializeField] private string creditsContent;

        [Header("Scroll")]
        [SerializeField] private bool autoScroll = true;
        [SerializeField] private float scrollSpeed = 20f;
        [SerializeField] private float speedUpMultiplier = 3f; // 加速倍数
        [SerializeField] private bool enableSlowDown = true; // 是否启用结束减速
        [SerializeField] private float slowDownDistance = 100f; // 接近终点多少像素开始减速
        [SerializeField] private bool loop = false;
        [SerializeField] private float restartDelay = 1f;
        [SerializeField] private float finishOffset = 400f; // 提前结束偏移量（像素），避免等待尾部空行
        
        [Header("Scene Transition")]
        [SerializeField] private bool loadSceneOnComplete = true;
        [SerializeField] private string targetSceneName = "MainMenuNew";
        [SerializeField] private float delayBeforeLoad = 1f;
        [SerializeField] private bool useFadeOut = true; // 是否使用淡出效果
        [SerializeField] private CanvasGroup fadeCanvasGroup; // 用于淡出的 CanvasGroup
        [SerializeField] private float fadeDuration = 1.5f; // 淡出时间
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 淡出曲线

        private RectTransform contentRect;
        private RectTransform viewportRect;
        private float restartTimer;
        private bool hasFinished = false;
        private bool isSpeedingUp = false; // 是否正在加速
        private bool isFading = false; // 是否正在淡出

        private void Awake()
        {
            ApplyCreditsContent();
            CacheRects();
        }
        
        private void Start()
        {
            // 禁用 ScrollRect 的手动滚动，避免干扰脚本控制
            if (scrollRect != null)
            {
                scrollRect.enabled = false;
                Debug.Log("<color=yellow>已禁用 ScrollRect 组件，使用脚本控制滚动</color>");
            }
            
            // 延迟一帧确保 Layout 组件完成计算
            StartCoroutine(DelayedRefresh());
        }
        
        private System.Collections.IEnumerator DelayedRefresh()
        {
            // 等待一帧，让 Content Size Fitter 和 Layout Group 完成计算
            yield return null;
            RefreshLayoutAndReset();
            Debug.Log($"<color=green>Credits 初始化完成，Content 高度: {contentRect.rect.height:F1}</color>");
        }

        private void OnValidate()
        {
            ApplyCreditsContent();
            CacheRects();
            RefreshLayoutAndReset();
        }

        private void Update()
        {
            // 检测按键输入
            HandleInput();
            
            // 优先处理完成后的倒计时跳转（不受 autoScroll 影响）
            if (hasFinished && restartTimer > 0f)
            {
                restartTimer -= Time.deltaTime;
                if (restartTimer <= 0f && loadSceneOnComplete)
                {
                    LoadTargetScene();
                    return;
                }
            }
            
            if (!autoScroll || scrollRect == null || contentRect == null || viewportRect == null)
            {
                return;
            }

            float contentHeight = contentRect.rect.height;
            float viewportHeight = viewportRect.rect.height;
            if (contentHeight <= viewportHeight + 0.5f)
            {
                Debug.LogWarning($"内容高度不足以滚动: content={contentHeight}, viewport={viewportHeight}");
                return;
            }

            // 根据是否加速计算当前滚动速度
            float currentSpeed = isSpeedingUp ? scrollSpeed * speedUpMultiplier : scrollSpeed;
            
            Vector2 pos = contentRect.anchoredPosition;
            
            // 计算滚动终点
            float completeScrollY = contentHeight - (viewportHeight * 0.5f);
            completeScrollY -= finishOffset;
            completeScrollY = Mathf.Max(0, completeScrollY);
            
            // 接近终点时减速，让停止更平滑
            if (enableSlowDown && !isSpeedingUp)
            {
                float distanceToEnd = completeScrollY - pos.y;
                if (distanceToEnd > 0 && distanceToEnd < slowDownDistance)
                {
                    // 使用平滑曲线减速（0.2 是最低速度倍数）
                    float slowDownFactor = Mathf.Lerp(0.2f, 1f, distanceToEnd / slowDownDistance);
                    currentSpeed *= slowDownFactor;
                }
            }
            
            pos.y += currentSpeed * Time.deltaTime;
            contentRect.anchoredPosition = pos;
            
            // 调试日志（每秒输出一次进度）
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"滚动进度: {pos.y:F1} / {completeScrollY:F1} (content={contentHeight:F1}, viewport={viewportHeight:F1}, offset={finishOffset})");
            }
            
            if (pos.y >= completeScrollY || Mathf.Approximately(pos.y, completeScrollY)) // 到达预设的完成点或非常接近
            {
                if (loop)
                {
                    Debug.Log("检测到滚动完成但开启了循环，重新开始");
                    contentRect.anchoredPosition = new Vector2(pos.x, 0f);
                    restartTimer = restartDelay;
                }
                else
                {
                    autoScroll = false;
                    hasFinished = true;
                    restartTimer = delayBeforeLoad;
                    Debug.Log($"<color=yellow>制作人名单滚动完成，{delayBeforeLoad} 秒后跳转到 {targetSceneName}</color>");
                    
                    // 开始淡出
                    if (useFadeOut && fadeCanvasGroup != null)
                    {
                        StartCoroutine(FadeOutAndLoad());
                    }
                }
            }
        }
        
        private void HandleInput()
        {
            // 按 Enter 或 Return 键加速
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (!hasFinished)
                {
                    isSpeedingUp = true;
                    Debug.Log("<color=cyan>开始加速滚动</color>");
                }
                else
                {
                    // 如果已经完成，立即跳转
                    LoadTargetScene();
                }
            }
            
            // 松开 Enter 键停止加速
            if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
            {
                if (isSpeedingUp)
                {
                    isSpeedingUp = false;
                    Debug.Log("<color=cyan>停止加速滚动</color>");
                }
            }
            
            // 按 Escape 键直接跳转到主界面
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("<color=yellow>按下 Escape，直接跳转到主界面</color>");
                LoadTargetScene();
            }
        }
        
        private void LoadTargetScene()
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogWarning("目标场景名为空，无法跳转");
                return;
            }
            
            // 如果还没开始淡出，先淡出再跳转
            if (useFadeOut && fadeCanvasGroup != null && !isFading)
            {
                StartCoroutine(FadeOutAndLoad());
                return;
            }
            
            Debug.Log($"<color=green>制作人名单滚动完成，正在跳转到场景: {targetSceneName}</color>");
            try
            {
                SceneManager.LoadScene(targetSceneName);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"跳转场景失败: {e.Message}\n请确认场景 '{targetSceneName}' 已添加到 Build Settings");
            }
        }
        
        private System.Collections.IEnumerator FadeOutAndLoad()
        {
            isFading = true;
            float elapsedTime = 0f;
            float startAlpha = fadeCanvasGroup.alpha;
            
            Debug.Log("<color=cyan>开始平滑淡出动画</color>");
            
            // 使用 AnimationCurve 实现更顺滑的淡出效果
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / fadeDuration);
                
                // 应用缓动曲线
                float curveValue = fadeCurve.Evaluate(t);
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, curveValue);
                
                yield return null;
            }
            
            fadeCanvasGroup.alpha = 1f;
            
            // 淡出完成后跳转场景
            Debug.Log($"<color=green>淡出完成，正在跳转到场景: {targetSceneName}</color>");
            try
            {
                SceneManager.LoadScene(targetSceneName);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"跳转场景失败: {e.Message}\n请确认场景 '{targetSceneName}' 已添加到 Build Settings");
            }
        }

        private void ApplyCreditsContent()
        {
            if (creditsText != null && !string.IsNullOrEmpty(creditsContent))
            {
                creditsText.text = creditsContent;
            }
        }

        private void CacheRects()
        {
            if (scrollRect == null || creditsText == null) return;
            if (scrollRect.viewport != null)
            {
                viewportRect = scrollRect.viewport;
            }
            // Content 默认是文本父物体
            if (creditsText.rectTransform.parent is RectTransform parentRect)
            {
                contentRect = parentRect;
            }
            else
            {
                contentRect = creditsText.rectTransform;
            }
        }

        private void RefreshLayoutAndReset()
        {
            if (contentRect == null || viewportRect == null) return;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            contentRect.anchoredPosition = Vector2.zero;
        }
    }
}
