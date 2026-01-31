using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private string musicGroupName;
    UI_NewFade newFadeUI;
    Player player;

    private void Awake()
    {
        newFadeUI = FindFirstObjectByType<UI_NewFade>();
        player = FindFirstObjectByType<Player>();
    }

    private void Start()
    {
        AudioManager.instance.PlayBGM(musicGroupName);
        newFadeUI.DoNewFadeIn();

        if (SceneManager.GetActiveScene().name == "Level_0"
            || SceneManager.GetActiveScene().name == "Level_1")
            player.canAttack = true;
        else
            player.canAttack = false;
    }
}
