using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public event Action OnFlipped;

    public Animator anim { get; private set; }
    public Rigidbody2D rb;
    public Entity_SFX sfx { get; private set; }
    public Entity_Health health;
    protected StateMachine stateMachine;

    private bool facingRight = true;
    public int facingDir { get; private set; } = 1;

    [Header("Collision detection")]
    [SerializeField] protected LayerMask whatIsGround;
    [SerializeField] protected LayerMask whatIsWall;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    //[SerializeField] public float wallCheckDistance { get; private set; } = .4f;
    [SerializeField] public Transform primaryWallCheck;
    [SerializeField] public Transform secondaryWallCheck;
    [SerializeField] public Transform primaryGroundCheck;
    [SerializeField] public Transform secondaryGroundCheck;
    public bool groundDetected { get; private set; }
    public bool wallDetected { get; private set; }

    private Coroutine knockbackCo;
    private bool isKnocked;

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sfx = GetComponent<Entity_SFX>();
        health = GetComponent<Entity_Health>();

        stateMachine = new StateMachine();
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        HandleCollisionDetection();
        stateMachine.UpdateActiveState();
        // run update without monobehaviour on entity state
    }

    public void CurrentStateAnimationTrigger()
    {
        stateMachine.currentState.AnimationTrigger();
    }

    public void Transport(Vector3 position) => this.transform.position = position;

    public virtual void EntityDeath()
    {

    }

    public void ReceiveKnockback(Vector2 knockback, float duration)
    {
        if (knockbackCo != null)
              StopCoroutine(knockbackCo);

        if (duration < 0) // Signifying that entity is knocked back by harzard ground
            KnockbackByHarzardGround(knockback);
        else
            knockbackCo = StartCoroutine(KnockbackCo(knockback, duration));
    }

    private void KnockbackByHarzardGround(Vector2 knockback)
    {
        rb.velocity = knockback;
    }

    private IEnumerator KnockbackCo(Vector2 knockback, float duration)
    {
        isKnocked = true;
        rb.velocity = knockback;

        yield return new WaitForSeconds(duration);

        rb.velocity = Vector2.zero;
        isKnocked = false;
    }

    public void SetVelocity(float xVelocity, float yVelocity)
    {
        if (isKnocked)
            return;

        rb.velocity = new Vector2(xVelocity, yVelocity);
        HandleFlip(xVelocity);
    }

    public void HandleFlip(float xVelocity)
    {
        if (xVelocity > 0 && !facingRight)
            Flip();
        else if (xVelocity < 0 && facingRight)
            Flip();
    }

    public void Flip()
    {
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
        facingDir *= -1;

        OnFlipped?.Invoke(); // ·ÀÖ¹ UI_MiniHealthBar ·­×ª
    }

    private void HandleCollisionDetection()
    {
        if (secondaryGroundCheck != null)
            groundDetected = Physics2D.Raycast(primaryGroundCheck.position, Vector2.down, groundCheckDistance, whatIsGround)
                || Physics2D.Raycast(secondaryGroundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        else
            groundDetected = Physics2D.Raycast(primaryGroundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);

        if (secondaryWallCheck != null)
            wallDetected = Physics2D.Raycast(primaryWallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround)
                && Physics2D.Raycast(secondaryWallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsWall);
        else
            wallDetected = Physics2D.Raycast(primaryWallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsWall);
    }

    public bool JumpingAgainstWall() => wallDetected && ((facingDir == -1 && rb.velocity.x < 0) || (facingDir == 1 && rb.velocity.x > 0));

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(primaryGroundCheck.position, primaryGroundCheck.position + new Vector3(0, -groundCheckDistance));
        if (secondaryGroundCheck != null)
            Gizmos.DrawLine(secondaryGroundCheck.position, secondaryGroundCheck.position + new Vector3(0, -groundCheckDistance));
        Gizmos.DrawLine(primaryWallCheck.position, primaryWallCheck.position + new Vector3(wallCheckDistance * facingDir, 0));
        if (secondaryWallCheck != null)
            Gizmos.DrawLine(secondaryWallCheck.position, secondaryWallCheck.position + new Vector3(wallCheckDistance * facingDir, 0));
    }
}
