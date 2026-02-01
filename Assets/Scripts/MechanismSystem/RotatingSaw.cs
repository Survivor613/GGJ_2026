using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 旋转锯齿陷阱：左右或上下来回运动，边缘短暂停留，速度过渡流畅
/// 玩家碰到会受到伤害
/// </summary>
public class RotatingSaw : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("运动方向：Horizontal=左右，Vertical=上下")]
    [SerializeField] private MovementDirection direction = MovementDirection.Horizontal;
    
    [Tooltip("移动距离（单位：米）")]
    [SerializeField] private float travelDistance = 5f;
    
    [Tooltip("单程移动时间（秒）")]
    [SerializeField] private float moveDuration = 2f;
    
    [Tooltip("到达边缘时的停留时间")]
    [SerializeField] private float pauseDuration = 0.5f;
    
    [Tooltip("速度曲线：控制加速和减速效果（0-1为时间进度，值为速度倍率）")]
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Damage Settings")]
    [Tooltip("造成的伤害值")]
    [SerializeField] private float damage = 15f;
    
    [Tooltip("伤害检测半径")]
    [SerializeField] private float damageRadius = 0.5f;
    
    [Tooltip("玩家所在的Layer")]
    [SerializeField] private LayerMask playerLayer;
    
    [Tooltip("伤害间隔（防止连续造成伤害）")]
    [SerializeField] private float damageInterval = 0.5f;

    // 私有变量
    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool isMovingToEnd = true;
    private bool isPaused = false;
    private float pauseTimer = 0f;
    private float moveTimer = 0f;
    
    // 伤害冷却
    private Dictionary<Collider2D, float> lastDamageTime = new Dictionary<Collider2D, float>();

    public enum MovementDirection
    {
        Horizontal,
        Vertical
    }

    private void Start()
    {
        // 记录起始位置和结束位置
        startPosition = transform.position;
        
        if (direction == MovementDirection.Horizontal)
            endPosition = startPosition + Vector3.right * travelDistance;
        else
            endPosition = startPosition + Vector3.up * travelDistance;
    }

    private void Update()
    {
        HandleMovement();
        CheckForDamage();
    }

    /// <summary>
    /// 处理平滑移动逻辑（使用加速-减速曲线）
    /// </summary>
    private void HandleMovement()
    {
        // 如果正在暂停
        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isPaused = false;
                moveTimer = 0f;
                // 切换方向
                isMovingToEnd = !isMovingToEnd;
            }
            return;
        }

        // 累积移动时间
        moveTimer += Time.deltaTime;
        
        // 计算移动进度（0到1）
        float progress = Mathf.Clamp01(moveTimer / moveDuration);
        
        // 使用AnimationCurve评估当前进度对应的插值值
        float curveValue = speedCurve.Evaluate(progress);
        
        // 根据方向和曲线值计算位置
        Vector3 targetPosition = isMovingToEnd ? endPosition : startPosition;
        Vector3 sourcePosition = isMovingToEnd ? startPosition : endPosition;
        transform.position = Vector3.Lerp(sourcePosition, targetPosition, curveValue);

        // 检查是否完成移动
        if (progress >= 1f)
        {
            transform.position = targetPosition;
            isPaused = true;
            pauseTimer = pauseDuration;
            moveTimer = 0f;
        }
    }

    /// <summary>
    /// 检测范围内的玩家并造成伤害
    /// </summary>
    private void CheckForDamage()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, damageRadius, playerLayer);
        
        foreach (Collider2D collider in hitColliders)
        {
            // 检查伤害冷却
            if (lastDamageTime.ContainsKey(collider))
            {
                if (Time.time - lastDamageTime[collider] < damageInterval)
                    continue;
            }

            // 尝试对目标造成伤害
            IDamagable damagable = collider.GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.TakeDamage(damage, transform);
                lastDamageTime[collider] = Time.time;
            }
        }

        // 清理已销毁的对象
        List<Collider2D> toRemove = new List<Collider2D>();
        foreach (var key in lastDamageTime.Keys)
        {
            if (key == null)
                toRemove.Add(key);
        }
        foreach (var key in toRemove)
        {
            lastDamageTime.Remove(key);
        }
    }

    /// <summary>
    /// 编辑器可视化
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Vector3 start = Application.isPlaying ? startPosition : transform.position;
        Vector3 end;
        
        if (direction == MovementDirection.Horizontal)
            end = start + Vector3.right * travelDistance;
        else
            end = start + Vector3.up * travelDistance;

        // 绘制起始位置
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(start, 0.3f);
        
        // 绘制结束位置
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(end, 0.3f);
        
        // 绘制路径
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, end);
        
        // 绘制伤害范围
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
