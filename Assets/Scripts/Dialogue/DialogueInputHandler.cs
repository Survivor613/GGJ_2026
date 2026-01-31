using UnityEngine;
using DialogueSystem.Core;

/// <summary>
/// 对话输入处理器
/// 处理鼠标点击和键盘输入来推进对话
/// </summary>
public class DialogueInputHandler : MonoBehaviour
{
    [Header("输入设置")]
    [SerializeField] private KeyCode advanceKey = KeyCode.Space;
    [SerializeField] private bool enableMouseClick = true;
    [SerializeField] private DialogueSystem.UI.HistoryView historyView;

    private void Awake()
    {
        if (historyView == null)
        {
            historyView = FindObjectOfType<DialogueSystem.UI.HistoryView>(true);
        }
    }
    
    private void Update()
    {
        if (DialogueRunner.Instance == null) return;

        // 历史面板打开时，屏蔽对话推进输入
        if (historyView != null && historyView.IsOpen)
        {
            return;
        }
        
        bool shouldAdvance = false;
        
        // 检测鼠标点击
        if (enableMouseClick && Input.GetMouseButtonDown(0))
        {
            shouldAdvance = true;
        }
        
        // 检测键盘按键
        if (Input.GetKeyDown(advanceKey))
        {
            shouldAdvance = true;
        }
        
        // 推进对话
        if (shouldAdvance)
        {
            DialogueRunner.Instance.OnClickArea();
        }
    }
}
