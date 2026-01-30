using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystem.UI
{
    public class HistoryEntryView : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text contentText;

        private void Awake()
        {
            // 兼容旧预制体或引用丢失时的自动绑定
            if (nameText == null || contentText == null)
            {
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
        }

        public void SetData(string name, string content)
        {
            if (nameText == null || contentText == null)
            {
                Debug.LogError("HistoryEntryView 缺少 Text 引用，无法设置历史记录文本。");
                return;
            }

            nameText.text = name;
            contentText.text = content;
        }
    }
}
