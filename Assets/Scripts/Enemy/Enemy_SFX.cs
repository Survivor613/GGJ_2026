using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_SFX : Entity_SFX
{
    [Header("SFX Names")]
    [SerializeField] private string attack;
    [SerializeField] private string death;
    [SerializeField] private string player_detected;
    [SerializeField] private string skeleton_walk;

    Coroutine enemyMoveCo;

    public override void EntityAttackHit()
    {
        AudioManager.instance.PlaySFX(attack, audioSource);
    }

    public override void EntityAttackMiss()
    {
        AudioManager.instance.PlaySFX(attack, audioSource);
    }

    public override void EntityDeath()
    {
        AudioManager.instance.PlaySFX(death, audioSource);
    }

    public override void EntityPlayerDetected()
    {
        AudioManager.instance.PlaySFX(player_detected, audioSource);
    }

    public override void EntityMove()
    {
        if (enemyMoveCo == null)
            enemyMoveCo = StartCoroutine(EnemyMoveCo());
    }

    IEnumerator EnemyMoveCo()
    {
        AudioManager.instance.PlaySFX(skeleton_walk, audioSource);

        yield return new WaitForSeconds(0.6f);
        enemyMoveCo = null;
    }
}
