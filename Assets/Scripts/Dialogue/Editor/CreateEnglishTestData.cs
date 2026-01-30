using UnityEngine;
using UnityEditor;
using DialogueSystem.Data;

/// <summary>
/// 创建英文测试数据 - 使用默认字体可以清晰显示
/// 菜单：Tools/Dialogue/Create English Test Data
/// </summary>
public class CreateEnglishTestData : EditorWindow
{
    // [MenuItem("Tools/Dialogue/Create English Test Data (清晰版)")] // 已禁用：英文测试用，不常用
    public static void CreateTestData()
    {
        // 创建测试对话脚本
        var dialogueScript = ScriptableObject.CreateInstance<DialogueScriptSO>();
        
        // 添加命令节点：显示角色
        var cmd1 = new CommandNode
        {
            id = "cmd_show_alice",
            command = "actor show id=alice portrait=default x=-200 y=0",
            nextId = "line_1"
        };
        
        // 添加对话节点1
        var line1 = new LineNode
        {
            id = "line_1",
            speakerId = "alice",
            speakerName = "Alice",
            text = "Hello![pause=0.3] Welcome to the dialogue system test.",
            nextId = "line_2"
        };
        
        // 添加对话节点2：带特效
        var line2 = new LineNode
        {
            id = "line_2",
            speakerId = "alice",
            speakerName = "Alice",
            text = "I can speak [shake=2]very fast![/shake][pause=0.5] Or [spd=0.1]very slowly...[/spd]",
            nextId = "line_3"
        };
        
        // 添加对话节点3：旁白
        var line3 = new LineNode
        {
            id = "line_3",
            speakerId = "",
            speakerName = "Narrator",
            text = "This is a narrator text without character highlight.",
            nextId = ""
        };
        
        dialogueScript.nodes.Add(cmd1);
        dialogueScript.nodes.Add(line1);
        dialogueScript.nodes.Add(line2);
        dialogueScript.nodes.Add(line3);
        
        // 保存到 Resources 文件夹
        string path = "Assets/Resources/TestDialogue_English.asset";
        AssetDatabase.CreateAsset(dialogueScript, path);
        
        Debug.Log($"<color=green>✓ 创建英文测试对话脚本: {path}</color>");
        
        // 创建角色定义
        var actorDef = ScriptableObject.CreateInstance<ActorDefinitionSO>();
        actorDef.actorId = "alice";
        actorDef.displayName = "Alice";
        
        string actorPath = "Assets/Resources/Actor_Alice_English.asset";
        AssetDatabase.CreateAsset(actorDef, actorPath);
        
        Debug.Log($"<color=green>✓ 创建角色定义: {actorPath}</color>");
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // 选中创建的对话脚本
        Selection.activeObject = dialogueScript;
        EditorGUIUtility.PingObject(dialogueScript);
        
        EditorUtility.DisplayDialog("成功", 
            "英文测试数据创建完成！\n\n请在 DialogueSystem 物体上的 DialogueTest 组件中，\n将 Test Script 改为：TestDialogue_English\n\n然后按 Play 测试！", 
            "确定");
    }
}
