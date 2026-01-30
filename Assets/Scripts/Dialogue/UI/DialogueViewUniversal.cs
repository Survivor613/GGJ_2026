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

        public bool IsTypewriterPlaying => typewriter != null && typewriter.IsPlaying;

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
