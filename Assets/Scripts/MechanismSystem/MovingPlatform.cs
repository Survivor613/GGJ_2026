using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 简化版移动平台：只负责移动，通过物理引擎自动带动乘客
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour, IActivatable
{
    [Header("Movement Parameters")]
    [Tooltip("位移向量（相对于初始位置）")]
    [SerializeField] private Vector3 travelOffset = new Vector3(5f, 0f, 0f);
    
    [Tooltip("移动速度")]
    [SerializeField] private float speed = 2f;
    
    [Tooltip("是否需要持续踩踏（true=放开按钮会回退，false=到达后保持）")]
    [SerializeField] private bool holdToActive = false;
    
    // 位置状态
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 currentTarget;
    
    // 激活状态
    private bool isActivated = false;
    public bool IsActivated => isActivated;
    
    // Rigidbody2D（用于物理移动）
    private Rigidbody2D rb;
    
    // 记录上一帧位置（用于计算速度）
    private Vector3 lastPosition;
    
    // 当前站在平台上的物体
    private List<Rigidbody2D> passengersOnPlatform = new List<Rigidbody2D>();
    
    private void Awake()
    {
        // 获取或添加 Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // 配置 Rigidbody2D 为 Kinematic（不受重力影响，但能推动其他物体）
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        
        // 记录初始位置
        startPosition = transform.position;
        targetPosition = startPosition + travelOffset;
        currentTarget = startPosition;
        lastPosition = transform.position;
    }
    
    private void FixedUpdate()
    {
        // 计算本帧移动增量
        Vector3 deltaMovement = transform.position - lastPosition;
        
        // 更新目标位置
        UpdateTarget();
        
        // 使用 Rigidbody2D 移动（自动带动乘客）
        MoveToTarget();
        
        // 如果平台在横向移动，给站在上面的物体相同的横向速度
        if (Mathf.Abs(deltaMovement.x) > 0.001f)
        {
            MovePlatformPassengers(deltaMovement);
        }
        
        // 更新上一帧位置
        lastPosition = transform.position;
    }
    
    /// <summary>
    /// 更新目标位置
    /// </summary>
    private void UpdateTarget()
    {
        if (holdToActive)
        {
            currentTarget = isActivated ? targetPosition : startPosition;
        }
        else
        {
            if (isActivated)
            {
                currentTarget = targetPosition;
            }
        }
    }
    
    /// <summary>
    /// 使用 Rigidbody2D.MovePosition 移动（物理引擎会自动推动站在上面的物体）
    /// </summary>
    private void MoveToTarget()
    {
        Vector3 newPosition = Vector3.MoveTowards(
            rb.position,
            currentTarget,
            speed * Time.fixedDeltaTime
        );
        
        // 使用 Rigidbody2D.MovePosition（会正确处理与其他物体的交互）
        rb.MovePosition(newPosition);
    }
    
    /// <summary>
    /// 横向移动时，给站在平台上的物体相同的横向位移
    /// </summary>
    private void MovePlatformPassengers(Vector3 deltaMovement)
    {
        // 清理列表（移除已销毁的对象）
        passengersOnPlatform.RemoveAll(p => p == null);
        
        foreach (var passenger in passengersOnPlatform)
        {
            if (passenger != null && passenger.bodyType == RigidbodyType2D.Dynamic)
            {
                // 只应用横向位移（不影响纵向，让 Player 能正常跳跃）
                passenger.position += new Vector2(deltaMovement.x, 0);
            }
        }
    }
    
    /// <summary>
    /// 当物体开始接触平台时
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D otherRb = collision.rigidbody;
        if (otherRb != null && !passengersOnPlatform.Contains(otherRb))
        {
            passengersOnPlatform.Add(otherRb);
        }
    }
    
    /// <summary>
    /// 当物体离开平台时
    /// </summary>
    private void OnCollisionExit2D(Collision2D collision)
    {
        Rigidbody2D otherRb = collision.rigidbody;
        if (otherRb != null)
        {
            passengersOnPlatform.Remove(otherRb);
        }
    }
    
    // ========== IActivatable 接口实现 ==========
    
    public void Activate()
    {
        isActivated = true;
    }
    
    public void Deactivate()
    {
        if (!holdToActive) return;
        isActivated = false;
    }
    
    // ========== 编辑器可视化 ==========
    
    private void OnDrawGizmosSelected()
    {
        Vector3 start = Application.isPlaying ? startPosition : transform.position;
        Vector3 target = start + travelOffset;
        
        // 绘制起始位置
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(start, Vector3.one);
        
        // 绘制目标位置
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(target, Vector3.one);
        
        // 绘制路径
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, target);
    }
}
