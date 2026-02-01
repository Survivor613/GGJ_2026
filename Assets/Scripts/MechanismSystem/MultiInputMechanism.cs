using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 多输入机关：支持"多对一"逻辑（必须激活N个按钮才能触发）
/// 使用方式：将多个TriggerButton连接到此组件，此组件再连接到实际机关
/// </summary>
public class MultiInputMechanism : MonoBehaviour, IActivatable
{
    [Header("Input Buttons")]
    [Tooltip("所有输入按钮（必须全部激活才能触发）")]
    [SerializeField] private List<TriggerButton> inputButtons = new List<TriggerButton>();
    
    [Header("Target Mechanisms")]
    [Tooltip("要控制的目标机关（可以是MovingPlatform等）")]
    [SerializeField] private List<GameObject> targetMechanisms = new List<GameObject>();
    
    [Header("Logic Type")]
    [Tooltip("逻辑类型：AND=全部激活，OR=任意一个激活")]
    [SerializeField] private LogicType logicType = LogicType.AND;
    
    public enum LogicType
    {
        AND,  // 逻辑与：所有按钮都激活
        OR    // 逻辑或：任意按钮激活
    }
    
    // 缓存目标机关
    private List<IActivatable> cachedActivatables = new List<IActivatable>();
    
    // 当前激活状态
    private bool isActivated = false;
    public bool IsActivated => isActivated;
    
    private void Awake()
    {
        CacheActivatables();
        RegisterInputCallbacks();
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
        }
    }
    
    /// <summary>
    /// 注册输入按钮的回调
    /// </summary>
    private void RegisterInputCallbacks()
    {
        foreach (var button in inputButtons)
        {
            if (button == null) continue;
            
            // 监听按钮的激活/停用事件
            button.onActivate.AddListener(OnInputChanged);
            button.onDeactivate.AddListener(OnInputChanged);
        }
    }
    
    /// <summary>
    /// 输入状态改变时的回调
    /// </summary>
    private void OnInputChanged()
    {
        bool shouldActivate = EvaluateLogic();
        
        if (shouldActivate && !isActivated)
        {
            Activate();
        }
        else if (!shouldActivate && isActivated)
        {
            Deactivate();
        }
    }
    
    /// <summary>
    /// 评估逻辑条件
    /// </summary>
    private bool EvaluateLogic()
    {
        if (inputButtons.Count == 0) return false;
        
        if (logicType == LogicType.AND)
        {
            // AND逻辑：所有按钮都必须激活
            foreach (var button in inputButtons)
            {
                if (button == null || !button.IsActivated)
                    return false;
            }
            return true;
        }
        else // OR逻辑
        {
            // OR逻辑：任意一个按钮激活即可
            foreach (var button in inputButtons)
            {
                if (button != null && button.IsActivated)
                    return true;
            }
            return false;
        }
    }
    
    public void Activate()
    {
        isActivated = true;
        Debug.Log($"[MultiInputMechanism] {gameObject.name} 激活！逻辑类型: {logicType}");
        
        foreach (var activatable in cachedActivatables)
        {
            activatable.Activate();
        }
    }
    
    public void Deactivate()
    {
        isActivated = false;
        Debug.Log($"[MultiInputMechanism] {gameObject.name} 停用！");
        
        foreach (var activatable in cachedActivatables)
        {
            activatable.Deactivate();
        }
    }
    
    private void OnDestroy()
    {
        // 清理回调
        foreach (var button in inputButtons)
        {
            if (button != null)
            {
                button.onActivate.RemoveListener(OnInputChanged);
                button.onDeactivate.RemoveListener(OnInputChanged);
            }
        }
    }
    
    // 编辑器可视化
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        
        // 绘制输入连接线
        foreach (var button in inputButtons)
        {
            if (button != null)
            {
                Gizmos.DrawLine(button.transform.position, transform.position);
            }
        }
        
        // 绘制输出连接线
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
