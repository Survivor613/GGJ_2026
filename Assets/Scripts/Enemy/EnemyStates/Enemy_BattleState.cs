using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_BattleState : EnemyState
{
    private Transform player;
    private float lastTimeWasInBattle;

    public Enemy_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        UpdateBattleTimer(); // 很重要，防止React阶段玩家反向逃跑！

        player = enemy.GetPlayerReference();

        HandleRetreat();
    }

    public override void Update()
    {
        base.Update();

        if (WithinAttackRange() && enemy.PlayerDetected())  // enemy.PlayerDetection() == true 确保骷髅攻击时正对 Player
            stateMachine.ChangeState(enemy.attackState);
        else if (!WithinAttackRange())  // 在Player距离超过攻击范围时追逐，靠得很近但攻击不到（如不同y轴）时保持静止，有效避免抽搐
            enemy.SetVelocity(enemy.battleMoveSpeed * DirectionToPlayer(), rb.velocity.y);
        else if (WithinAttackRange() && DirectionToPlayer() != enemy.facingDir) // 或者Player在攻击范围内，反方向时，转身即可
            enemy.Flip();
        //HandleRetreat(); // 通过避免玩家过分靠近敌人来解决玩家x轴与敌人几乎对齐时导致的抽搐

        // 加入ReactState后, Update()中的顺序很重要!一定要 先面向Player, 再UpdateBattleTimer! 防止React阶段玩家反向逃跑！(如果玩家在ReactState时跳到其他平台，就算了)
        // 保险起见，Enter()阶段进行一次UpdateBattleTimer();
        if (enemy.PlayerDetected() == true)
            UpdateBattleTimer();

        if (BattleTimeIsOver())
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        
    }

    private void UpdateBattleTimer() => lastTimeWasInBattle = Time.time;

    private bool BattleTimeIsOver() => Time.time > lastTimeWasInBattle + enemy.battleTimeDuration;

    private bool WithinAttackRange() => DistanceToPlayer() < enemy.attackDistance;

    private void HandleRetreat()
    {
        if (ShouldRetreat())
        {
            rb.velocity = new Vector2(enemy.retreatVelocity.x * -DirectionToPlayer(), enemy.retreatVelocity.y);
            enemy.HandleFlip(DirectionToPlayer());
        }
    }
    private bool ShouldRetreat() => DistanceToPlayer() < enemy.minRetreatDistance;

    private float DistanceToPlayer()
    {
        if (player == null)
            return float.MaxValue;

        return Mathf.Abs(player.position.x - enemy.transform.position.x);
    }

    private int DirectionToPlayer()
    {
        if (player == null)
        {
            Debug.Log("Enter battleState but no player!");
            return 0;
        }

        return player.position.x - enemy.transform.position.x > 0 ? 1 : -1;
    }
}
