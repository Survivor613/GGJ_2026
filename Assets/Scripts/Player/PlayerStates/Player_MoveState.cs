using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_MoveState : Player_GroundedState
{
    public Player_MoveState(Player player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
    {
    }

    public override void Update()
    {
        base.Update();
        // 检查状态是否已经不再是自己，如果是，立即跳出，不执行后续物理逻辑
        // 非常重要，防止CounterAttackState中，即使Enter()中设置了x方向速度清零，但仍然在这里修改x方向速度
        if (stateMachine.currentState != this)
            return;

        player.SetVelocity(player.moveInput.x * player.moveSpeed, rb.velocity.y); // 先跑起来再检测是否对着墙跑，防止面壁思过(对墙抽搐)

        if (player.moveInput.x == 0 || player.wallDetected)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

        //player.sfx.EntityMove(); // Player Move SFX
    }
}
