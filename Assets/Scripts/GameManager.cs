using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private UI_Fade fadeUI;
    private Player player;
    private UI_NewFade newFadeUI;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject, 0.01f); // 如果已经有上一个 level 中没有 Destroy 的 GameManager(gameObject)，则沿用 instance，并 Destroy 新的 GameManager(gameObject)
            return;
        }

        instance = this; // 赋值 this 给单例 GameManager
        DontDestroyOnLoad(gameObject);
    }

    public void StartPlay() // 临时实现，较为简单，未实现保存功能
    {
        StartCoroutine(ChangeSceneCo("dialog1"));
    }

    public void BackToMainMenu()
    {
        StartCoroutine(ChangeSceneCo("MainMenu"));
    }

    public void ChangeSceneTo(string sceneName)
    {
        StartCoroutine(ChangeSceneCo(sceneName));
    }

    private IEnumerator ChangeSceneCo(string sceneName)
    {
        //GetFadeUI().DoFadeOut();

        //yield return GetFadeUI().fadeEffectCo; // fadeEffectCo 指向正在运行的 ChangeAlphaCo(), 等 ChangeAlphaCo() Coroutine 结束后返回

        GetNewFadeUI().DoNewFadeOut();

        float duration = GetNewFadeUI().newFadeUIAnim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(duration);

        SceneManager.LoadScene(sceneName);
    }

    public void DeathTransition()
    {
        StartCoroutine(DeathTransitionCo());
    }

    private IEnumerator DeathTransitionCo()
    {
        yield return new WaitForSeconds(2);

        GetNewFadeUI().DoNewFadeOut(needSFX : false);

        float duration = GetNewFadeUI().newFadeUIAnim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(duration);

        GetPlayer().ui.OpenDeathScreenUI();

        GetNewFadeUI().DoNewFadeIn(needSFX : false);
    }

    private Player GetPlayer()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();

        return player;
    }

    private UI_Fade GetFadeUI()
    {
        if (fadeUI == null)
            fadeUI = FindFirstObjectByType<UI_Fade>();

        return fadeUI;
    }

    private UI_NewFade GetNewFadeUI()
    {
        if (newFadeUI == null)
        {
            newFadeUI = FindFirstObjectByType<UI_NewFade>();
        }
        return newFadeUI;
    }
}
