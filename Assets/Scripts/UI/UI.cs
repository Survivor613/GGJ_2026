using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class UI : MonoBehaviour
{
    [SerializeField] private GameObject[] uiElements;
    private PlayerInputSet input;

    #region UI Components
    public UI_Options optionsUI { get; private set; }
    public UI_DeathScreen deathScreenUI { get; private set; }



    private void Awake()
    {
        optionsUI = GetComponentInChildren<UI_Options>(true); // 括号中true参数表示强制搜索隐藏物体，否则GetComponent默认只寻找处于激活状态的物体
        deathScreenUI = GetComponentInChildren<UI_DeathScreen>(true);
    }

    public void SetupControlsUI(PlayerInputSet inputSet)
    {
        input = inputSet;

        input.UI.OptionsUI.performed += ctx =>
        {
            foreach (var element in uiElements)
            {
                if (element.activeSelf)
                {
                    SwitchToInGameUI();
                    return;
                }
            }

            OpenOptionsUI();
        };
    }

    public void OpenDeathScreenUI()
    {
        foreach (var element in uiElements)
            element.gameObject.SetActive(false);

        StopPlayerControls(true);
        deathScreenUI.gameObject.SetActive(true);
    }

    public void OpenOptionsUI()
    {
        AudioManager.instance.PlayGlobalSFX("button_click");

        foreach (var element in uiElements)
            element.gameObject.SetActive(false);

        StopPlayerControls(true);
        optionsUI.gameObject.SetActive(true);
    }

    public void SwitchToInGameUI()
    {
        AudioManager.instance.PlayGlobalSFX("button_click");

        foreach (var element in uiElements)
            element.gameObject.SetActive(false);

        StopPlayerControls(false);
        //inGameUI.gameObject.SetActive(true);
    }

    private void StopPlayerControls(bool stopControls)
    {
        if (stopControls)
            input.Player.Disable();
        else
            input.Player.Enable();
    }
}

#endregion