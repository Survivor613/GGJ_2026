using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 修复原生 Unity Text 设置问题
/// 菜单：Tools/Dialogue/Fix Unity Text Setup
/// </summary>
public class FixUnityTextSetup : EditorWindow
{
    [MenuItem("Tools/Dialogue/Fix Unity Text Setup (修复设置) 🔧")]
    static void FixSetup()
    {
        Debug.Log("<color=cyan>========== 修复原生 Text 设置 ==========</color>");
        
        int fixedCount = 0;
        
        // 1. 禁用所有 TextEffectController
        var effectControllers = GameObject.FindObjectsOfType<DialogueSystem.Effects.TextEffectController>(true);
        foreach (var controller in effectControllers)
        {
            if (controller.enabled)
            {
                controller.enabled = false;
                EditorUtility.SetDirty(controller);
                fixedCount++;
                Debug.Log($"✓ 禁用 TextEffectController 在 {controller.gameObject.name}");
            }
        }
        
        // 2. 检查是否有 TMP 组件残留
        TMP_Text[] tmpTexts = GameObject.FindObjectsOfType<TMP_Text>(true);
        foreach (var tmp in tmpTexts)
        {
            if (tmp.gameObject.name == "NameText" || tmp.gameObject.name == "BodyText")
            {
                Debug.LogWarning($"⚠ 发现 TMP 组件残留在 {tmp.gameObject.name}，建议删除并重新搭建");
            }
        }
        
        // 3. 确保 Unity Text 组件存在
        Text[] unityTexts = GameObject.FindObjectsOfType<Text>(true);
        bool foundNameText = false;
        bool foundBodyText = false;
        
        foreach (var text in unityTexts)
        {
            if (text.gameObject.name == "NameText") foundNameText = true;
            if (text.gameObject.name == "BodyText") foundBodyText = true;
        }
        
        if (!foundNameText || !foundBodyText)
        {
            Debug.LogWarning("⚠ 未找到完整的 Unity Text 组件，可能需要重新运行 Auto Setup Scene");
        }
        else
        {
            Debug.Log("✓ Unity Text 组件检查通过");
        }
        
        // 4. 保存场景
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        if (fixedCount > 0)
        {
            Debug.Log($"<color=green>✓ 修复完成！共处理 {fixedCount} 个问题</color>");
            EditorUtility.DisplayDialog("修复完成", 
                $"已禁用 {fixedCount} 个不兼容的组件。\n\n现在可以测试对话系统了！", 
                "确定");
        }
        else
        {
            Debug.Log("<color=green>✓ 检查完成，未发现问题</color>");
            EditorUtility.DisplayDialog("检查完成", 
                "未发现需要修复的问题。\n\n如果仍有错误，建议重新执行完整设置步骤。", 
                "确定");
        }
    }
}
