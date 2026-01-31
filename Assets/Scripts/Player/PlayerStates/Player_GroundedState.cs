using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player_GroundedState : PlayerState
{
    public Player_GroundedState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (player.HasJumpBuffer)  //提供跳跃缓冲,优化跳跃手感
        {
            stateMachine.ChangeState(player.jumpState);
            return;
        }
    }

    public override void Update()
    {
        base.Update();

        player.SetJumpCoyote();

        if (rb.velocity.y < 0 && player.groundDetected == false)
        {
            stateMachine.ChangeState(player.fallState);
            return;
        }

        if (input.Player.Jump.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.jumpState);
            return;
        }

        if (input.Player.Attack.WasPressedThisFrame())
        {
            if (player.canAttack == true)
            {
                stateMachine.ChangeState(player.basicAttackState);
                return;

            }
        }
    }
}
