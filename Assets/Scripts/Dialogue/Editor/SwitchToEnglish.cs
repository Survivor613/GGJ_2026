using UnityEngine;
using UnityEditor;
using TMPro;
using DialogueSystem.Data;

/// <summary>
/// 一键切换到英文模式 - 自动配置所有设置
/// 菜单：Tools/Dialogue/Switch to English Mode (一键切换英文)
/// </summary>
public class SwitchToEnglish : EditorWindow
{
    // [MenuItem("Tools/Dialogue/Switch to English Mode (一键切换英文) 🔄")] // 已禁用：英文测试用，不常用
    static void SwitchToEnglishMode()
    {
        Debug.Log("<color=cyan>========== 切换到英文模式 ==========</color>");
        
        // 1. 确保英文测试数据存在
        var englishScript = AssetDatabase.LoadAssetAtPath<DialogueScriptSO>("Assets/Resources/TestDialogue_English.asset");
        if (englishScript == null)
        {
            Debug.Log("未找到英文测试数据，正在创建...");
            CreateEnglishTestData.CreateTestData();
            englishScript = AssetDatabase.LoadAssetAtPath<DialogueScriptSO>("Assets/Resources/TestDialogue_English.asset");
        }
        
        // 2. 切换 DialogueTest 的测试脚本
        DialogueTest test = GameObject.FindObjectOfType<DialogueTest>();
        if (test != null)
        {
            SerializedObject so = new SerializedObject(test);
            so.FindProperty("testScript").objectReferenceValue = englishScript;
            so.ApplyModifiedProperties();
            Debug.Log("✓ 切换测试脚本为英文版");
        }
        else
        {
            Debug.LogWarning("⚠ 场景中未找到 DialogueTest 组件，请先运行 Auto Setup Scene");
        }
        
        // 3. 切换所有 TMP 组件为默认字体
        TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        TMP_Text[] allTexts = GameObject.FindObjectsOfType<TMP_Text>(true);
        
        int count = 0;
        foreach (var text in allTexts)
        {
            // 只修改对话相关的组件，不影响其他 UI
            if (text.gameObject.name == "NameText" || text.gameObject.name == "BodyText")
            {
                text.font = defaultFont;
                EditorUtility.SetDirty(text);
                count++;
            }
        }
        
        Debug.Log($"✓ 已为 {count} 个组件切换为默认英文字体");
        
        // 4. 保存场景
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("<color=green>========== ✓ 切换完成！现在按 Play 测试！ ==========</color>");
        
        EditorUtility.DisplayDialog("切换成功", 
            "已切换到英文模式！\n\n现在按 Play 按钮，对话应该清晰显示了！\n\n对话内容：\n- Hello! Welcome to the dialogue system test.\n- I can speak very fast! Or very slowly...\n- This is a narrator text.", 
            "确定");
    }
}
