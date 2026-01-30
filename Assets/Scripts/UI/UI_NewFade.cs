using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_NewFade : MonoBehaviour
{
    public Animator newFadeUIAnim { get; private set; }

    private void Awake()
    {
        newFadeUIAnim = GetComponent<Animator>();
    }

    public void DoNewFadeIn(bool needSFX = true)
    {
        if (needSFX)
            AudioManager.instance.PlayGlobalSFX("fade_in");

        newFadeUIAnim.SetBool("newFadeIn", true);
        newFadeUIAnim.SetBool("newFadeOut", false);
    }

    public void DoNewFadeOut(bool needSFX = true)
    {
        AudioManager.instance.StopBGM();

        if (needSFX)
            AudioManager.instance.PlayGlobalSFX("fade_out");

        newFadeUIAnim.SetBool("newFadeOut", true);
        newFadeUIAnim.SetBool("newFadeIn", false);
    }
}
