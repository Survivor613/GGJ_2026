using UnityEngine;

/// <summary>
/// 可激活接口：所有接收触发信号的机关必须实现此接口
/// 用于解耦 TriggerButton 和具体机关逻辑
/// </summary>
public interface IActivatable
{
    /// <summary>
    /// 激活机关（计数器从0变为1时调用）
    /// </summary>
    void Activate();
    
    /// <summary>
    /// 停用机关（计数器从1变为0时调用）
    /// </summary>
    void Deactivate();
    
    /// <summary>
    /// 机关是否处于激活状态
    /// </summary>
    bool IsActivated { get; }
}
