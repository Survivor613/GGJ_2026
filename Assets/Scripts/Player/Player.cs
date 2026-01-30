using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Player : Entity
{
    public static event Action onPlayerDeath;
    public PlayerInputSet input { get; private set; }

    public Player_IdleState idleState { get; private set; }
    public Player_MoveState moveState { get; private set; }
    public Player_JumpState jumpState { get; private set; }
    public Player_FallState fallState { get; private set; }
    public Player_WallSlideState wallSlideState { get; private set; }
    public Player_WallJumpState wallJumpState { get; private set; }
    public Player_DashState dashState { get; private set; }
    public Player_BasicAttackState basicAttackState { get; private set; }
    public Player_JumpAttackState jumpAttackState { get; private set; }
    public Player_DeadState deadState { get; private set; }


    [Header("Attack details")]
    public Vector2[] attackVelocity = new Vector2[]
    {
        new Vector2(2, 1),
        new Vector2(5, 2),
        new Vector2(5, 5)
    };
    public Vector2 jumpAttackVelocity;
    public float attackVeclocityDuration = .1f;
    public float comboResetTime = 1;
    private Coroutine queuedAttackCo;  // Coroutine needs MonoBehaviour

    [Header("Movement details")]
    public float moveSpeed = 4;
    public float jumpForce = 8;
    public Vector2 wallJumpForce = new Vector2(4, 8);
    [SerializeField] public float wallJumpInputLockDuration = .12f; // 新增: 配合爬墙小跳使用

    [Header("Jump Improvements")]
    public float jumpCoyoteDuration = .2f; // 土狼时间 (见 player.fallState)
    public bool HasJumpCoyote { get; private set; }
    private Coroutine queuedJumpCoyoteCo;

    public float jumpBufferDuration = .2f;
    public bool HasJumpBuffer { get; private set; }
    private Coroutine queuedJumpBufferCo;

    //[SerializeField] public Vector3 ledgeCorrection = new Vector3(.2f, .2f, 0);

    [Range(0, 1)]
    public float inAirMoveMultiplier = .8f;
    [Range(0, 1)]
    public float wallSlideSlowMultiplier = .7f;

    [Header("Dash details")]
    [SerializeField] public float dashDuration = .25f;
    [SerializeField] public float dashSpeed = 6;

    [SerializeField] public float dashCoolDown = 1;
    public bool HasDash { get; private set; } = true;
    private Coroutine queuedDashCo;



    public Vector2 moveInput { get; private set; }

    public UI ui { get; private set; }

    [Header("Respawn details")]
    [SerializeField] public Vector3 respawnLoc;

    [Header("Skills Able")]
    [SerializeField] public bool canAttack;



    protected override void Awake()
    {
        base.Awake();

        ui = FindAnyObjectByType<UI>();

        input = new PlayerInputSet();
        ui.SetupControlsUI(input);

        idleState = new Player_IdleState(this, stateMachine, "idle");
        moveState = new Player_MoveState(this, stateMachine, "move");
        jumpState = new Player_JumpState(this, stateMachine, "jumpFall");
        fallState = new Player_FallState(this, stateMachine, "jumpFall");
        wallSlideState = new Player_WallSlideState(this, stateMachine, "wallSlide");
        wallJumpState = new Player_WallJumpState(this, stateMachine, "jumpFall");
        dashState = new Player_DashState(this, stateMachine, "dash");
        basicAttackState = new Player_BasicAttackState(this, stateMachine, "basicAttack");
        jumpAttackState = new Player_JumpAttackState(this, stateMachine, "jumpAttack");
        deadState = new Player_DeadState(this, stateMachine, "dead");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    public void Respawn()
    {
        Transport(respawnLoc);

        health.ResetHp();
    }

    public override void EntityDeath()
    {
        base.EntityDeath();

        sfx.EntityDeath(); // Player Death SFX

        onPlayerDeath?.Invoke(); // Enemy.cs 中，将敌人切回 idleState
        stateMachine.ChangeState(deadState);
        return;
    }

    private IEnumerator SetJumpCoyoteCo()
    {
        HasJumpCoyote = true;

        yield return new WaitForSeconds(jumpCoyoteDuration);
        HasJumpCoyote = false;
    }

    public void SetJumpCoyote()
    {
        if (queuedJumpCoyoteCo != null)
            StopCoroutine(queuedJumpCoyoteCo);

        queuedJumpCoyoteCo = StartCoroutine(SetJumpCoyoteCo()); // 一定要赋值给 queuedJumpCoyoteCo! 否则会千百个协程同时修改 HasJumpCoyote
    }

    public void ClearJumpCoyote() => HasJumpCoyote = false;

    private IEnumerator SetJumpBufferCo()
    {
        HasJumpBuffer = true;

        yield return new WaitForSeconds(jumpBufferDuration);
        HasJumpBuffer = false;
    }

    public void SetJumpBuffer()
    {
        if (queuedJumpBufferCo != null)
            StopCoroutine(queuedJumpBufferCo);

        queuedJumpBufferCo = StartCoroutine(SetJumpBufferCo());
    }

    public void ClearJumpBuffer() => HasJumpBuffer = false;

    //public bool CanApplyLedgeCorrection()
    //{
    //    return (secondaryWallCheck != null && Physics2D.Raycast(secondaryWallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround) == true);
    //}

    private IEnumerator SetDashCo()
    {
        HasDash = false;

        yield return new WaitForSeconds(dashCoolDown);
        HasDash = true;
        queuedDashCo = null; // 很关键！否则之后进入 dashState 后，SetDash() 都无法进行正常运行
    }

    public void SetDash()
    {
        if (queuedDashCo == null)
            queuedDashCo = StartCoroutine(SetDashCo());
    }

    public void ClearDash() => HasDash = false;

    private IEnumerator EnterAttackStateWithDelayCo()
    {
        yield return new WaitForEndOfFrame();
        stateMachine.ChangeState(basicAttackState);
    }

    public void EnterAttackStateWithDelay()
    {
        if (queuedAttackCo != null)
            StopCoroutine(queuedAttackCo);

        queuedAttackCo = StartCoroutine(EnterAttackStateWithDelayCo());
    }

    private void OnEnable()
    {
        input.Enable();

        input.Player.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Movement.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnDisable()
    {
        input.Disable();
    }

    internal void ApplySpeedPenalty()
    {
        moveSpeed *= 0.5f;
    }
}

