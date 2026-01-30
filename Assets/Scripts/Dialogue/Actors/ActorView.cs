using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystem.Actors
{
    public class ActorView : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private CanvasGroup canvasGroup;
        
        public string ActorId { get; private set; }
        
        private Coroutine fadeCo;
        private Coroutine colorCo;
        private Coroutine scaleCo;

        public void Init(string id)
        {
            ActorId = id;
            canvasGroup.alpha = 0;
        }

        public void SetPortrait(Sprite sprite)
        {
            portraitImage.sprite = sprite;
        }

        public void Show(float duration = 0.2f)
        {
            StopFade();
            fadeCo = StartCoroutine(FadeRoutine(1, duration));
        }

        public void Hide(float duration = 0.2f)
        {
            StopFade();
            fadeCo = StartCoroutine(FadeRoutine(0, duration));
        }

        public void SetFocus(bool focused, float duration = 0.2f)
        {
            StopColor();
            StopScale();
            
            Color targetColor = focused ? Color.white : new Color(0.6f, 0.6f, 0.6f, 1f);
            Vector3 targetScale = focused ? Vector3.one * 1.05f : Vector3.one;
            
            colorCo = StartCoroutine(ColorRoutine(targetColor, duration));
            scaleCo = StartCoroutine(ScaleRoutine(targetScale, duration));
        }

        private void StopFade() { if (fadeCo != null) StopCoroutine(fadeCo); }
        private void StopColor() { if (colorCo != null) StopCoroutine(colorCo); }
        private void StopScale() { if (scaleCo != null) StopCoroutine(scaleCo); }

        private IEnumerator FadeRoutine(float targetAlpha, float duration)
        {
            float startAlpha = canvasGroup.alpha;
            float time = 0;
            while (time < duration)
            {
                time += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                yield return null;
            }
            canvasGroup.alpha = targetAlpha;
            if (targetAlpha <= 0) gameObject.SetActive(false);
        }

        private IEnumerator ColorRoutine(Color targetColor, float duration)
        {
            Color startColor = portraitImage.color;
            float time = 0;
            while (time < duration)
            {
                time += Time.deltaTime;
                portraitImage.color = Color.Lerp(startColor, targetColor, time / duration);
                yield return null;
            }
            portraitImage.color = targetColor;
        }

        private IEnumerator ScaleRoutine(Vector3 targetScale, float duration)
        {
            Vector3 startScale = transform.localScale;
            float time = 0;
            while (time < duration)
            {
                time += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, targetScale, time / duration);
                yield return null;
            }
            transform.localScale = targetScale;
        }
    }
}
