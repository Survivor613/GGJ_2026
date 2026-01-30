using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using DialogueSystem.Data;

/// <summary>
/// 为原生 Unity Text 切换到中文模式
/// 菜单：Tools/Dialogue/Switch to Chinese (Unity Text)
/// </summary>
public class SwitchToChineseForUnityText : EditorWindow
{
    [MenuItem("Tools/Dialogue/Switch to Chinese (Unity Text) 🇨🇳")]
    static void SwitchToChineseMode()
    {
        Debug.Log("<color=cyan>========== 切换到中文模式（Unity Text）==========</color>");
        
        // 1. 加载中文测试数据
        var chineseScript = AssetDatabase.LoadAssetAtPath<DialogueScriptSO>("Assets/Resources/TestDialogue.asset");
        if (chineseScript == null)
        {
            Debug.LogWarning("⚠ 未找到中文测试数据，正在创建...");
            DialogueTestDataCreator.CreateTestData();
            chineseScript = AssetDatabase.LoadAssetAtPath<DialogueScriptSO>("Assets/Resources/TestDialogue.asset");
        }
        
        // 2. 切换 DialogueTest 的测试脚本
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
        
        // 3. 检查是否使用的是 Unity Text（原生Text）
        Text[] unityTexts = GameObject.FindObjectsOfType<Text>(true);
        bool usingUnityText = false;
        
        foreach (var text in unityTexts)
        {
            if (text.gameObject.name == "NameText" || text.gameObject.name == "BodyText")
            {
                usingUnityText = true;
                Debug.Log($"✓ 检测到 Unity Text 组件: {text.gameObject.name}");
            }
        }
        
        if (!usingUnityText)
        {
            Debug.LogWarning("⚠ 未检测到 Unity Text 组件，可能还在使用 TextMeshPro");
            EditorUtility.DisplayDialog("提示", 
                "未检测到 Unity Text 组件。\n\n如果你还在使用 TextMeshPro，请先运行：\nTools → Dialogue → Convert to Unity Text", 
                "确定");
            return;
        }
        
        // 4. 保存场景
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("<color=green>========== ✓ 切换完成！现在按 Play 测试中文对话！ ==========</color>");
        
        EditorUtility.DisplayDialog("切换成功", 
            "已切换到中文模式！\n\nUnity Text 会自动使用系统字体显示中文。\n\n现在按 Play 按钮，应该能看到清晰的中文对话了！\n\n对话内容：\n- 你好！欢迎来到对话系统测试。\n- 我可以说话很快！或者说得很慢...\n- 这是一段旁白文字。", 
            "确定");
    }
}
