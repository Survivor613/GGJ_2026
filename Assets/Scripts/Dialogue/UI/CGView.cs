using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystem.UI
{
    /// <summary>
    /// 简单的 CG 展示视图（基于 Unity UI Image）
    /// </summary>
    public class CGView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Image image;

        private void Awake()
        {
            if (panel == null)
            {
                panel = gameObject;
            }
        }

        public void Show(Sprite sprite)
        {
            if (panel != null) panel.SetActive(true);
            if (image != null)
            {
                image.sprite = sprite;
                image.enabled = sprite != null;
            }
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}
