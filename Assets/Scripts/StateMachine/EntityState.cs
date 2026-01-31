using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityState
{
    protected StateMachine stateMachine;
    protected string animBoolName;

    protected Animator anim;
    protected Rigidbody2D rb;

    protected float stateTimer;
    protected bool triggerCalled;

    public EntityState(StateMachine stateMachine, string animBoolName)
    {
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
    }

    public virtual void Enter()
    {
        SafeSetBool(animBoolName, true);
        // anim.SetBool只算对anim的访问，因为访问是public方法
        triggerCalled = false;
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
        updateAnimationParameters();
    }

    public virtual void Exit()
    {
        SafeSetBool(animBoolName, false);
    }

    public void AnimationTrigger()
    {
        triggerCalled = true;
    }

    public virtual void updateAnimationParameters()
    {

    }

    // ==================== 安全设置 Animator 参数（避免狐狸等不同动画控制器报错）====================
    
    /// <summary>
    /// 安全设置 Float 参数（如果参数不存在则跳过）
    /// </summary>
    protected void SafeSetFloat(string paramName, float value)
    {
        if (anim == null) return;
        
        // 检查参数是否存在
        foreach (var param in anim.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Float)
            {
                anim.SetFloat(paramName, value);
                return;
            }
        }
        
        // 参数不存在时不报错，只输出警告（可选）
        // Debug.LogWarning($"Animator parameter '{paramName}' not found in {anim.runtimeAnimatorController.name}");
    }
    
    /// <summary>
    /// 安全设置 Bool 参数
    /// </summary>
    protected void SafeSetBool(string paramName, bool value)
    {
        if (anim == null) return;
        
        foreach (var param in anim.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
            {
                anim.SetBool(paramName, value);
                return;
            }
        }
    }
    
    /// <summary>
    /// 安全设置 Trigger 参数
    /// </summary>
    protected void SafeSetTrigger(string paramName)
    {
        if (anim == null) return;
        
        foreach (var param in anim.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Trigger)
            {
                anim.SetTrigger(paramName);
                return;
            }
        }
    }
    
    /// <summary>
    /// 安全设置 Integer 参数
    /// </summary>
    protected void SafeSetInteger(string paramName, int value)
    {
        if (anim == null) return;
        
        foreach (var param in anim.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Int)
            {
                anim.SetInteger(paramName, value);
                return;
            }
        }
    }
}
