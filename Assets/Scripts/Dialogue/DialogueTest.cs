using UnityEngine;
using DialogueSystem.Core;
using DialogueSystem.Data;

/// <summary>
/// 对话系统测试脚本
/// 用于快速启动对话和测试功能
/// </summary>
public class DialogueTest : MonoBehaviour
{
    [Header("测试对话脚本")]
    [SerializeField] private DialogueScriptSO testScript;
    
    [Header("自动启动")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private float startDelay = 0.5f;
    
    [Header("调试快捷键")]
    [SerializeField] private KeyCode historyToggleKey = KeyCode.H;
    [SerializeField] private KeyCode restartKey = KeyCode.R;

    private DialogueSystem.UI.HistoryView cachedHistoryView;

    private void Start()
    {
        CacheHistoryView();

        if (autoStart && testScript != null)
        {
            Invoke(nameof(StartTestDialogue), startDelay);
        }
    }

    private void Update()
    {
        // 快捷键：H 打开/关闭历史面板
        if (Input.GetKeyDown(historyToggleKey))
        {
            if (cachedHistoryView == null)
            {
                CacheHistoryView();
            }

            if (cachedHistoryView != null)
            {
                // Toggle between showing and hiding
                bool isActive = cachedHistoryView.gameObject.activeSelf;
                cachedHistoryView.Toggle(!isActive);
                Debug.Log($"历史面板切换: {!isActive}");
            }
            else
            {
                Debug.LogWarning("场景中未找到 HistoryView 组件！");
            }
        }

        // 快捷键：R 重新开始对话
        if (Input.GetKeyDown(restartKey))
        {
            StartTestDialogue();
        }
    }

    public void StartTestDialogue()
    {
        if (DialogueRunner.Instance != null && testScript != null)
        {
            Debug.Log($"<color=green>启动测试对话: {testScript.name}</color>");
            DialogueRunner.Instance.StartDialogue(testScript);
        }
        else
        {
            if (DialogueRunner.Instance == null)
                Debug.LogError("场景中未找到 DialogueRunner！");
            if (testScript == null)
                Debug.LogError("未指定测试对话脚本！");
        }
    }

    private void CacheHistoryView()
    {
        // HistoryPanel 默认是隐藏的，需要包含未激活对象
        cachedHistoryView = FindObjectOfType<DialogueSystem.UI.HistoryView>(true);
    }

    private void OnGUI()
    {
        // 简单的调试 GUI
        GUILayout.BeginArea(new Rect(10, 10, 250, 150));
        GUILayout.Box("对话系统调试工具");
        
        if (GUILayout.Button("开始对话"))
        {
            StartTestDialogue();
        }
        
        GUILayout.Label($"快捷键 [{historyToggleKey}]: 历史面板");
        GUILayout.Label($"快捷键 [{restartKey}]: 重新开始");
        GUILayout.Label($"鼠标点击: 推进/跳过");
        
        GUILayout.EndArea();
    }
}
