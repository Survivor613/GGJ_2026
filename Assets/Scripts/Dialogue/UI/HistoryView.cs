using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystem.UI
{
    public class HistoryView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private GameObject entryPrefab;
        [SerializeField] private ScrollRect scrollRect;

        private List<GameObject> entries = new List<GameObject>();
        private const int MAX_ENTRIES = 100;

        public void AddEntry(string speaker, string text)
        {
            // Remove tags for history
            string cleanText = System.Text.RegularExpressions.Regex.Replace(text, @"\[.*?\]", "");
            
            var go = Instantiate(entryPrefab, contentRoot);
            var entry = go.GetComponent<HistoryEntryView>();
            if (entry == null)
            {
                Debug.LogError("HistoryEntry Prefab 缺少 HistoryEntryView 组件。");
                Destroy(go);
                return;
            }

            entry.SetData(speaker, cleanText);
            entries.Add(go);

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
    }
}
