using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_JumpState : Player_AiredState
{
    public Player_JumpState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.SetVelocity(rb.velocity.x, player.jumpForce);

        player.ClearJumpCoyote();
        player.ClearJumpBuffer();

        //player.sfx.EntityJump(); // Jump SFX
    }

    public override void Update()
    {
        base.Update();

        if (input.Player.Jump.WasCompletedThisFrame())
            player.SetVelocity(rb.velocity.x, 0.5f * rb.velocity.y); // 长按跳得更高，轻按跳得更低
        
        if (rb.velocity.y < 0 && stateMachine.currentState != player.jumpAttackState)
            stateMachine.ChangeState(player.fallState);
    }
}
