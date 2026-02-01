using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 工业级触发按钮：基于触发计数器的交互系统
/// 支持多物体同时触发、视觉反馈、扩展性强
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class TriggerButton : MonoBehaviour
{
    [Header("Target Mechanisms")]
    [Tooltip("要控制的机关列表（一对多支持）")]
    [SerializeField] private List<GameObject> targetMechanisms = new List<GameObject>();
    
    [Header("Trigger Filtering")]
    [Tooltip("检测对象的Layer（Player=7, Box=10）")]
    [SerializeField] private LayerMask triggerLayers = ~0; // 默认检测所有层
    
    [Tooltip("是否启用Tag过滤（勾选后只检测指定Tag）")]
    [SerializeField] private bool useTagFiltering = true;
    
    [Tooltip("允许触发的Tag列表")]
    [SerializeField] private string[] allowedTags = { "Player", "Box" };
    
    [Header("Visual Feedback")]
    [Tooltip("按钮激活时的Sprite（可选）")]
    [SerializeField] private Sprite activatedSprite;
    
    [Tooltip("按钮未激活时的Sprite（可选）")]
    [SerializeField] private Sprite deactivatedSprite;
    
    [Tooltip("按钮激活时的颜色")]
    [SerializeField] private Color activatedColor = Color.green;
    
    [Tooltip("按钮未激活时的颜色")]
    [SerializeField] private Color deactivatedColor = Color.gray;
    
    [Header("Events")]
    [Tooltip("按钮激活时触发的事件")]
    public UnityEvent onActivate;
    
    [Tooltip("按钮停用时触发的事件")]
    public UnityEvent onDeactivate;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    // 触发计数器（核心逻辑）
    private int triggerCount = 0;
    
    // 视觉组件缓存
    private SpriteRenderer spriteRenderer;
    
    // 缓存目标机关的IActivatable接口
    private List<IActivatable> cachedActivatables = new List<IActivatable>();
    
    // 当前激活状态
    public bool IsActivated => triggerCount > 0;
    
    private void Awake()
    {
        // 获取或添加BoxCollider2D
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (!collider.isTrigger)
        {
            Debug.LogWarning($"[TriggerButton] {gameObject.name} 的 BoxCollider2D 未勾选 isTrigger，已自动修正！");
            collider.isTrigger = true;
        }
        
        // 缓存SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 缓存目标机关的IActivatable接口
        CacheActivatables();
        
        // 初始化视觉状态
        UpdateVisuals(false);
    }
    
    private void CacheActivatables()
    {
        cachedActivatables.Clear();
        foreach (var target in targetMechanisms)
        {
            if (target == null) continue;
            
            IActivatable activatable = target.GetComponent<IActivatable>();
            if (activatable != null)
            {
                cachedActivatables.Add(activatable);
            }
            else
            {
                Debug.LogWarning($"[TriggerButton] {target.name} 没有实现 IActivatable 接口！");
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[TriggerButton] {gameObject.name} 已连接 {cachedActivatables.Count} 个机关");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 过滤检测
        if (!IsValidTrigger(other))
            return;
        
        // 计数器递增
        triggerCount++;
        
        if (showDebugInfo)
        {
            Debug.Log($"[TriggerButton] {other.name} 进入触发区 | 计数器: {triggerCount}");
        }
        
        // 从0变为1时激活
        if (triggerCount == 1)
        {
            ActivateButton();
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // 过滤检测
        if (!IsValidTrigger(other))
            return;
        
        // 计数器递减
        triggerCount = Mathf.Max(0, triggerCount - 1);
        
        if (showDebugInfo)
        {
            Debug.Log($"[TriggerButton] {other.name} 离开触发区 | 计数器: {triggerCount}");
        }
        
        // 从1变为0时停用
        if (triggerCount == 0)
        {
            DeactivateButton();
        }
    }
    
    /// <summary>
    /// 验证触发对象是否合法（Layer + Tag 双重过滤）
    /// </summary>
    private bool IsValidTrigger(Collider2D collider)
    {
        // Layer过滤
        if ((triggerLayers.value & (1 << collider.gameObject.layer)) == 0)
            return false;
        
        // Tag过滤
        if (useTagFiltering)
        {
            bool tagValid = false;
            foreach (string tag in allowedTags)
            {
                if (collider.CompareTag(tag))
                {
                    tagValid = true;
                    break;
                }
            }
            if (!tagValid)
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 激活按钮（计数器 0→1）
    /// </summary>
    private void ActivateButton()
    {
        if (showDebugInfo)
        {
            Debug.Log($"[TriggerButton] {gameObject.name} 激活！");
        }
        
        // 更新视觉反馈
        UpdateVisuals(true);
        
        // 触发UnityEvent
        onActivate?.Invoke();
        
        // 激活所有连接的机关
        foreach (var activatable in cachedActivatables)
        {
            activatable.Activate();
        }
    }
    
    /// <summary>
    /// 停用按钮（计数器 1→0）
    /// </summary>
    private void DeactivateButton()
    {
        if (showDebugInfo)
        {
            Debug.Log($"[TriggerButton] {gameObject.name} 停用！");
        }
        
        // 更新视觉反馈
        UpdateVisuals(false);
        
        // 触发UnityEvent
        onDeactivate?.Invoke();
        
        // 停用所有连接的机关
        foreach (var activatable in cachedActivatables)
        {
            activatable.Deactivate();
        }
    }
    
    /// <summary>
    /// 更新视觉反馈
    /// </summary>
    private void UpdateVisuals(bool activated)
    {
        if (spriteRenderer == null) return;
        
        // 切换Sprite
        if (activated && activatedSprite != null)
        {
            spriteRenderer.sprite = activatedSprite;
        }
        else if (!activated && deactivatedSprite != null)
        {
            spriteRenderer.sprite = deactivatedSprite;
        }
        
        // 切换颜色
        spriteRenderer.color = activated ? activatedColor : deactivatedColor;
    }
    
    // 编辑器可视化
    private void OnDrawGizmosSelected()
    {
        // 绘制触发区域
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Gizmos.color = IsActivated ? Color.green : Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(collider.offset, collider.size);
        }
        
        // 绘制连接线
        Gizmos.color = Color.cyan;
        foreach (var target in targetMechanisms)
        {
            if (target != null)
            {
                Gizmos.DrawLine(transform.position, target.transform.position);
            }
        }
    }
}
