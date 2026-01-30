using UnityEngine;
using UnityEditor;
using DialogueSystem.Data;

/// <summary>
/// Unity Editor 工具：快速创建测试对话数据
/// 菜单：Tools/Dialogue/Create Test Data
/// </summary>
public class DialogueTestDataCreator : EditorWindow
{
    [MenuItem("Tools/Dialogue/Create Test Data")]
    public static void CreateTestData()
    {
        // 1. 创建测试对话脚本
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
            speakerName = "爱丽丝",
            text = "你好！[pause=0.3]欢迎来到对话系统测试。",
            nextId = "line_2"
        };
        
        // 添加对话节点2：带特效
        var line2 = new LineNode
        {
            id = "line_2",
            speakerId = "alice",
            speakerName = "爱丽丝",
            text = "我可以说话[shake=2]很快！[/shake][pause=0.5]或者[spd=0.1]说得很慢...[/spd]",
            nextId = "line_3"
        };
        
        // 添加对话节点3：旁白
        var line3 = new LineNode
        {
            id = "line_3",
            speakerId = "",
            speakerName = "旁白",
            text = "这是一段旁白文字，没有角色高亮。",
            nextId = ""
        };
        
        dialogueScript.nodes.Add(cmd1);
        dialogueScript.nodes.Add(line1);
        dialogueScript.nodes.Add(line2);
        dialogueScript.nodes.Add(line3);
        
        // 保存到 Resources 文件夹
        string path = "Assets/Resources/TestDialogue.asset";
        AssetDatabase.CreateAsset(dialogueScript, path);
        
        Debug.Log($"<color=green>✓ 创建测试对话脚本: {path}</color>");
        
        // 2. 创建测试角色定义（可选，如果用户有立绘图片）
        var actorDef = ScriptableObject.CreateInstance<ActorDefinitionSO>();
        actorDef.actorId = "alice";
        actorDef.displayName = "爱丽丝";
        
        string actorPath = "Assets/Resources/Actor_Alice.asset";
        AssetDatabase.CreateAsset(actorDef, actorPath);
        
        Debug.Log($"<color=green>✓ 创建角色定义: {actorPath}</color>");
        Debug.Log("<color=yellow>请在 Inspector 中为 Actor_Alice 添加立绘 Sprite！</color>");
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // 选中创建的对话脚本
        Selection.activeObject = dialogueScript;
        EditorGUIUtility.PingObject(dialogueScript);
    }
    
    // [MenuItem("Tools/Dialogue/Open Dialogue Folder")] // 已禁用：不常用
    static void OpenDialogueFolder()
    {
        string path = "Assets/Scripts/Dialogue";
        Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
        Selection.activeObject = obj;
        EditorGUIUtility.PingObject(obj);
    }
}
