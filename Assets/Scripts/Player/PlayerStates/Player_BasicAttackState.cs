using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_BasicAttackState : PlayerState
{
    private float  attackVelocityTimer;
    private float lastTimeAttacked;

    private bool comboAttackQueue;
    private int attackDir;
    private int comboIndex = FirstComboIndex;
    private int comboLimit = 3;
    private const int FirstComboIndex = 1; // Used as initial basicAttackIndex in Animator


    public Player_BasicAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
        if (comboLimit != player.attackVelocity.Length)
            Debug.LogWarning("Adjusted combo limit to match attack velocity array!");
            comboLimit = player.attackVelocity.Length;
    }

    public override void Enter()
    {
        base.Enter();
        comboAttackQueue = false;
        ResetComboIndexIfNeeded();

        // Define attack direction according to input  
        attackDir = player.moveInput.x != 0 ? ((int)player.moveInput.x) : player.facingDir;
        
        SafeSetInteger("basicAttackIndex", comboIndex);
        ApplyAttackVelocity();
    }

    public override void Update()
    {
        base.Update();
        HandleAttackVelocity();

        if (input.Player.Attack.WasPressedThisFrame())
            QueueNextAttack();

        if (triggerCalled)
            HandleStateExit();
    }

    public override void Exit()
    {
        base.Exit();
        comboIndex++;
        lastTimeAttacked = Time.time;
    }

    private void HandleStateExit()
    {
        if (comboAttackQueue)
        {
            SafeSetBool(animBoolName, false);   // make animator boolean false on the current frame and then true on the next frame
            player.EnterAttackStateWithDelay();
        }
        else stateMachine.ChangeState(player.idleState);
    }

    private void QueueNextAttack()
    {
        if (comboIndex < comboLimit)
            comboAttackQueue = true;
    }

    private void HandleAttackVelocity()
    {
        attackVelocityTimer -= Time.deltaTime;

        if (attackVelocityTimer < 0)
            player.SetVelocity(0, rb.velocity.y);
    }

    private void ApplyAttackVelocity()
    {
        Vector2 attackVelocity = player.attackVelocity[comboIndex-1];

        attackVelocityTimer = player.attackVeclocityDuration;
        player.SetVelocity(attackVelocity.x * attackDir, attackVelocity.y);
    }

    private void ResetComboIndexIfNeeded()
    {
        if (Time.time > lastTimeAttacked + player.comboResetTime)
            comboIndex = FirstComboIndex;

        if (comboIndex > comboLimit)
            comboIndex = FirstComboIndex;
    }
}
