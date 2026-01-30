using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_DeadState : PlayerState
{
    public Player_DeadState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        input.Disable();
        rb.simulated = false;
    }

    public override void Update()
    {
        if (triggerCalled)
        {
            player.Respawn(); // etc. transport to respawn point && recover full HP

            stateMachine.ChangeState(player.idleState);
            return;
        }
    }

    public override void Exit()
    {
        base.Exit();

        input.Enable();
        rb.simulated = true;
    }
}
