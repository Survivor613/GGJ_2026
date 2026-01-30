using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_FallState : Player_AiredState
{
    float heavyFallLimit = 9;
    bool isHeavyFall;

    public Player_FallState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        anim.SetBool("jumpFall", true);
        isHeavyFall = false;
    }

    public override void Update()
    {
        base.Update();

        if (rb.velocity.y < -heavyFallLimit && isHeavyFall == false)
            isHeavyFall = true;

        if (input.Player.Jump.WasPressedThisFrame() && player.HasJumpCoyote)  // 提供土狼时间,优化跳跃手感
        {
            stateMachine.ChangeState(player.jumpState);
            return;
        }

        Debug.Log(player.groundDetected);

        if (player.groundDetected)
        {
            stateMachine.ChangeState(player.idleState);
            //if (isHeavyFall)
                //player.sfx.EntityJumpLand(); // JumpLand SFX, only when fallState => groundedState && heavy impact on the ground
            return;
        }

        if (player.wallDetected)
        {
            stateMachine.ChangeState(player.wallSlideState); // 进入wallSlideState逻辑：fallState && wallDetected 或 AiredState && 起跳时不紧贴墙但现在对墙壁有挤压 (见Player_AiredState)
            return;
        }
    }
}
