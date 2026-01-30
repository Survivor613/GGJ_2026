using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_DeathScreen : MonoBehaviour
{
    public void RestartGameBTN()
    {
        //AudioManager.instance.PlayGlobalSFX("button_click");
        GameManager.instance.StartPlay();
    }

    public void GoMainMenuBTN()
    {
        //AudioManager.instance.PlayGlobalSFX("button_click");
        GameManager.instance.BackToMainMenu();
    }
}
