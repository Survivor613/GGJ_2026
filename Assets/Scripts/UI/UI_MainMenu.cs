using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_MainMenu : MonoBehaviour
{
    UI_NewFade newFadeUI;

    private void Awake()
    {
        newFadeUI = FindFirstObjectByType<UI_NewFade>();
    }

    private void Start()
    {
        newFadeUI.DoNewFadeIn(needSFX: false);
        AudioManager.instance.PlayBGM("playlist_mainMenu");
    }

    public void PlayBTN()
    {
        //AudioManager.instance.PlayGlobalSFX("button_click");
        GameManager.instance.StartPlay();
    }

    public void OptionsBTN()
    {
        //AudioManager.instance.PlayGlobalSFX("button_click");
    }

    public void QuitGameBTN()
    {
        //AudioManager.instance.PlayGlobalSFX("button_click");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
