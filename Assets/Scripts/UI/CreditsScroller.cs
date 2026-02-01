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
        [SerializeField] private bool loop = false;
        [SerializeField] private float restartDelay = 1f;
        
        [Header("Scene Transition")]
        [SerializeField] private bool loadSceneOnComplete = true;
        [SerializeField] private string targetSceneName = "MainMenu";
        [SerializeField] private float delayBeforeLoad = 1f;

        private RectTransform contentRect;
        private RectTransform viewportRect;
        private float restartTimer;
        private bool hasFinished = false;

        private void Awake()
        {
            ApplyCreditsContent();
            CacheRects();
            RefreshLayoutAndReset();
        }

        private void OnValidate()
        {
            ApplyCreditsContent();
            CacheRects();
            RefreshLayoutAndReset();
        }

        private void Update()
        {
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

            Vector2 pos = contentRect.anchoredPosition;
            pos.y += scrollSpeed * Time.deltaTime;
            contentRect.anchoredPosition = pos;

            // 滚动到内容完全消失在上方（整个内容高度都滚过去）
            float completeScrollY = contentHeight;
            
            // 调试日志（每秒输出一次进度）
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"滚动进度: {pos.y:F1} / {completeScrollY:F1} (loop={loop})");
            }
            
            if (pos.y >= completeScrollY - 10f) // 留一点余量避免浮点误差
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
                }
            }
        }
        
        private void LoadTargetScene()
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogWarning("目标场景名为空，无法跳转");
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
