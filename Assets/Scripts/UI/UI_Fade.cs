using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Fade : MonoBehaviour
{
    [Header("Fade UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1.5f;
    public Coroutine fadeEffectCo { get; private set; } // Coroutine类型变量指向ChangeAlphaCo()

    [Header("Fade for Specific UI")]
    private Dictionary<CanvasGroup, Coroutine> activeFades = new Dictionary<CanvasGroup, Coroutine>();

    private void Start()
    {
        //DoFadeIn();
    }

    public void DoFadeIn() // black => transparent
    {
        //AudioManager.instance.PlayGlobalSFX("fade_in"); // Fade In SFX
        FadeEffect(0);
    }

    public void DoFadeOut() // transparent => black
    {
        //AudioManager.instance.StopBGM();
        //AudioManager.instance.PlayGlobalSFX("fade_out"); // Fade Out SFX
        FadeEffect(1);
    }

    private void FadeEffect(float targetAlpha)
    {
        if (fadeEffectCo != null)
            StopCoroutine(fadeEffectCo);

       fadeEffectCo = StartCoroutine(ChangeAlphaCo(targetAlpha));
    }

    private IEnumerator ChangeAlphaCo(float targetAlpha)
    {
        float timePass = 0;
        float startAlpha = canvasGroup.alpha;

        while (timePass < fadeDuration)
        {
            timePass += Time.deltaTime;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timePass / fadeDuration); // 线性插值

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    public void DoFadeInForSpecificUI(CanvasGroup canvasGroup, float notificationUIFadeDuration) => FadeEffectForSpecificUI(canvasGroup, notificationUIFadeDuration, 1);  // transparent => black

    public void DoFadeOutForSpecificUI(CanvasGroup canvasGroup, float notificationUIFadeDuration) => FadeEffectForSpecificUI(canvasGroup, notificationUIFadeDuration, 0); // black => transparent

    private void FadeEffectForSpecificUI(CanvasGroup canvasGroup, float notificationUIFadeDuration, float targetAlpha)
    {
        // 1. 检查这个特定的 canvasGroup 是否已经在跑协程了
        if (activeFades.ContainsKey(canvasGroup))
        {
            // 如果在跑，只停止属于这个 canvasGroup 的协程，不影响别人
            if (activeFades[canvasGroup] != null)
                StopCoroutine(activeFades[canvasGroup]);

            activeFades.Remove(canvasGroup);
        }

        // 2. 开启新协程，并记录到字典中
        Coroutine newFade = StartCoroutine(ChangeAlphaForSpecificUICo(canvasGroup, notificationUIFadeDuration, targetAlpha));
        activeFades.Add(canvasGroup, newFade);
    }

    private IEnumerator ChangeAlphaForSpecificUICo(CanvasGroup canvasGroup, float notificationUIFadeDuration, float targetAlpha)
    {
        float timePass = 0;
        float startAlpha = canvasGroup.alpha;

        while (timePass < notificationUIFadeDuration)
        {
            timePass += Time.deltaTime;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timePass / notificationUIFadeDuration);

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        // 3. 运行结束，从字典中移除，释放内存
        if (activeFades.ContainsKey(canvasGroup))
            activeFades.Remove(canvasGroup);
    }
}
