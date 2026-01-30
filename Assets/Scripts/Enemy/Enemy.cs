using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity, ICounterable
{
    public Enemy_IdleState idleState;
    public Enemy_MoveState moveState;
    public Enemy_AttackState attackState;
    public Enemy_BattleState battleState;
    public Enemy_DeadState deadState;

    public float reactDuration = .4f;

    [Header("Battle details")]
    public float battleMoveSpeed = 1.5f;
    public float attackDistance = 2;
    public float battleTimeDuration = 5;
    public float minRetreatDistance = 1;
    public Vector2 retreatVelocity = new Vector2(2.5f, 1.5f);

    [Header("Stunned state details")]
    public float stunnedDuration = 1;
    public Vector2 stunnedVelocity = new Vector2(7, 7);
    [SerializeField] protected bool canBeStunned;
    public bool CanBeCountered { get => canBeStunned; }

    [Header("Movement details")]
    public float idleTime = 2;
    [SerializeField] public float moveSpeed = 0.7f;
    [Range(0, 2)]
    public float moveAnimSpeedMultiplier = 1;

    [Header("Player detection")]
    [SerializeField] private LayerMask whatIsPlayer;
    [SerializeField] private Transform playerCheck;
    [SerializeField] private float playerCheckDistance = 10;
    public Transform player { get; private set; }

    public void EnableCounterWindow(bool enable) => canBeStunned = enable;

    public override void EntityDeath()
    {
        base.EntityDeath();

        sfx.EntityDeath(); // Enemy Death SFX

        stateMachine.ChangeState(deadState);
        return;
    }

    private void HandlePlayerDeath()
    {
        stateMachine.ChangeState(idleState);
    }

    public void TryEnterBattleState(Transform player)
    {
        if (stateMachine.currentState == battleState || stateMachine.currentState == attackState)
            return;

        SetPlayer(player);
        stateMachine.ChangeState(battleState);
    }

    public Transform GetPlayerReference()
    {
        if (player == null)
            player = PlayerDetected().transform;

        return player;
    }

    public void SetPlayer(Transform player) => this.player = player;

    public RaycastHit2D PlayerDetected()   // PlayerDectected 返回 RaycastHit2D, 类似结构体, 其中 transform 对象保存了对 player.Transform 的引用
    {
        RaycastHit2D hit = Physics2D.Raycast(playerCheck.position, Vector2.right * facingDir, playerCheckDistance, whatIsPlayer | whatIsGround);

        if (hit.collider == null || hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
            return default;

        return hit;
    }

    public void HandleCounter(float counterAttackDamage, Transform damageDealer)
    {
        health.TakeDamage(counterAttackDamage, damageDealer);

        
        sfx?.EntityAttackHit();
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x + (facingDir * playerCheckDistance), playerCheck.position.y));
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x + (facingDir * attackDistance), playerCheck.position.y));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x + (facingDir * minRetreatDistance), playerCheck.position.y));
    }

    private void OnEnable()
    {
        Player.onPlayerDeath += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        Player.onPlayerDeath -= HandlePlayerDeath;
    }
}
