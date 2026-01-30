using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Notification : MonoBehaviour
{
    [SerializeField] private UI_Fade fadeUI;
    [SerializeField] private GameObject notificationUI;
    [SerializeField] private float notificationUIFadeDuration = .5f;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        fadeUI = FindFirstObjectByType<UI_Fade>(FindObjectsInactive.Include);
        canvasGroup.alpha = 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 玩家进入区域后显示操作提示
        if (other.CompareTag("Player"))
        {
            DisplayNotification();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 玩家离开区域后隐藏操作提示
        if (other.CompareTag("Player"))
        {
            HideNotification();
        }
    }

    private void DisplayNotification()
    {
        fadeUI.DoFadeInForSpecificUI(canvasGroup, notificationUIFadeDuration);
        
    }

    private void HideNotification()
    {
        fadeUI.DoFadeOutForSpecificUI(canvasGroup, notificationUIFadeDuration);
    }
}
