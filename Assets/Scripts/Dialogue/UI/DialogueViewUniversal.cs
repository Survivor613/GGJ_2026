using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystem.UI
{
    /// <summary>
    /// 通用对话视图 - 同时支持 Unity Text 和 TextMeshPro
    /// 用于原生 Text 版本
    /// </summary>
    public class DialogueViewUniversal : MonoBehaviour, IDialogueView
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text nameText;  // Unity Text
        [SerializeField] private Text bodyText;  // Unity Text
        [SerializeField] private GameObject continueIcon;
        [SerializeField] private TypewriterEffectUniversal typewriter;
        [Header("Text Scale")]
        [Range(0.5f, 3f)]
        [SerializeField] private float textScale = 1f;
        [SerializeField] private int nameBaseSize = 28;
        [SerializeField] private int bodyBaseSize = 24;
        [SerializeField] private bool applyTextScale = true;
        [Header("Best Fit")]
        [SerializeField] private bool nameBestFit = true;
        [SerializeField] private int nameMinSize = 16;
        [SerializeField] private int nameMaxSize = 48;
        [SerializeField] private bool bodyBestFit = false;
        [SerializeField] private int bodyMinSize = 18;
        [SerializeField] private int bodyMaxSize = 48;
        [Header("Auto Resize Rect")]
        [SerializeField] private bool autoResizeNameRect = true;
        [SerializeField] private bool autoResizeBodyRect = false;

        public bool IsTypewriterPlaying => typewriter != null && typewriter.IsPlaying;

        private void Awake()
        {
            ApplyTextScale();
            ClearTexts();
        }

        private void ClearTexts()
        {
            if (nameText != null) nameText.text = string.Empty;
            if (bodyText != null) bodyText.text = string.Empty;
        }

        private void OnValidate()
        {
            ApplyTextScale();
        }

        private void ApplyTextScale()
        {
            if (nameText != null)
            {
                if (applyTextScale)
                {
                    nameText.fontSize = Mathf.Max(1, Mathf.RoundToInt(nameBaseSize * Mathf.Max(0.5f, textScale)));
                }
                nameText.enabled = true;
                nameText.gameObject.SetActive(true);
                nameText.resizeTextForBestFit = nameBestFit;
                nameText.resizeTextMinSize = Mathf.Max(1, nameMinSize);
                nameText.resizeTextMaxSize = Mathf.Max(nameText.resizeTextMinSize, nameMaxSize);
                nameText.horizontalOverflow = HorizontalWrapMode.Wrap;
                nameText.verticalOverflow = VerticalWrapMode.Truncate;

                if (autoResizeNameRect)
                {
                    ResizeRectToText(nameText);
                }
            }
            if (bodyText != null)
            {
                if (applyTextScale)
                {
                    bodyText.fontSize = Mathf.Max(1, Mathf.RoundToInt(bodyBaseSize * Mathf.Max(0.5f, textScale)));
                }
                bodyText.enabled = true;
                bodyText.gameObject.SetActive(true);
                bodyText.resizeTextForBestFit = bodyBestFit;
                bodyText.resizeTextMinSize = Mathf.Max(1, bodyMinSize);
                bodyText.resizeTextMaxSize = Mathf.Max(bodyText.resizeTextMinSize, bodyMaxSize);
                bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
                bodyText.verticalOverflow = VerticalWrapMode.Truncate;

                if (autoResizeBodyRect)
                {
                    ResizeRectToText(bodyText);
                }
            }
        }

        private void ResizeRectToText(Text text)
        {
            if (text == null) return;
            RectTransform rect = text.rectTransform;
            if (rect == null) return;

            float preferredHeight = text.preferredHeight;
            Vector2 size = rect.sizeDelta;
            size.y = Mathf.Max(1f, preferredHeight);
            rect.sizeDelta = size;
        }

        public void ShowLine(string speaker, string text)
        {
            if (panel != null) panel.SetActive(true);
            if (nameText != null) nameText.text = speaker;
            if (continueIcon != null) continueIcon.SetActive(false);
            if (typewriter != null) typewriter.Play(text);
        }

        public void SkipTypewriter()
        {
            if (typewriter != null) typewriter.Skip();
        }

        public void ShowContinueIcon(bool show)
        {
            if (continueIcon != null) continueIcon.SetActive(show);
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}
