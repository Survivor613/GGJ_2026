using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_DashState : PlayerState
{
    private float originalGravityScale;
    private int dashDir;

    public Player_DashState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 不要用 ((int)player.moveInput.x)，当按住 “右+下” 方向键并按 Dash 时，由于速度归一化，x方向速度会从 1 变成 0.7
        // 此时 ((int)player.moveInput.x) 会直接抹除小数点（+0.7/-0.7 => 0）
        // 改用 ((int)Mathf.Sign(player.moveInput.x))，保留符号
        dashDir = player.moveInput.x != 0 ? ((int)Mathf.Sign(player.moveInput.x)) : player.facingDir;
        stateTimer = player.dashDuration;

        originalGravityScale = rb.gravityScale;
        rb.gravityScale = 0;

        //player.sfx.EntityDash(); // Dash SFX
        player.SetDash(); // clear Dash
    }

    public override void Update()
    {
        base.Update();
        CancelDashIfNeeded();
        player.SetVelocity(player.dashSpeed * dashDir, 0);

        if (stateTimer < 0)
        {
            if (player.groundDetected)
                stateMachine.ChangeState(player.idleState);
            else
                stateMachine.ChangeState(player.fallState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.SetVelocity(0, 0);
        rb.gravityScale = originalGravityScale;
    }

    private void CancelDashIfNeeded()
    {
        if (player.wallDetected)
        {
            if (player.groundDetected)
                stateMachine.ChangeState(player.idleState);
            else
                stateMachine.ChangeState(player.wallSlideState);
        }
    }
}
