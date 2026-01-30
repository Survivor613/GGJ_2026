using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_WallJumpState : PlayerState
{
    private float wallJumpInputLockTimer;

    public Player_WallJumpState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        wallJumpInputLockTimer = player.wallJumpInputLockDuration;

        // 蹬墙跳 
        if (player.wallDetected)
            player.SetVelocity(player.wallJumpForce.x * -player.facingDir, player.wallJumpForce.y);
        else
            // wallSlide 切换到 fallState 后短时间按跳跃键可进入 wallJump，提供一定缓冲区
            // x 不用乘 -1，很重要
            player.SetVelocity(player.wallJumpForce.x * player.facingDir, player.wallJumpForce.y);

        player.ClearJumpCoyote();
        player.ClearJumpBuffer();

        player.sfx.EntityJump(); // Jump SFX
    }

    public override void Update()
    {
        base.Update();
        // 实现xInput的输入，实现爬墙小跳, 致敬丝之歌
        if (WallJumpInputEnable() && player.moveInput.x != 0)
            player.SetVelocity(player.moveInput.x * (player.moveSpeed * player.inAirMoveMultiplier), rb.velocity.y);

        if (input.Player.Jump.WasPressedThisFrame())
            player.SetJumpBuffer();  //提供跳跃缓冲,优化跳跃手感

        if (input.Player.Attack.WasPressedThisFrame())
            stateMachine.ChangeState(player.jumpAttackState);

        if (rb.velocity.y < 0 && stateMachine.currentState != player.jumpAttackState)
            stateMachine.ChangeState(player.fallState);

        if (player.JumpingAgainstWall())
            stateMachine.ChangeState(player.wallSlideState);
    }

    private bool WallJumpInputEnable()
    {
        wallJumpInputLockTimer -= Time.deltaTime;
        if (wallJumpInputLockTimer < 0)
            return true;
        else
            return false;
    }
}
