using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest_SFX : Entity_SFX
{
    [Header("SFX Names")]
    [SerializeField] private string open;

    public override void EntityOpen()
    {
        base.EntityOpen();
        AudioManager.instance.PlaySFX(open, audioSource);
    }
}
