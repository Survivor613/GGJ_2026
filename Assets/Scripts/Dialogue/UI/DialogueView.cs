using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace DialogueSystem.UI
{
    public class DialogueView : MonoBehaviour, IDialogueView
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private GameObject continueIcon;
        [SerializeField] private TypewriterEffect typewriter;
        [Header("Text Scale")]
        [Range(0.5f, 3f)]
        [SerializeField] private float textScale = 1f;
        [SerializeField] private float nameBaseSize = 28f;
        [SerializeField] private float bodyBaseSize = 24f;

        public bool IsTypewriterPlaying => typewriter.IsPlaying;

        private void Awake()
        {
            ApplyTextScale();
        }

        private void OnValidate()
        {
            ApplyTextScale();
        }

        private void ApplyTextScale()
        {
            if (nameText != null)
            {
                nameText.fontSize = Mathf.Max(1f, nameBaseSize * Mathf.Max(0.5f, textScale));
                nameText.enabled = true;
                nameText.gameObject.SetActive(true);
            }
            if (bodyText != null)
            {
                bodyText.fontSize = Mathf.Max(1f, bodyBaseSize * Mathf.Max(0.5f, textScale));
                bodyText.enabled = true;
                bodyText.gameObject.SetActive(true);
            }
        }

        public void ShowLine(string speaker, string text)
        {
            panel.SetActive(true);
            nameText.text = speaker;
            continueIcon.SetActive(false);
            typewriter.Play(text);
        }

        public void SkipTypewriter()
        {
            typewriter.Skip();
        }

        public void ShowContinueIcon(bool show)
        {
            if (continueIcon != null) continueIcon.SetActive(show);
        }

        public void Hide()
        {
            panel.SetActive(false);
        }
    }
}
