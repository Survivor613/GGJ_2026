using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystem.UI
{
    public class HistoryEntryView : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text contentText;
        [SerializeField] private bool autoResize = true;
        [SerializeField] private float extraPadding = 8f;

        private void Awake()
        {
            EnsureBindings();
        }

        public bool HasValidBindings()
        {
            return nameText != null && contentText != null;
        }

        public void EnsureBindings()
        {
            // 兼容旧预制体或引用丢失时的自动绑定
            if (nameText != null && contentText != null) return;

            var texts = GetComponentsInChildren<Text>(true);
            foreach (var text in texts)
            {
                if (text.gameObject.name == "NameText")
                {
                    nameText = text;
                }
                else if (text.gameObject.name == "ContentText")
                {
                    contentText = text;
                }
            }
        }

        public void SetData(string name, string content)
        {
            if (nameText == null || contentText == null)
            {
                Debug.LogError("HistoryEntryView 缺少 Text 引用，无法设置历史记录文本。");
                return;
            }

            bool showName = !string.IsNullOrWhiteSpace(name);
            nameText.gameObject.SetActive(showName);
            nameText.enabled = showName;
            if (showName)
            {
                nameText.text = name;
                nameText.color = Color.yellow;
                nameText.fontStyle = FontStyle.Bold;
            }
            contentText.text = content;

            if (autoResize)
            {
                ResizeToContent(showName);
            }
        }

        private void ResizeToContent(bool showName)
        {
            // 调整子文本框高度，避免被裁切
            if (nameText != null)
            {
                ResizeRectToText(nameText);
            }
            if (contentText != null)
            {
                ResizeRectToText(contentText);
            }

            // 调整条目自身高度（若存在 LayoutElement）
            var layout = GetComponent<LayoutElement>();
            if (layout != null)
            {
                float total = 0f;
                if (showName && nameText != null)
                {
                    total += nameText.preferredHeight;
                }
                if (contentText != null)
                {
                    total += contentText.preferredHeight;
                }
                layout.preferredHeight = Mathf.Max(40f, total + extraPadding);
            }
        }

        private void ResizeRectToText(Text text)
        {
            RectTransform rect = text.rectTransform;
            if (rect == null) return;
            float preferred = text.preferredHeight;
            Vector2 size = rect.sizeDelta;
            size.y = Mathf.Max(1f, preferred);
            rect.sizeDelta = size;
        }
    }
}
