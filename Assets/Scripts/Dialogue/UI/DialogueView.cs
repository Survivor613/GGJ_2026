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

        public bool IsTypewriterPlaying => typewriter.IsPlaying;

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
