using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_AiredState : PlayerState
{
    private bool isPressingWall; // 起跳时是否紧贴墙

    public override void Enter()
    {
        base.Enter();
        isPressingWall = player.wallDetected;
    }

    public Player_AiredState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();

        if (player.moveInput.x != 0)
            player.SetVelocity(player.moveInput.x * (player.moveSpeed * player.inAirMoveMultiplier), rb.velocity.y);

        if (input.Player.Jump.WasPressedThisFrame())
            player.SetJumpBuffer();  //提供跳跃缓冲

        //if (player.wallDetected && player.CanApplyLedgeCorrection())
        //    ApplyLedgeCorrection();

        if (isPressingWall = false && player.JumpingAgainstWall())
        // 进入wallSlideState逻辑：fallState && wallDetected (见Player_FallState) 或 AiredState && 起跳时不紧贴墙但现在对墙壁有挤压
        {
            stateMachine.ChangeState(player.wallSlideState);
            return;
        }

        if (input.Player.Attack.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.jumpAttackState);
            return;
        }
    }

    //public void ApplyLedgeCorrection()
    //{
    //        player.transform.position += player.ledgeCorrection;
    //}
}
