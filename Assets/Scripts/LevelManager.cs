using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private string musicGroupName;
    UI_NewFade newFadeUI;

    private void Awake()
    {
        newFadeUI = FindFirstObjectByType<UI_NewFade>();
    }

    private void Start()
    {
        AudioManager.instance.PlayBGM(musicGroupName);
        newFadeUI.DoNewFadeIn();
    }
}
