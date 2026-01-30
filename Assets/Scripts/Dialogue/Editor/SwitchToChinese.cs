using UnityEngine;
using UnityEditor;
using TMPro;
using DialogueSystem.Data;

/// <summary>
/// 一键切换到中文模式 - 自动配置所有设置
/// 菜单：Tools/Dialogue/Switch to Chinese Mode (切换中文)
/// </summary>
public class SwitchToChinese : EditorWindow
{
    // [MenuItem("Tools/Dialogue/Switch to Chinese Mode (切换中文) 🇨🇳")] // 已禁用：TMP版本，请使用 Switch to Chinese (Unity Text)
    static void SwitchToChineseMode()
    {
        Debug.Log("<color=cyan>========== 切换到中文模式 ==========</color>");
        
        // 1. 加载中文测试数据
        var chineseScript = AssetDatabase.LoadAssetAtPath<DialogueScriptSO>("Assets/Resources/TestDialogue.asset");
        if (chineseScript == null)
        {
            Debug.LogWarning("⚠ 未找到中文测试数据，请先运行 Tools → Dialogue → Create Test Data");
            EditorUtility.DisplayDialog("警告", 
                "未找到中文测试数据！\n\n请先运行：\nTools → Dialogue → Create Test Data", 
                "确定");
            return;
        }
        
        // 2. 加载中文字体
        TMP_FontAsset chineseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/ChineseFont SDF.asset");
        if (chineseFont == null)
        {
            Debug.LogError("❌ 未找到中文字体资源！");
            EditorUtility.DisplayDialog("错误", 
                "未找到中文字体资源！\n\n请确保已创建：\nAssets/Resources/ChineseFont SDF.asset\n\n参考说明文档创建中文字体。", 
                "确定");
            return;
        }
        
        // 3. 切换 DialogueTest 的测试脚本
        DialogueTest test = GameObject.FindObjectOfType<DialogueTest>();
        if (test != null)
        {
            SerializedObject so = new SerializedObject(test);
            so.FindProperty("testScript").objectReferenceValue = chineseScript;
            so.ApplyModifiedProperties();
            Debug.Log("✓ 切换测试脚本为中文版");
        }
        else
        {
            Debug.LogWarning("⚠ 场景中未找到 DialogueTest 组件");
        }
        
        // 4. 切换所有对话相关 TMP 组件为中文字体
        TMP_Text[] allTexts = GameObject.FindObjectsOfType<TMP_Text>(true);
        
        int count = 0;
        foreach (var text in allTexts)
        {
            // 只修改对话相关的组件
            if (text.gameObject.name == "NameText" || text.gameObject.name == "BodyText")
            {
                text.font = chineseFont;
                EditorUtility.SetDirty(text);
                count++;
                Debug.Log($"  → 为 {text.gameObject.name} 应用中文字体");
            }
        }
        
        Debug.Log($"✓ 已为 {count} 个组件切换为中文字体");
        
        // 5. 保存场景
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("<color=green>========== ✓ 切换完成！现在按 Play 测试中文对话！ ==========</color>");
        
        EditorUtility.DisplayDialog("切换成功", 
            "已切换到中文模式！\n\n现在按 Play 按钮，应该能看到清晰的中文对话了！\n\n对话内容：\n- 你好！欢迎来到对话系统测试。\n- 我可以说话很快！或者说得很慢...\n- 这是一段旁白文字。", 
            "确定");
    }
}
