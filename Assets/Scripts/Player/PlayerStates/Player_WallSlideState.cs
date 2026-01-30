using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_WallSlideState : PlayerState
{
    public Player_WallSlideState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (player.HasJumpBuffer)  //提供跳跃缓冲,优化跳跃手感
        {
            stateMachine.ChangeState(player.wallJumpState);
            return;
        }
    }

    public override void Update()
    {
        base.Update();  
        HandleWallSlide();

        player.SetJumpCoyote(); // 提供野狼时间,优化跳跃手感

        if (input.Player.Jump.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.wallJumpState);
            return;
        }

        if (player.wallDetected == false)
        {
            stateMachine.ChangeState(player.fallState);
            return;
        }

        if (player.groundDetected)
        {
            stateMachine.ChangeState(player.idleState);
            if (player.facingDir != player.moveInput.x)
                player.Flip();
            return;
        }
    }

    private void HandleWallSlide()
    {   
        
        if (player.moveInput.y < 0)
            player.SetVelocity(player.moveInput.x, rb.velocity.y);
        else
            player.SetVelocity(player.moveInput.x, rb.velocity.y * player.wallSlideSlowMultiplier);
    }
}