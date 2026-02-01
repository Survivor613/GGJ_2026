using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 矛陷阱：位于地面或天花板，可以戳刺攻击
/// 动画通过Animator控制，攻击期间通过AnimationEvent调用造成伤害
/// </summary>
public class SpikeTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    [Tooltip("陷阱位置：Ground=地面，Ceiling=天花板")]
    [SerializeField] private TrapPosition trapPosition = TrapPosition.Ground;
    
    [Tooltip("自动攻击：如果为true，会按间隔自动攻击")]
    [SerializeField] private bool autoAttack = true;
    
    [Tooltip("攻击间隔（秒）")]
    [SerializeField] private float attackInterval = 2f;
    
    [Tooltip("初始延迟（秒）")]
    [SerializeField] private float initialDelay = 0f;

    [Header("Trigger Settings")]
    [Tooltip("触发器检测：如果启用，玩家进入触发区域才会攻击")]
    [SerializeField] private bool useTriggerDetection = false;
    
    [Tooltip("触发器检测范围")]
    [SerializeField] private Vector2 triggerSize = new Vector2(2f, 2f);
    
    [Tooltip("触发器偏移")]
    [SerializeField] private Vector2 triggerOffset = new Vector2(0f, 1f);
    
    [Tooltip("玩家所在的Layer")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Damage Settings")]
    [Tooltip("造成的伤害值")]
    [SerializeField] private float damage = 20f;
    
    [Tooltip("攻击检测位置（相对于陷阱）")]
    [SerializeField] private Transform attackPoint;
    
    [Tooltip("攻击检测范围")]
    [SerializeField] private Vector2 attackRange = new Vector2(1f, 2f);

    // 私有变量
    private Animator animator;
    private float attackTimer = 0f;
    private bool playerInRange = false;
    private bool isAttacking = false;
    
    // Animator参数名
    private readonly string ATTACK_TRIGGER = "Attack";
    private readonly string IS_ATTACKING = "IsAttacking";

    public enum TrapPosition
    {
        Ground,
        Ceiling
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogWarning($"SpikeTrap: {gameObject.name} 缺少 Animator 组件！");
        }

        // 如果没有设置攻击点，使用自身位置
        if (attackPoint == null)
            attackPoint = transform;
        
        attackTimer = initialDelay;
    }

    private void Update()
    {
        if (autoAttack)
        {
            HandleAutoAttack();
        }
        else if (useTriggerDetection)
        {
            HandleTriggerAttack();
        }
    }

    /// <summary>
    /// 处理自动攻击逻辑
    /// </summary>
    private void HandleAutoAttack()
    {
        if (isAttacking)
            return;

        attackTimer += Time.deltaTime;
        
        if (attackTimer >= attackInterval)
        {
            TriggerAttack();
            attackTimer = 0f;
        }
    }

    /// <summary>
    /// 处理触发器攻击逻辑
    /// </summary>
    private void HandleTriggerAttack()
    {
        if (isAttacking)
            return;

        // 检测玩家是否在触发范围内
        Vector2 triggerCenter = (Vector2)transform.position + triggerOffset;
        Collider2D[] hits = Physics2D.OverlapBoxAll(triggerCenter, triggerSize, 0f, playerLayer);
        
        playerInRange = hits.Length > 0;

        if (playerInRange)
        {
            attackTimer += Time.deltaTime;
            
            if (attackTimer >= attackInterval)
            {
                TriggerAttack();
                attackTimer = 0f;
            }
        }
        else
        {
            attackTimer = 0f; // 玩家离开时重置计时器
        }
    }

    /// <summary>
    /// 触发攻击动画
    /// </summary>
    private void TriggerAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger(ATTACK_TRIGGER);
            isAttacking = true;
        }
    }

    /// <summary>
    /// 由动画事件调用：在攻击的伤害帧造成伤害
    /// 在Animator的攻击动画中，在戳出的关键帧添加AnimationEvent，调用此方法
    /// </summary>
    public void DealDamage()
    {
        Vector2 attackCenter = (Vector2)attackPoint.position;
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackCenter, attackRange, 0f, playerLayer);

        foreach (Collider2D hit in hits)
        {
            IDamagable damagable = hit.GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.TakeDamage(damage, transform);
            }
        }
    }

    /// <summary>
    /// 由动画事件调用：攻击动画结束时调用
    /// 在Animator的攻击动画结束时添加AnimationEvent，调用此方法
    /// </summary>
    public void AttackFinished()
    {
        isAttacking = false;
    }

    /// <summary>
    /// 手动触发攻击（供外部调用，如TriggerButton）
    /// </summary>
    public void ManualTriggerAttack()
    {
        if (!isAttacking)
        {
            TriggerAttack();
        }
    }

    /// <summary>
    /// 编辑器可视化
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 绘制触发范围
        if (useTriggerDetection)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Vector2 triggerCenter = (Vector2)transform.position + triggerOffset;
            Gizmos.DrawWireCube(triggerCenter, triggerSize);
        }

        // 绘制攻击范围
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Vector2 attackCenter = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        Gizmos.DrawWireCube(attackCenter, attackRange);
        
        // 绘制攻击方向指示
        Gizmos.color = Color.red;
        Vector3 direction = trapPosition == TrapPosition.Ground ? Vector3.up : Vector3.down;
        Gizmos.DrawLine(transform.position, transform.position + direction * 0.5f);
    }
}
