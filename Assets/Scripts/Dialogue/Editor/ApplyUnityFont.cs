using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// 为原生 Unity Text 应用中文字体
/// 菜单：Tools/Dialogue/Apply Unity Font (应用原生字体)
/// </summary>
public class ApplyUnityFont : EditorWindow
{
    private Font selectedFont;

    [MenuItem("Tools/Dialogue/Apply Unity Font (应用原生字体) 🔤")]
    static void ShowWindow()
    {
        GetWindow<ApplyUnityFont>("应用原生字体");
    }

    void OnGUI()
    {
        GUILayout.Label("为对话系统应用原生字体", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("1. 将思源字体 .ttf 文件拖入 Assets/Fonts/\n2. 在下方选择该字体\n3. 点击应用按钮", MessageType.Info);
        GUILayout.Space(10);
        
        selectedFont = (Font)EditorGUILayout.ObjectField("选择字体", selectedFont, typeof(Font), false);
        
        GUILayout.Space(10);
        
        if (selectedFont == null)
        {
            EditorGUILayout.HelpBox("请先选择一个字体文件（.ttf）", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox($"当前选择：{selectedFont.name}", MessageType.None);
        }
        
        GUILayout.Space(10);
        
        GUI.enabled = selectedFont != null;
        if (GUILayout.Button("应用到对话系统", GUILayout.Height(40)))
        {
            ApplyFont();
        }
        GUI.enabled = true;
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("查找项目中的所有字体"))
        {
            ListAllFonts();
        }
    }

    void ApplyFont()
    {
        Debug.Log($"<color=cyan>应用字体: {selectedFont.name}</color>");
        
        Text[] allTexts = GameObject.FindObjectsOfType<Text>(true);
        int count = 0;
        
        foreach (var text in allTexts)
        {
            if (text.gameObject.name == "NameText" || text.gameObject.name == "BodyText")
            {
                text.font = selectedFont;
                EditorUtility.SetDirty(text);
                count++;
                Debug.Log($"  → 为 {text.gameObject.name} 应用字体");
            }
        }
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log($"<color=green>✓ 完成！已为 {count} 个文本组件应用字体</color>");
        
        EditorUtility.DisplayDialog("应用成功", 
            $"已成功应用字体：{selectedFont.name}\n\n共更新 {count} 个文本组件。\n\n现在按 Play 测试中文显示！", 
            "确定");
    }

    void ListAllFonts()
    {
        Debug.Log("<color=cyan>========== 项目中的所有字体 ==========</color>");
        
        string[] guids = AssetDatabase.FindAssets("t:Font");
        
        if (guids.Length == 0)
        {
            Debug.Log("未找到任何字体文件");
            EditorUtility.DisplayDialog("查找结果", "项目中没有找到任何字体文件（.ttf）", "确定");
            return;
        }
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Font font = AssetDatabase.LoadAssetAtPath<Font>(path);
            if (font != null)
            {
                Debug.Log($"  - {font.name} ({path})");
            }
        }
        
        Debug.Log($"<color=green>共找到 {guids.Length} 个字体文件</color>");
        EditorUtility.DisplayDialog("查找结果", $"找到 {guids.Length} 个字体文件，详情请查看 Console", "确定");
    }
}
