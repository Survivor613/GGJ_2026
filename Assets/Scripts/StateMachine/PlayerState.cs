 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState : EntityState
{
    protected Player player;
    protected PlayerInputSet input;

    public PlayerState(Player player, StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
    {
        this.player = player;

        anim = player.anim;
        rb = player.rb;
        input = player.input;
    }

    public override void Update()
    {
        base.Update();

        if (input.Player.Dash.WasPressedThisFrame() && CanDash())
            stateMachine.ChangeState(player.dashState);
    }

    public override void updateAnimationParameters()
    {
        base.updateAnimationParameters();

        // 使用安全方法设置，避免狐狸动画控制器缺少参数时报错
        SafeSetFloat("yVelocity", rb.velocity.y);
    }

    private bool CanDash()
    {
        if (player.wallDetected)
            return false;

        if (stateMachine.currentState == player.dashState)
            return false;

        if (player.HasDash == false)
            return false;

        // canDash
        return true;
    }
}
