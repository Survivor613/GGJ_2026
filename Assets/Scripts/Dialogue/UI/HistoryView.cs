using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DialogueSystem.UI
{
    public class HistoryView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private GameObject entryPrefab;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private bool closeOnBackgroundClick = true;
        [SerializeField] private float scrollSensitivity = 20f;

        private List<GameObject> entries = new List<GameObject>();
        private const int MAX_ENTRIES = 100;

        public bool IsOpen => panel != null && panel.activeSelf;

        private void Awake()
        {
            ApplyScrollSensitivity();
        }

        private void OnValidate()
        {
            ApplyScrollSensitivity();
        }

        public void AddEntry(string speaker, string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            // Remove tags for history
            string cleanText = System.Text.RegularExpressions.Regex.Replace(text, @"\[.*?\]", "");
            string cleanSpeaker = NormalizeSpeaker(speaker);

            HistoryEntryView entry = null;
            if (entryPrefab != null)
            {
                var go = Instantiate(entryPrefab, contentRoot);
                entry = go.GetComponent<HistoryEntryView>();
                if (entry == null)
                {
                    Debug.LogWarning("HistoryEntry Prefab 缺少 HistoryEntryView 组件，尝试创建默认条目。");
                    Destroy(go);
                }
                else
                {
                    entry.EnsureBindings();
                    if (!entry.HasValidBindings())
                    {
                        Debug.LogWarning("HistoryEntry Prefab 没有有效的 Text 绑定，改用默认条目。");
                        Destroy(go);
                        entry = null;
                    }
                }
            }

            if (entry == null)
            {
                entry = CreateFallbackEntry();
            }

            if (entry == null)
            {
                Debug.LogError("无法创建 HistoryEntry 条目。");
                return;
            }

            entry.SetData(cleanSpeaker, cleanText);
            entries.Add(entry.gameObject);

            if (entries.Count > MAX_ENTRIES)
            {
                Destroy(entries[0]);
                entries.RemoveAt(0);
            }

            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }

        public void Toggle(bool show)
        {
            panel.SetActive(show);
            if (show)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private HistoryEntryView CreateFallbackEntry()
        {
            var entryGO = new GameObject("HistoryEntry");
            entryGO.transform.SetParent(contentRoot, false);
            var rect = entryGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(500, 80);

            var layout = entryGO.AddComponent<LayoutElement>();
            layout.preferredHeight = 80;
            layout.flexibleHeight = -1;

            var bg = entryGO.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

            var nameGO = new GameObject("NameText");
            nameGO.transform.SetParent(entryGO.transform, false);
            var nameRect = nameGO.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 1);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.pivot = new Vector2(0.5f, 1);
            nameRect.anchoredPosition = new Vector2(0, -5);
            nameRect.sizeDelta = new Vector2(-10, 25);

            var nameText = nameGO.AddComponent<Text>();
            nameText.fontSize = 18;
            nameText.fontStyle = FontStyle.Bold;
            nameText.color = Color.yellow;
            nameText.supportRichText = true;
            nameText.horizontalOverflow = HorizontalWrapMode.Wrap;
            nameText.verticalOverflow = VerticalWrapMode.Truncate;
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var contentGO = new GameObject("ContentText");
            contentGO.transform.SetParent(entryGO.transform, false);
            var contentRect = contentGO.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = new Vector2(0, -30);
            contentRect.sizeDelta = new Vector2(-10, -35);

            var contentText = contentGO.AddComponent<Text>();
            contentText.fontSize = 16;
            contentText.color = Color.white;
            contentText.supportRichText = true;
            contentText.horizontalOverflow = HorizontalWrapMode.Wrap;
            contentText.verticalOverflow = VerticalWrapMode.Truncate;
            contentText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var entry = entryGO.AddComponent<HistoryEntryView>();
            entry.EnsureBindings();
            return entry;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!closeOnBackgroundClick || panel == null || !panel.activeSelf) return;

            // 仅当点击的是背景面板本身时关闭（不影响滚动区域）
            if (eventData.pointerPressRaycast.gameObject == panel)
            {
                Toggle(false);
            }
        }

        private string NormalizeSpeaker(string speaker)
        {
            if (string.IsNullOrEmpty(speaker)) return string.Empty;

            // 去除 BOM 和零宽字符
            string result = speaker
                .Replace("\uFEFF", string.Empty)
                .Replace("\u200B", string.Empty)
                .Trim();

            return result;
        }

        private void ApplyScrollSensitivity()
        {
            if (scrollRect != null)
            {
                scrollRect.scrollSensitivity = scrollSensitivity;
            }
        }
    }
}
