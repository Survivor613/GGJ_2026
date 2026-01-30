using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UI_Options : MonoBehaviour
{
    [Header("BGM Volume Settings")]
    [SerializeField] private Slider bgmSlider; // unimportant

    [Header("SFX Volume Settings")]
    [SerializeField] private Slider sfxSlider; // unimportant

    private void OnEnable()
    {
        if (AudioManager.instance != null)
        {
            bgmSlider.value = AudioManager.instance.currentBgmValue;
            sfxSlider.value = AudioManager.instance.currentSfxValue;
        }
        else
            Debug.Log("Cannot find AudioManager in UI_Options:OnEnable");
    }

    public void BGMSliderValue(float value)
    {
        AudioManager.instance.SetBGMVolume(value);
    }

    public void SFXSliderValue(float value)
    {
        AudioManager.instance.SetSFXVolume(value);
    }

    public void GoMainMenuBTN()
    {
        //AudioManager.instance.PlayGlobalSFX("button_click");
        GameManager.instance.BackToMainMenu();
    }

    public void ExitOptionBTN()
    {
        //AudioManager.instance.PlayGlobalSFX("button_click");
    }
}
