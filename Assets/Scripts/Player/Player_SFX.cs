using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_SFX : Entity_SFX
{
    [Header("SFX Names")]
    [SerializeField] private string attackHit;
    [SerializeField] private string attackMiss;
    [SerializeField] private string dash;
    [SerializeField] private string jump;
    [SerializeField] private string jumpLand;
    [SerializeField] private string move;
    [SerializeField] private string death;

    Coroutine playerMoveCo;

    public override void EntityAttackHit()
    {
        AudioManager.instance.PlaySFX(attackHit, audioSource);
    }

    public override void EntityAttackMiss()
    {
        AudioManager.instance.PlaySFX(attackMiss, audioSource);
    }

    public override void EntityDash()
    {
        AudioManager.instance.PlaySFX(dash, audioSource);
    }

    public override void EntityJump()
    {
        AudioManager.instance.PlaySFX(jump, audioSource);
    }

    public override void EntityJumpLand()
    {
        AudioManager.instance.PlaySFX(jumpLand, audioSource);
    }

    public override void EntityMove()
    {
        if (playerMoveCo == null)
            playerMoveCo = StartCoroutine(PlayerMoveCo());
    }

    IEnumerator PlayerMoveCo()
    {
        AudioManager.instance.PlaySFX(move, audioSource);

        yield return new WaitForSeconds(0.5f);
        playerMoveCo = null;
    }

    public override void EntityDeath()
    {
        AudioManager.instance.PlaySFX(death, audioSource);
    }
}
